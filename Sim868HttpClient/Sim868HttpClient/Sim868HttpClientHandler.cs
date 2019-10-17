using System;
using System.IO.Ports;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sim868HttpClient
{
    public class Sim868HttpClientHandler : HttpClientHandler
    {
        SerialPort _serialPort = new SerialPort();

        public Sim868HttpClientHandler()
        {
            _serialPort.PortName = "COM4"; // "/dev/ttyS0";
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;

            _serialPort.Open();

            string response = string.Empty;

            _serialPort.WriteLine("AT+SAPBR=3,1,\"Contype\",\"GPRS\"");
            AssertOk();
            _serialPort.WriteLine("AT+SAPBR=3,1,\"APN\",\"wholesale\"");
            AssertOk();
            //_serialPort.WriteLine("AT+SAPBR=1,1");
            //AssertOk();
        }

        ~Sim868HttpClientHandler()
        {
            _serialPort.Close();
        }

        private void AssertOk()
        {
            _serialPort.ReadLine();
            string response = _serialPort.ReadLine();
            if (response != "OK\r") { throw new InvalidOperationException(); }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //_serialPort.WriteLine("AT+HTTPINIT");
            //AssertOk();
            _serialPort.WriteLine("AT+HTTPPARA=\"CID\",1");
            AssertOk();
            _serialPort.WriteLine($"AT+HTTPPARA=\"URL\",\"{request.RequestUri.AbsoluteUri}\"");
            AssertOk();
            _serialPort.WriteLine("AT+HTTPACTION=0");
            AssertOk();

            Thread.Sleep(2000);

            string output1 = _serialPort.ReadLine();
            string output2 = _serialPort.ReadLine();

            _serialPort.WriteLine("AT+HTTPREAD");
            Thread.Sleep(2000);
            string output3 = _serialPort.ReadLine();
            string output4 = _serialPort.ReadLine();
            string output5 = _serialPort.ReadLine();

            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(output5),
            };

            responseMessage.Headers.Add("Testing", "Test");

            return responseMessage;
        }
    }
}