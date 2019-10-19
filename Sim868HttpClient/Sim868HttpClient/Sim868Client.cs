using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

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
        StringBuilder _buffer = new StringBuilder();
        bool _dataAvailable = false;

        public Sim868Client(SerialPort serialPort)
        {
            _serialPort = serialPort;

            _serialPort.PortName = "COM4"; // "/dev/ttyS0";
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;

            _serialPort.Open();

            _serialPort.DataReceived += DataReceived;

            // Turn echo off
            _serialPort.WriteLine("ATE0");
            string response = GetCommandResponse();
            if(response == "ATE0")
            {
                AssertOk();
            }            
        }

        internal void ClearBuffer()
        {
            lock (this)
            {
                _buffer.Clear();
                SanitizeNewLinesFromBuffer();
            }
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (this)
            {
                _buffer.Append(_serialPort.ReadExisting());
                SanitizeNewLinesFromBuffer();
            }
        }

        ~Sim868Client()
        {
            _serialPort.Close();
        }

        public (int bearerId, BearerStatus status, string ipAddress) QueryBearerStatus(int bearerId)
        {
            // Query, this returns: +SAPBR: <cid>,<Status>,<IP_Addr> 
            // 1,1,xxx = connected
            // 1,3,xxx = closed
            _serialPort.WriteLine($"AT+SAPBR=2,{bearerId}");

            string response = GetCommandResponse();
            
            if(response == "error")
            {
                return (bearerId, BearerStatus.Closed, string.Empty);
            }
            else if(response == "OK")
            {

            }
            
            var commandParser = new Regex(@"\+SAPBR: (\d),(\d),\""([\d\.]*)\""");

            var match = commandParser.Match(response);

            var retBearerId = int.Parse(match.Groups[1].Value);
            var retStatus = (BearerStatus) int.Parse(match.Groups[2].Value);
            var retIpAddress = match.Groups[3].Value;

            return (retBearerId, retStatus, retIpAddress);
        }

        public void SetBearerParam(int bearerId, string paramName, string paramValue)
        {
            _serialPort.WriteLine($"AT+SAPBR=3,{bearerId},\"{paramName}\",\"{paramValue}\"");
            AssertOk();
        }

        public void OpenBearer(int bearerId)
        {
            _serialPort.WriteLine($"AT+SAPBR=1,{bearerId}");
            AssertOk();
        }

        public void CloseBearer(int bearerId)
        {
            _serialPort.WriteLine($"AT+SAPBR=0,{bearerId}");
            AssertOk();
        }

        public void InitHttp()
        {
            _serialPort.WriteLine("AT+HTTPINIT");
            AssertOk();
        }

        public void SetHttpParam(string paramName, int paramValue)
        {
            _serialPort.WriteLine($"AT+HTTPPARA=\"{paramName}\",{paramValue}");
            AssertOk();
        }

        public void SetHttpParam(string paramName, string paramValue)
        {
            _serialPort.WriteLine($"AT+HTTPPARA=\"{paramName}\",\"{paramValue}\"");
            AssertOk();            
        }

        public (HttpMethod method, int statusCode, int dataLength) ExecuteHttpAction(HttpMethod method)
        {
            _serialPort.WriteLine($"AT+HTTPACTION={(int)method}");
            string response1 = GetCommandResponse();
            string response2 = GetCommandResponse();
            string response3 = GetCommandResponse();

            var commandParser = new Regex(@"\+HTTPACTION: (\d),(\d\d\d),(\d*)");

            var match = commandParser.Match(response3);

            var retMethod = (HttpMethod)int.Parse(match.Groups[1].Value);
            var retStatus = int.Parse(match.Groups[2].Value);
            var retDataLength = int.Parse(match.Groups[3].Value);

            return (retMethod, retStatus, retDataLength);
        }

        public string ReadHttpResponse()
        {
            _serialPort.WriteLine($"AT+HTTPREAD");

            string response1 = GetCommandResponse();
            string response2 = GetCommandResponse();
            string response3 = GetCommandResponse();

            return response2;
        }

        private void AssertOk()
        {
            string response = GetCommandResponse();
            if (response != "OK") { throw new InvalidOperationException(); }
        }

        private string GetCommandResponse()
        {
            Thread.Sleep(10);

            while (!_dataAvailable)
            {
                Thread.Sleep(10);
            }

            lock (this)
            {
                int indexOf = _buffer.ToString().IndexOf("\r\n");
                string response = _buffer.ToString(0, indexOf);
                _buffer.Remove(0, indexOf + 2);

                // Ignore these messages.
                if (response == "SMS Ready" || response == "Call Ready" || response == string.Empty)
                {
                    response = GetCommandResponse();
                }

                SanitizeNewLinesFromBuffer();

                return response;
            }
        }

        private void SanitizeNewLinesFromBuffer()
        {
            if (_buffer.Length > 0)
            {
                // Remove empty lines
                if (_buffer.ToString(0, 2) == "\r\n")
                {
                    _buffer.Remove(0, 2);
                }
            }
            _dataAvailable = _buffer.ToString().Contains("\r\n");
        }
    }
}
