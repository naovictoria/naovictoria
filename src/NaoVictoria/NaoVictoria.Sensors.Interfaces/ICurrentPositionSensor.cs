using NaoVictoria.Models;

namespace NaoVictoria.Sensors.Interfaces
{
    public interface ICurrentPositionSensor
    {
        GeoPoint GetReading();
    }
}
