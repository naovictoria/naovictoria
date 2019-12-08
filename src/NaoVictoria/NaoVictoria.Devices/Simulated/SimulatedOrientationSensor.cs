using NaoVictoria.Devices.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.Devices
{
    public class SimulatedOrientationSensor : ICurrentOrientationSensor
    {
        public (double heading, double roll, double pitch) GetOrientationInRadian()
        {
            return (0, 0, 0);
        }
    }
}
