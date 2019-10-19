using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sim868HttpClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Note: Access point name is dependent on the provider.  
            // For example, it's "wholesale" for Ting.
            string apn = "wholesale";

            using (HttpClient httpClient = new HttpClient(new Sim868HttpClientHandler(new Sim868Client(new System.IO.Ports.SerialPort()), apn)))
            {
                var response = await httpClient.GetAsync("http://moonberry01.ddns.net:5000/weatherforecast");
                string data = await response.Content.ReadAsStringAsync();
                Console.WriteLine(data);
            }
        }
    }
}
