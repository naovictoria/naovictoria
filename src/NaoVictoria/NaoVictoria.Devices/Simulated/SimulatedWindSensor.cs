using NaoVictoria.Devices.Interfaces;

namespace NaoVictoria.Devices
{
    public class SimulatedWindSensor : ICurrentWindDirectionSensor
    {
        public double GetReadingInRadians()
        {
            return 0;
        }
    }
}
