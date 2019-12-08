using NaoVictoria.Models;
using NaoVictoria.Sim868.Gps;
using NaoVictoria.Sim868.Http;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NaoVictoria
{
    public class GsmTelemetrySender : ITelemetrySender
    {
        static string _apn = "wholesale";

        private readonly Sim868Driver _driver;

        private long _lastTelemetrySent;


        public GsmTelemetrySender(Sim868Driver driver)
        {
            _driver = driver;
        }

        public async Task SendTelementry(TelemetryData data)
        {
            if (_lastTelemetrySent + 30 >= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return;
            }

            _lastTelemetrySent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            using (var httpClient = new HttpClient(new Sim868HttpClientHandler(_driver, _apn)))
            {
                var content = new StringContent(JsonSerializer.Serialize(data).ToString(), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("http://moonberry01.ddns.net:5000/telemetry", content);
                _ = await response.Content.ReadAsStringAsync();
            }
        }
    }
}
