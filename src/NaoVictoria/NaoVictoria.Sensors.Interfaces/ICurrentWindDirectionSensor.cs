using System;

namespace NaoVictoria.Sensors.Interfaces
{
    public interface ICurrentWindDirectionSensor
    {
        double GetReadingInRadians();
    }
}
