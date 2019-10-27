using NaoVictoria.Sim868Driver.Models;
using Sim868HttpClient.Helpers;
using System;
using System.Device.Gpio;
using System.Globalization;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NaoVictoria.Sim868Driver
{
    public enum BearerStatus
    {
        Connecting = 0,
        Connected = 1,
        Closing = 2,
        Closed = 3
    }

    public enum HttpMethod
    {
        Get = 0,
        Post = 1,
        head = 2
    }

    public class Driver
    {
        public const string BearerParamConnType = "CONTYPE";
        public const string BearerParamApn = "APN";

        public const string HttpParamCid = "CID";
        public const string HttpParamUrl = "URL";
        public const string HttpParamContent = "CONTENT";
        public const string HttpParamUa = "UA";
        public const string HttpParamTimeout = "TIMEOUT";

        SerialPort _serialPort;

        public Driver(SerialPort serialPort, string portName)
        {
            _serialPort = serialPort;

            _serialPort.PortName = portName; // "/dev/ttyS0";
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.NewLine = "\r\n";
        }

        ~Driver()
        {
            _serialPort.Close();
        }

        public async Task TurnOnModuleAsync()
        {
            // Based on: https://www.waveshare.com/w/upload/8/80/GSM_PWRKEY.rar
            // Note: Only works on the Raspberry PI

            GpioController controller = new GpioController(PinNumberingScheme.Board);
            controller.OpenPin(7, PinMode.Output);
            controller.Write(7, PinValue.Low);
            await Task.Delay(TimeSpan.FromSeconds(4));
            controller.Write(7, PinValue.High);
            controller.ClosePin(7);
            // Give it 30 seconds to boot up. It prints out some info that can't be handled atm (`Call Ready` and `SMS Ready`)
            await Task.Delay(TimeSpan.FromSeconds(30));
            _serialPort.Open();
        }
        
        public async Task<(int bearerId, BearerStatus status, string ipAddress)> QueryBearerStatusAsync(int bearerId)
        {
            // Query, this returns: +SAPBR: <cid>,<Status>,<IP_Addr> 
            // 1,1,xxx = connected
            // 1,3,xxx = closed
            string response = await _serialPort.SendCommandAsync($"AT+SAPBR=2,{bearerId}",4, 2);
            var commandParser = new Regex(@"\+SAPBR: (\d),(\d),\""([\d\.]*)\""");
            var match = commandParser.Match(response);
            var retBearerId = int.Parse(match.Groups[1].Value);
            var retStatus = (BearerStatus)int.Parse(match.Groups[2].Value);
            var retIpAddress = match.Groups[3].Value;

            return (retBearerId, retStatus, retIpAddress);
        }

        public async Task<bool> SetBearerParamAsync(int bearerId, string paramName, string paramValue)
        {
            string response = await _serialPort.SendCommandAsync($"AT+SAPBR=3,{bearerId},\"{paramName}\",\"{paramValue}\"", 2, 2);
            return response == "OK";
        }

        public async Task<bool> OpenBearerAsync(int bearerId)
        {
            string response = await _serialPort.SendCommandAsync($"AT+SAPBR=1,{bearerId}", 2, 2);
            return response == "OK";
        }

        public async Task<bool> ExecuteHttpDataAsync(byte[] data, TimeSpan wait)
        {
            string response = await _serialPort.SendCommandAsync($"AT+HTTPDATA={data.Length},{wait.TotalMilliseconds}", 2, 2);
            if(response == "DOWNLOAD")
            {
                await _serialPort.WriteAsync(data, 0, data.Length);
                await _serialPort.ReadLineAsync();
                response = await _serialPort.ReadLineAsync();
                return response == "OK";
            }

            return false;
        }

        public async Task<bool> CloseBearerAsync(int bearerId)
        {
            string response = await _serialPort.SendCommandAsync($"AT+SAPBR=0,{bearerId}", 2, 2);
            return response == "OK";
        }

        public async Task<bool> ActivateHttpModuleAsync()
        {
            string response = await _serialPort.SendCommandAsync("AT+HTTPINIT", 2, 2);
            return response == "OK";
        }

        public async Task<bool> TerminateHttpModuleAsync()
        {
            string response = await _serialPort.SendCommandAsync("AT+HTTPTERM", 2, 2);
            return response == "OK";
        }

        public async Task<bool> ActivateGpsModuleAsync(bool isActivate = true)
        {
            int isActivateCode = isActivate ? 1 : 0;
            string response = await _serialPort.SendCommandAsync($"AT+CGNSPWR={isActivateCode}", 2, 2);
            return response == "OK";
        }

        public async Task<GnssNavInfo> GetGnssNavInfoAsync()
        {
            string response = await _serialPort.SendCommandAsync("AT+CGNSINF", 4, 2);

            if(response != "OK") { 
                return null;  
            }

            // +CGNSINF: 1,1,20191019225524.000,41.679408,-71.159402,62.724,0.00,19.6,2,,0.9,1.4,1.1,,12,13,,,45,,

            var commandParser = new Regex(@"\+CGNSINF: (\d),(\d),([\d\.]*),([\d\.-]*),([\d\.-]*),([\d\.]*),([\d\.]*),([\d\.]*),(\d),,([\d\.]*),([\d\.]*),([\d\.]*),,(\d*),(\d*),(\d*),(\d*),([\d\.]*),([\d\.]*)");
            var match = commandParser.Match(response);

            var gnssRunStatus = int.Parse(match.Groups[1].Value);
            var fixStatus = int.Parse(match.Groups[2].Value);
            var dateTime = DateTime.ParseExact(match.Groups[3].Value, "yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture);
            var latitude = double.Parse(match.Groups[4].Value);
            var longitude = double.Parse(match.Groups[5].Value);
            var mslAltitude = double.Parse(match.Groups[6].Value);
            var speedOverGround = double.Parse(match.Groups[7].Value);
            var courseOverGround = double.Parse(match.Groups[8].Value);
            var fixMode = int.Parse(match.Groups[9].Value);
            // reserve
            var hDop = double.Parse(match.Groups[10].Value);
            var pDop = double.Parse(match.Groups[11].Value);
            var vDop = double.Parse(match.Groups[12].Value);
            // reserve
            int gpsSatInView;
            int.TryParse(match.Groups[13].Value, out gpsSatInView);
            int gnssSatUsed;
            int.TryParse(match.Groups[14].Value, out gnssSatUsed);
            int glonassUsed;
            int.TryParse(match.Groups[15].Value, out glonassUsed);
            int cnoMax;
            int.TryParse(match.Groups[16].Value, out cnoMax);

            double hpa = 0f;
            double vpa = 0f;

            double.TryParse(match.Groups[17].Value, out hpa);
            double.TryParse(match.Groups[18].Value, out vpa);

            var data = new GnssNavInfo()
            {
                GnssRunStatus = gnssRunStatus,
                FixStatus = fixStatus,
                DateTime = dateTime,
                Latitude = latitude,
                Longitude = longitude,
                MslAltitude = mslAltitude,
                SpeedOverGround = speedOverGround,
                CourseOverGround = courseOverGround,
                FixMode = fixMode,
                HDop = hDop,
                PDop = pDop,
                VDop = vDop,
                GpsSatInView = gpsSatInView,
                GnssSatUsed = gnssSatUsed,
                GlonassSatUSed = glonassUsed,
                CnoMax = cnoMax,
                Hpa = hpa,
                Vpa = vpa
            };
           
            return data;
        }

        public async Task<bool> SetHttpParamAsync(string paramName, int paramValue)
        {
            string response = await _serialPort.SendCommandAsync($"AT+HTTPPARA=\"{paramName}\",{paramValue}", 2, 2);
            return response == "OK";
        }

        public async Task<bool> SetHttpParamAsync(string paramName, string paramValue)
        {
            string response = await _serialPort.SendCommandAsync($"AT+HTTPPARA=\"{paramName}\",\"{paramValue}\"", 2, 2);
            return response == "OK";
        }

        public async Task<(HttpMethod method, int statusCode, int dataLength)> ExecuteHttpActionAsync(HttpMethod method)
        {
            string response = await _serialPort.SendCommandAsync($"AT+HTTPACTION={(int)method}", 2, 2);

            if(response == "OK")
            {
                await _serialPort.ReadLineAsync();
                response = await _serialPort.ReadLineAsync();
            }
            else
            {
                throw new InvalidOperationException();
            }

            var commandParser = new Regex(@"\+HTTPACTION: (\d),(\d\d\d),(\d*)");

            var match = commandParser.Match(response);

            var retMethod = (HttpMethod)int.Parse(match.Groups[1].Value);
            var retStatus = int.Parse(match.Groups[2].Value);
            var retDataLength = int.Parse(match.Groups[3].Value);

            return (retMethod, retStatus, retDataLength);
        }

        public async Task<string> ReadHttpResponseAsync()
        {
            return await _serialPort.SendCommandAsync("AT+HTTPREAD", 3, 3);
        }

    }
}
