using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Text;

namespace NaoVictoria.NavEngine.Sensors
{
    public class LidarSensor : ICollisionSensor
    {
        private LidarLiteV3.LidarLiteV3 _llv3;

        public LidarSensor()
        {
            var settings = new I2cConnectionSettings(1, LidarLiteV3.LidarLiteV3.DefaultI2cAddress);
            var device = I2cDevice.Create(settings);
            _llv3 = new LidarLiteV3.LidarLiteV3(device);
        }

        public double GetDistanceToObject()
        {
            return _llv3.MeasureDistance();
        }        
    }
}
