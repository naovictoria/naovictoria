﻿using Microsoft.Extensions.Logging;
using NaoVictoria.NavEngine;
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

            _navEngine = new RealNavEngine(
                _compassSensor, 
                _gpsSensor, _windVaneSensor, 
                worldOceanMap, 
                globalPlan);
        }


        public void DoWork()
        {
            // Do Navigation
            _navEngine.Navigate();

            // Gather Telemetry

            // Send Telementry
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }
    }
}