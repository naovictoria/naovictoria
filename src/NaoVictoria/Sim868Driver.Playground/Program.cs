using NaoVictoria.Sim868Driver;
using NaoVictoria.Sim868Driver.Gps;
using NaoVictoria.Sim868Driver.Http;
using NaoVictoria.Sim868Driver.Models;
using Sim868HttpClient.Helpers;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sim868HttpClient
{
    class Program
    {
        static string _apn = "wholesale";
        static string _port = "/dev/ttyS0"; //"COM4";

        static Driver _driver;

        static async Task Main(string[] args)
        {
            // Note: Access point name is dependent on the provider.  
            // For example, it's "wholesale" for Ting.
            _driver = new Driver(new System.IO.Ports.SerialPort(), _port, 29);

            // Turn on and connect via serial port.
            await _driver.TurnOnModuleAsync();

            //using (var httpClient = new HttpClient(new Sim868HttpClientHandler(sim868Client, apn)))
            //{
            //    //var response = await httpClient.GetAsync("http://moonberry01.ddns.net:5000/telemetry");
            //    //string data = await response.Content.ReadAsStringAsync();
            //    //Console.WriteLine(data);

            //    TelemetryData telemetryData = new TelemetryData()
            //    {
            //        Notes = "This is a test!"
            //    };

            //    var content = new StringContent(JsonSerializer.Serialize(telemetryData).ToString(), Encoding.UTF8, "application/json");

            //    var response = await httpClient.PostAsync("http://moonberry01.ddns.net:5000/telemetry", content);
            //    string data = await response.Content.ReadAsStringAsync();
            //    Console.WriteLine(data);
            //}

            SerialPortExtensions.LogWriter = new StreamWriter(@"buffer.log", true, UTF8Encoding.UTF8, 1); 

            GpsApi gpsAPi = new GpsApi(_driver, TimeSpan.FromSeconds(15));
            gpsAPi.DataAvailable += new DataAvailableEventHandler(Gps_DataAvailable);
            
            await gpsAPi.StartAsync();

            Console.ReadLine();
        }

        static private async Task Gps_DataAvailable(object sender, GnssNavInfo gnssNavInfo)
        {
            using (var httpClient = new HttpClient(new Sim868HttpClientHandler(_driver, _apn)))
            {
                TelemetryData telemetryData = new TelemetryData()
                {
                    Latitude = 0.0f,
                    Longitude = 0.0f,
                    Notes = "Unabled to look onto GPS."
                };

                if (gnssNavInfo != null)
                {
                    telemetryData.Timestamp = gnssNavInfo.DateTime.ToUnixTimeMilliseconds();
                    telemetryData.Latitude = gnssNavInfo.Latitude;
                    telemetryData.Longitude = gnssNavInfo.Longitude;
                    telemetryData.Notes = "Gps is tracking.";
                }

                var content = new StringContent(JsonSerializer.Serialize(telemetryData).ToString(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("http://moonberry01.ddns.net:5000/telemetry", content);
                string data = await response.Content.ReadAsStringAsync();
            }

        }
    }
}
