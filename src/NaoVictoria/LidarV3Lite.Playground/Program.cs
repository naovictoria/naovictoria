using Iot.Device.DistanceSensor;
using Iot.Device.DistanceSensor.Models.LidarLiteV3;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;

namespace LidarV3LitePlayground
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Simple            
            using (var llv3 = new LidarLiteV3(CreateI2cDevice()))
            {
                llv3.SetMeasurementRepetitionMode(MeasurementRepetition.RepeatIndefinitely);
                // Take 10 measurements, each one second apart.
                for (int i = 0; i < 10; i++)
                {
                    int currentDistance = llv3.MeasureDistance();
                    Console.WriteLine($"Current Distance: {currentDistance} cm");
                    Thread.Sleep(1000);
                }
            }
        }

        private static I2cDevice CreateI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, LidarLiteV3.DefaultI2cAddress);
            return I2cDevice.Create(settings);
        }

        private static GpioController CreateGpioController()
        {
            return new GpioController();
        }
    }

}
