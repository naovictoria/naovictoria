using NaoVictoria.Devices.Interfaces;
using System;

namespace NaoVictoria.Devices
{
    public class SimulatedCollisionSensor : ICollisionSensor
    {
        public int GetDistanceToObject()
        {
            return 0;
        }
    }
}
