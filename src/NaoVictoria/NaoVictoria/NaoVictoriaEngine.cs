using Microsoft.Extensions.Logging;
using NaoVictoria.Models;
using NaoVictoria.NavEngine;
using NaoVictoria.NavEngine.Controls;
using NaoVictoria.NavEngine.Models;
using NaoVictoria.NavEngine.Sensors;
using NaoVictoria.Sim868Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NaoVictoria
{
    class NaoVictoriaEngine : INaoVictoriaEngine
    {
        private readonly ILogger<NaoVictoriaEngine> _logger;
        private readonly INavEngine _navEngine;
        private readonly GpsSensor _gpsSensor;
        private readonly OrientationSensor _orientationSensor;
        private readonly WindVaneSensor _windVaneSensor;
        private readonly LidarSensor _bowCollisionDetector;
        private readonly SailControl _sailControl;
        private readonly RudderControl _rudderControl;
        private readonly Driver _driver;
        private readonly TelemetrySender _telemetrySender;
        private long _lastTelemetrySent;

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

            _driver = new Driver(new System.IO.Ports.SerialPort(), "/dev/ttyUSB0", 29);
            _ = _driver.TurnOnModuleAsync().Result;

            _gpsSensor = new GpsSensor(_driver);
            _orientationSensor = new OrientationSensor();
            _windVaneSensor = new WindVaneSensor();
            _bowCollisionDetector = new LidarSensor();
            _sailControl = new SailControl();
            _rudderControl = new RudderControl();
            _telemetrySender = new TelemetrySender(_driver);

            _navEngine = new RealNavEngine(
                _orientationSensor, 
                _gpsSensor, 
                _windVaneSensor,
                _bowCollisionDetector,
                _sailControl,
                _rudderControl,
                worldOceanMap, 
                globalPlan);
        }


        public async Task DoWork()
        {
            // Do Navigation
            _navEngine.Navigate();

            // Gather Telemetry
            var telemetryData = new TelemetryData();

            telemetryData.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            telemetryData.Notes = "This is a test.";

            var gpsCoord = _gpsSensor.GetReading();
            _logger.LogInformation($"GPS @ ({gpsCoord.Longitude}, {gpsCoord.Latitude})");
            telemetryData.Longitude = gpsCoord.Longitude;
            telemetryData.Latitude = gpsCoord.Latitude;

            //var currentWindVaneReading = _windVaneSensor.GetReadingInRadians();
            //_logger.LogInformation($"Wind vane @ {currentWindVaneReading}");

            // var distanceToCollisionCm = _bowCollisionDetector.GetDistanceToObject();
            var distanceToCollisionCm = 0;
            telemetryData.BowDistanceToCollisionCm = distanceToCollisionCm;

            _logger.LogInformation($"Collision to object @ {distanceToCollisionCm} cm");

            if (distanceToCollisionCm > 50)
            {
                _sailControl.MoveTo(0.5);
            } else
            {
                _sailControl.MoveTo(1.0);
            }

            var orientation = _orientationSensor.GetOrientationInRadian();
            telemetryData.OrientationHeading = orientation.heading;
            telemetryData.OrientationPitch = orientation.pitch;
            telemetryData.OrientationRoll = orientation.roll;
            _logger.LogInformation($"Orientation reading @ heading: {orientation.heading}, pitch: {orientation.pitch}, roll: {orientation.roll}");

            // Send Telementry
            if (_lastTelemetrySent + 30 < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                _logger.LogInformation($"Sending telemetry.");
                await _telemetrySender.SendTelementry(telemetryData);
                _lastTelemetrySent = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }
    }
}
