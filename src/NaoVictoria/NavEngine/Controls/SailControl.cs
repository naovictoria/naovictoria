using Iot.Device.Pwm;
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Text;

namespace NaoVictoria.NavEngine.Controls
{
    public class SailControl
    {
        I2cDevice _device;
        public SailControl()
        {
            var busId = 1;
            var selectedI2cAddress = 0b000000;     // A5 A4 A3 A2 A1 A0
            var deviceAddress = Pca9685.I2cAddressBase + selectedI2cAddress;

            var settings = new I2cConnectionSettings(busId, deviceAddress);
            _device = I2cDevice.Create(settings);
        }

        public void MoveTo(double angleRadians)
        {
            using (var pca9685 = new Pca9685(_device))
            {
                pca9685.SetDutyCycleAllChannels(angleRadians);
            }
        }
    }
}
