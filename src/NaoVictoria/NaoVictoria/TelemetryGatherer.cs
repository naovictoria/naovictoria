using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using NaoVictoria.Devices.Interfaces;
using NaoVictoria.Models;

namespace NaoVictoria
{
    public class TelemetryGatherer : ITelemetryGatherer
    {
        private readonly ILogger<TelemetryGatherer> _logger;
        private readonly ICurrentOrientationSensor _currentOrientationSensor;
        private readonly ICurrentPositionSensor _currentPositionSensor;
        private readonly ICurrentWindDirectionSensor _currentWindDirectionSensor;
        private readonly ICollisionSensor _collisionSensor;
        private readonly IMainSailControl _mainSailControl;
        private readonly IJibSailControl _jibSailControl;
        private readonly IRudderControl _rudderControl;


        public TelemetryGatherer(ILogger<TelemetryGatherer> logger,
            ICurrentOrientationSensor currentOrientationSensor,
            ICurrentPositionSensor currentPositionSensor,
            ICurrentWindDirectionSensor currentWindDirectionSensor,
            ICollisionSensor collisionSensor,
            IRudderControl rudderControl,
            IMainSailControl mainSailControl,
            IJibSailControl jibSailControl)
        {
            _logger = logger;
            _currentOrientationSensor = currentOrientationSensor;
            _currentPositionSensor = currentPositionSensor;
            _currentWindDirectionSensor = currentWindDirectionSensor;
            _collisionSensor = collisionSensor;
            _rudderControl = rudderControl;
            _mainSailControl = mainSailControl;
            _jibSailControl = jibSailControl;
        }

        public TelemetryData Gather()
        {
            var telemetryData = new TelemetryData();

            telemetryData.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            telemetryData.Notes = "This is a test.";

            var gpsCoord = _currentPositionSensor.GetReading();
            //_logger.LogInformation($"Position @ ({gpsCoord.Longitude}, {gpsCoord.Latitude})");
            telemetryData.Longitude = gpsCoord.Longitude;
            telemetryData.Latitude = gpsCoord.Latitude;

            var currentWindVaneReading = _currentWindDirectionSensor.GetReadingInRadians();
            telemetryData.WindVaneDirection = currentWindVaneReading;
            //_logger.LogInformation($"Wind vane @ {currentWindVaneReading}");

            var distanceToCollisionCm = _collisionSensor.GetDistanceToObject();
            // var distanceToCollisionCm = 0;
            telemetryData.BowDistanceToCollisionCm = distanceToCollisionCm;

            //_logger.LogInformation($"Collision to object @ {distanceToCollisionCm} cm");


            var orientation = _currentOrientationSensor.GetOrientationInRadian();
            telemetryData.OrientationHeading = orientation.heading;
            telemetryData.OrientationPitch = orientation.pitch;
            telemetryData.OrientationRoll = orientation.roll;
            //_logger.LogInformation($"Orientation reading @ heading: {orientation.heading}, pitch: {orientation.pitch}, roll: {orientation.roll}");

            return telemetryData;
        }
    }
}
