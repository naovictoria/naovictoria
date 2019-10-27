using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.NavEngine.Sensors
{
    public class CompassSensor : ICurrentDirectionSensor
    {
        public double GetReadingInRadian()
        {
            return 0.0;
        }
    }
}
