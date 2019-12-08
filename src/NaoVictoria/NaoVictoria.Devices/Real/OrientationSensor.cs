using Iot.Device.Bno055;
using NaoVictoria.Devices.Interfaces;
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Text;
using System.Threading;

namespace NaoVictoria.Devices
{
    public class OrientationSensor : ICurrentOrientationSensor
    {
        private readonly Bno055Sensor _bno055Sensor;

        public OrientationSensor()
        {
            I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Bno055Sensor.DefaultI2cAddress));
            _bno055Sensor = new Bno055Sensor(i2cDevice);
            _bno055Sensor.Units = _bno055Sensor.Units | Units.EulerAnglesRadians; 
        }

        public (double heading, double roll, double pitch) GetOrientationInRadian()
        {
            var orien = _bno055Sensor.Orientation;            
            return (orien.X, orien.Y, orien.Z);
        }
    }
}
