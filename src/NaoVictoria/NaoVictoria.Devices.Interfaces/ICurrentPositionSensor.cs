using NaoVictoria.Models;

namespace NaoVictoria.Devices.Interfaces
{
    public interface ICurrentPositionSensor
    {
        GeoPoint GetReading();
    }
}
