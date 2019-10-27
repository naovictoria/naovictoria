using NaoVictoria.NavEngine.Models;

namespace NaoVictoria.NavEngine.Sensors
{
    public class GpsSensor : ICurrentPositionSensor
    {
        public GeoPoint GetReading()
        {
            return new GeoPoint(0.0, 0.0);
        }
    }
}
