using Iot.Device.Bno055;
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Text;
using System.Threading;

namespace NaoVictoria.NavEngine.Sensors
{
    public class CompassSensor : ICurrentDirectionSensor
    {
        private readonly Bno055Sensor _bno055Sensor;

        public CompassSensor()
        {
            I2cDevice i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Bno055Sensor.DefaultI2cAddress));
            _bno055Sensor = new Bno055Sensor(i2cDevice);

            Console.WriteLine($"Id: {_bno055Sensor.Info.ChipId}, AccId: {_bno055Sensor.Info.AcceleratorId}, GyroId: {_bno055Sensor.Info.GyroscopeId}, MagId: {_bno055Sensor.Info.MagnetometerId}");
            Console.WriteLine($"Firmware version: {_bno055Sensor.Info.FirmwareVersion}, Bootloader: {_bno055Sensor.Info.BootloaderVersion}");
            Console.WriteLine($"Temperature source: {_bno055Sensor.TemperatureSource}, Operation mode: {_bno055Sensor.OperationMode}, Units: {_bno055Sensor.Units}");
            Console.WriteLine($"Powermode: {_bno055Sensor.PowerMode}");

            Console.WriteLine("Checking the magnetometer calibration, move the sensor up to the calibration will be complete if needed");
            var calibrationStatus = _bno055Sensor.GetCalibrationStatus();
            while ((calibrationStatus & CalibrationStatus.MagnetometerSuccess) != (CalibrationStatus.MagnetometerSuccess))
            {
                Console.Write($".");
                calibrationStatus = _bno055Sensor.GetCalibrationStatus();
                Thread.Sleep(200);
            }

            Console.WriteLine();
            Console.WriteLine("Calibration completed");
        }

        public double GetReadingInRadian()
        {
            var magneto = _bno055Sensor.Magnetometer;
            Console.WriteLine($"Magnetomer X: {magneto.X} Y: {magneto.Y} Z: {magneto.Z}");
            var gyro = _bno055Sensor.Gyroscope;
            Console.WriteLine($"Gyroscope X: {gyro.X} Y: {gyro.Y} Z: {gyro.Z}");
            var accele = _bno055Sensor.Accelerometer;
            Console.WriteLine($"Acceleration X: {accele.X} Y: {accele.Y} Z: {accele.Z}");
            var orien = _bno055Sensor.Orientation;
            Console.WriteLine($"Orientation Heading: {orien.X} Roll: {orien.Y} Pitch: {orien.Z}");
            var line = _bno055Sensor.LinearAcceleration;
            Console.WriteLine($"Linear acceleration X: {line.X} Y: {line.Y} Z: {line.Z}");
            var gravity = _bno055Sensor.Gravity;
            Console.WriteLine($"Gravity X: {gravity.X} Y: {gravity.Y} Z: {gravity.Z}");
            var qua = _bno055Sensor.Quaternion;
            Console.WriteLine($"Quaternion X: {qua.X} Y: {qua.Y} Z: {qua.Z} W: {qua.W}");
            var temp = _bno055Sensor.Temperature.Celsius;
            Console.WriteLine($"Temperature: {temp} °C");

            return 0.0;
        }
    }
}
