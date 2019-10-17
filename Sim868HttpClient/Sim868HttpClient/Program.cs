using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sim868HttpClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (HttpClient httpClient = new HttpClient(new Sim868HttpClientHandler()))
            {
                var response = await httpClient.GetAsync("http://moonberry01.ddns.net:5000/weatherforecast");
                string data = await response.Content.ReadAsStringAsync();
                Console.WriteLine(data);
            }
        }
    }
}
