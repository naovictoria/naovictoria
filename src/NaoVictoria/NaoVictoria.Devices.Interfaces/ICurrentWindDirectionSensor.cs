using System;

namespace NaoVictoria.Devices.Interfaces
{
    public interface ICurrentWindDirectionSensor
    {
        double GetReadingInRadians();
    }
}
