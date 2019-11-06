using System.Device.I2c;
using Iot.Device.Ads1115;

namespace NaoVictoria.NavEngine.Sensors
{
    public class WindVaneSensor : ICurrentWindDirectionSensor
    {
        I2cDevice _device;
        public WindVaneSensor()
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, (int)I2cAddress.GND);
            _device = I2cDevice.Create(settings);
        }
        public double GetReadingInRadians()
        {
            using (Ads1115 adc = new Ads1115(_device, InputMultiplexer.AIN1, MeasuringRange.FS6144))
            {
                short raw = adc.ReadRaw();
                return adc.RawToVoltage(raw);
            }
        }
    }
}
