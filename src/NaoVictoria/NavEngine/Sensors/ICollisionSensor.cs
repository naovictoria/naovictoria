using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.NavEngine.Sensors
{
    interface ICollisionSensor
    {
        double GetDistanceToObject();
    }
}
