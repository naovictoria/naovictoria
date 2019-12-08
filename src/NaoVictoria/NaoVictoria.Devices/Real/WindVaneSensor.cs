using System;
using System.Device.I2c;
using Iot.Device.Ads1115;
using NaoVictoria.Devices.Interfaces;

namespace NaoVictoria.Devices
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
                double voltage = adc.RawToVoltage(raw);
                // 0-to-4.7V
                return (voltage / 4.7) * 2.0 * Math.PI;
            }
        }
    }
}
