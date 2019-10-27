using NaoVictoria.NavEngine.Sensors;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.NavEngine.Sensors
{
    public class WindVaneSensor : ICurrentWindDirectionSensor
    {
        public double GetReadingInRadians()
        {
            // TODO: Use sensor to determine direction.
            return 0.0;
        }
    }
}
