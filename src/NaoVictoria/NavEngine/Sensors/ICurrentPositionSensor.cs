using NaoVictoria.NavEngine.Models;

namespace NaoVictoria.NavEngine.Sensors
{
    public interface ICurrentPositionSensor
    {
        GeoPoint GetReading();
    }
}
