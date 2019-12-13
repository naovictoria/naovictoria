using Microsoft.Extensions.Logging;
using NaoVictoria.Devices;
using NaoVictoria.Models;
using NaoVictoria.NavEngine;
using NaoVictoria.Sim868.Gps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaoVictoria
{
    public class NaoVictoriaEngine : INaoVictoriaEngine
    {
        private readonly ILogger<NaoVictoriaEngine> _logger;
        private readonly INavEngine _navEngine;
        private readonly ITelemetryGatherer _telemetryGatherer;
        private readonly ITelemetrySender _telemetrySender;

        public NaoVictoriaEngine(ILogger<NaoVictoriaEngine> logger, INavEngine navEngine, ITelemetryGatherer telemetryGatherer, ITelemetrySender telemetrySender)
        {
            _logger = logger;
            _navEngine = navEngine;
            _telemetryGatherer = telemetryGatherer;
            _telemetrySender = telemetrySender;

            // TODO: Load this from a file?
            IEnumerable<GeoPoint> globalPlan = new List<GeoPoint>() {
                new GeoPoint(-70.49648,41.95163),
                new GeoPoint(-70.29465,41.81431),
                new GeoPoint(-70.20813,41.52503)
            };

            // TODO: Load from a file?
            IEnumerable<GeoPoint> worldOceanMap = new List<GeoPoint>()
            {

            };
       }


        public async Task DoWork()
        {
            // Do Navigation
            _navEngine.Navigate();

            // Gather Telemetry
            var telemetryData = _telemetryGatherer.Gather();
            
            // Send Telementry
            await _telemetrySender.SendTelementry(telemetryData);
        }
    }
}
