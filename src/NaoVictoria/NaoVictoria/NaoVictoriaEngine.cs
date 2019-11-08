using Microsoft.Extensions.Logging;
using NaoVictoria.NavEngine;
using NaoVictoria.NavEngine.Controls;
using NaoVictoria.NavEngine.Models;
using NaoVictoria.NavEngine.Sensors;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria
{
    class NaoVictoriaEngine : INaoVictoriaEngine
    {
        private readonly ILogger<NaoVictoriaEngine> _logger;
        private readonly INavEngine _navEngine;
        private readonly GpsSensor _gpsSensor;
        private readonly CompassSensor _compassSensor;
        private readonly WindVaneSensor _windVaneSensor;
        private readonly LidarSensor _lidarSensor;
        private readonly SailControl _sailControl;
        private readonly RudderControl _rudderControl;

        public NaoVictoriaEngine(ILogger<NaoVictoriaEngine> logger)
        {
            _logger = logger;

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

            _gpsSensor = new GpsSensor();
            _compassSensor = new CompassSensor();
            _windVaneSensor = new WindVaneSensor();
            _lidarSensor = new LidarSensor();
            _sailControl = new SailControl();
            _rudderControl = new RudderControl();

            _navEngine = new RealNavEngine(
                _compassSensor, 
                _gpsSensor, 
                _windVaneSensor,
                _lidarSensor,
                _sailControl,
                _rudderControl,
                worldOceanMap, 
                globalPlan);
        }


        public void DoWork()
        {
            _logger.LogInformation("Doing work...");
            // Do Navigation
            _navEngine.Navigate();

            // Gather Telemetry

            //var currentWindVaneReading = _windVaneSensor.GetReadingInRadians();
            //_logger.LogInformation($"Wind vane @ {currentWindVaneReading}");

            var distanceToObject = _lidarSensor.GetDistanceToObject();
            _logger.LogInformation($"Collision to object @ {distanceToObject} cm");
            if (distanceToObject > 50)
            {
                _sailControl.MoveTo(0.5);
            } else
            {
                _sailControl.MoveTo(1.0);
            }

            // Send Telementry
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }
    }
}
