using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.Devices.Interfaces
{
    public interface ICollisionSensor
    {
        int GetDistanceToObject();
    }
}
