using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.Sensors.Interfaces
{
    public interface ICurrentOrientationSensor
    {
        (double heading, double roll, double pitch) GetOrientationInRadian();
    }
}
