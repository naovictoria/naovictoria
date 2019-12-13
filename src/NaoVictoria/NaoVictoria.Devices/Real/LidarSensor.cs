﻿using Iot.Device.DistanceSensor;
using NaoVictoria.Devices.Interfaces;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Text;

namespace NaoVictoria.Devices
{
    public class LidarSensor : ICollisionSensor
    {
        private LidarLiteV3 _llv3;

        public LidarSensor()
        {
            var settings = new I2cConnectionSettings(1, LidarLiteV3.DefaultI2cAddress);
            var device = I2cDevice.Create(settings);
            _llv3 = new LidarLiteV3(device, new GpioController(), 13);
        }

        public int GetDistanceToObject()
        {
            return _llv3.MeasureDistance();
        }        
    }
}