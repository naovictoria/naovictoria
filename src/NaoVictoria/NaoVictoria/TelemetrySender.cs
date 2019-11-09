using NaoVictoria.Models;
using NaoVictoria.Sim868Driver.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NaoVictoria
{
    class TelemetrySender
    {
        static string _apn = "wholesale";

        private readonly Sim868Driver.Driver _driver;
        public TelemetrySender(Sim868Driver.Driver driver)
        {
            _driver = driver;
        }

        public async Task SendTelementry(TelemetryData data)
        {
            using (var httpClient = new HttpClient(new Sim868HttpClientHandler(_driver, _apn)))
            {
                var content = new StringContent(JsonSerializer.Serialize(data).ToString(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("http://moonberry01.ddns.net:5000/telemetry", content);
                _ = await response.Content.ReadAsStringAsync();
            }
        }
    }
}
