using Sim868HttpClient.Helpers;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sim868HttpClient
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

    public class Sim868Client
    {
        public const string BearerParamConnType = "CONTYPE";
        public const string BearerParamApn = "APN";

        public const string HttpParamCid = "CID";
        public const string HttpParamUrl = "URL";
        public const string HttpParamUa = "UA";
        public const string HttpParamTimeout = "TIMEOUT";

        SerialPort _serialPort;

        public Sim868Client(SerialPort serialPort)
        {
            _serialPort = serialPort;

            _serialPort.PortName = "COM3"; // "/dev/ttyS0";
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.NewLine = "\r\n";
            _serialPort.Open();
        }

        ~Sim868Client()
        {
            _serialPort.Close();
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

        public async Task<bool> CloseBearerAsync(int bearerId)
        {
            string response = await _serialPort.SendCommandAsync($"AT+SAPBR=0,{bearerId}", 2, 2);
            return response == "OK";
        }

        public async Task<bool> InitHttpAsync()
        {
            string response = await _serialPort.SendCommandAsync("AT+HTTPINIT", 2, 2);
            return response == "OK";
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

        private void AssertOk()
        {
            //string response = GetCommandResponse();
            //if (response != "OK") { throw new InvalidOperationException(); }
        }

    }
}
