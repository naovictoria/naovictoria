using NaoVictoria.Models;
using NaoVictoria.Devices.Interfaces;
using System;

namespace NaoVictoria.Devices
{
    public class SimulatedGpsSensor : ICurrentPositionSensor
    {
        private Random _random = new Random();
        public GeoPoint GetReading()
        {
            return new GeoPoint(-71.155388, 41.701031);
        }
    }
}
