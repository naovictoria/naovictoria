using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Text;

namespace NaoVictoria.NavEngine.Sensors
{
    public class LidarSensor : ICollisionSensor
    {
        private I2cDevice _device;

        public LidarSensor()
        {
            var settings = new I2cConnectionSettings(1, LidarLiteV3.LidarLiteV3.DefaultI2cAddress);
            _device = I2cDevice.Create(settings);
        }

        public double GetDistanceToObject()
        {
            using (var llv3 = new LidarLiteV3.LidarLiteV3(_device))
            {
                return llv3.MeasureDistance();
            }
        }        
    }
}
