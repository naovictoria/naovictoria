// Licensed to the .NET Foundation under one or more agreements. 
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.TimeOfFlight.Models.LidarLiteV3;
using System;
using System.Buffers.Binary;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.TimeOfFlight
{
    /// <summary>
    /// Lidar Lite v3 is a long-range fixed position distance sensor by Garmin.
    /// </summary>
    public class LidarLiteV3 : IDisposable
    {
        /// <summary>
        /// Default address for LidarLiteV3
        /// </summary>
        public const byte DefaultI2cAddress = 0x62;

        internal GpioController _gpioController;
        internal I2cDevice _i2cDevice;
        internal int? _powerEnablePin;

        /// <summary>
        /// Initialize the LidarLiteV3
        /// </summary>
        /// <param name="i2cDevice">I2C device</param>
        /// <param name="gpioController">GPIO controller</param>
        /// <param name="powerEnablePin">The pin number used to control power to the device</param>
        public LidarLiteV3(I2cDevice i2cDevice, GpioController gpioController = null, int? powerEnablePin = null)
        {
            _gpioController = gpioController;
            _i2cDevice = i2cDevice;
            _powerEnablePin = powerEnablePin;

            if (_gpioController != null && _powerEnablePin.HasValue)
            {
                _gpioController.OpenPin(_powerEnablePin.Value);
                PowerOn();
            }

            Reset();
        }

        ~LidarLiteV3()
        {
            _gpioController.OpenPin(_powerEnablePin.Value);
        }

        public void PowerOff()
        {
            if (_gpioController != null && _powerEnablePin.HasValue)
            {
                _gpioController.SetPinMode(_powerEnablePin.Value, PinMode.InputPullDown);
            } else
            {
                throw new InvalidOperationException("Cannot power off without providing GPIO controller and power enable pin.");
            }
        }

        public void PowerOn()
        {
            if (_gpioController != null && _powerEnablePin.HasValue)
            {
                _gpioController.SetPinMode(_powerEnablePin.Value, PinMode.InputPullUp);
            }
            else
            {
                throw new InvalidOperationException("Cannot power off without providing GPIO controller and power enable pin.");
            }
        }

        /// <summary>
        /// Reset FPGA, all registers return to default values
        /// </summary>
        public void Reset()
        {
            try
            {
                WriteRegister(Register.ACQ_COMMAND, 0x00);
            }
            catch (System.IO.IOException)
            {
                // This exception is expected due to the reset.
            }
        }

        /// <summary>
        /// Measure distance in cm
        /// </summary>
        /// <param name="withReceiverBiasCorrection">Faster without bias correction, but more prone to errors if condition changes.</param>
        /// <returns>Distance in cm</returns>
        public ushort MeasureDistance(bool withReceiverBiasCorrection = true)
        {
            if (withReceiverBiasCorrection)
            {
                WriteRegister(Register.ACQ_COMMAND, 0x04);
            }
            else
            {
                WriteRegister(Register.ACQ_COMMAND, 0x03);
            }

            SystemStatus status = GetStatus();
            while (status.HasFlag(SystemStatus.BusyFlag))
            {
                status = GetStatus();
                Thread.Sleep(1);
            }

            return GetDistanceMeasurement();
        }

        /// <summary>
        /// The maximum aquisition count limits the number of times
        /// the device will integrate acquistions to find a correlation
        /// record peak.
        /// 
        /// Roughly correlates to: acq rate = 1/count and max 
        /// range = count^(1/4)
        /// </summary>
        private int MaximumAcquisitionCount
        {
            get {
                Span<byte> rawData = stackalloc byte[1] { 0 };
                ReadBytes(Register.SIG_COUNT_VAL, rawData);
                return rawData[0];
            }
            set {
                WriteRegister(Register.SIG_COUNT_VAL, (byte)value);
            }
        }

        /// <summary>
        /// Get or set the acquistion mode control
        /// </summary>
        public AcquistionMode AcquistionMode {
            get {
                Span<byte> rawData = stackalloc byte[1] { 0 };
                ReadBytes(Register.ACQ_CONFIG_REG, rawData);
                return (AcquistionMode)rawData[0];
            }
            set {
                WriteRegister(Register.ACQ_CONFIG_REG, (byte)value);
            }
        }

        /// <summary>
        /// The difference between the current and last measurement resulting
        /// in a signed (2's complement) 8-bit number in cm.
        /// Positive is away from the device.
        /// </summary>
        public byte Velocity {
            get {
                Span<byte> rawData = stackalloc byte[1] { 0 };
                ReadBytes(Register.VELOCITY, rawData);
                return rawData[0];
            }
            set {
                WriteRegister(Register.VELOCITY, value);
            }
        }

        /// <summary>
        /// Detection algorithm bypass threhold, default 0.
        /// 
        /// Recommended non-default values are 32 for higher sensitivity
        /// but higher erronenous measurement and 96 for reduced 
        /// sensitivity and fewer erroneous measurements.
        /// </summary>
        public int AlgorithmBypassThreshold {
            get {
                Span<byte> rawData = stackalloc byte[1] { 0 };
                ReadBytes(Register.THRESHOLD_BYPASS, rawData);
                return rawData[0];
            }
            set {
                WriteRegister(Register.THRESHOLD_BYPASS, (byte)value);
            }
        }

        public void SetMeasurementRepetitionMode(MeasurementRepetitionMode measurementRepetitionMode, int? count, int? delay)
        {
            // TODO: make sure count is between 0x02 and 0xfe
            if (count.HasValue && (count < 2 || count > 254))
            {
                throw new ArgumentOutOfRangeException("Count must be between 2 and 254");
            };

            switch (measurementRepetitionMode)
            {
                case MeasurementRepetitionMode.Repeat:
                    WriteRegister(Register.OUTER_LOOP_COUNT, (byte)count);
                    break;
                case MeasurementRepetitionMode.RepeatIndefintely:
                    WriteRegister(Register.OUTER_LOOP_COUNT, 0xff);
                    break;
                case MeasurementRepetitionMode.Off:
                    WriteRegister(Register.OUTER_LOOP_COUNT, 0x00);
                    break;
            }

            if (delay != null)
            {
                WriteRegister(Register.MEASURE_DELAY, (byte) delay.Value);

                // Set mode to use custom delay.
                var currentAcqMode = AcquistionMode;
                currentAcqMode |= AcquistionMode.UseDefaultDelay;
                AcquistionMode = currentAcqMode;
            } else
            {
                // Set mode to use default delay.
                var currentAcqMode = AcquistionMode;
                currentAcqMode &= ~AcquistionMode.UseDefaultDelay;
                AcquistionMode = currentAcqMode;
            }

        }

        public void ConfigureI2CAddress(byte address)
        {
            // Valid values are 7-bit values with 0 in the LSB.
            if((address & 1) == 1)
            {
                throw new ArgumentOutOfRangeException("Address must have 0-bit in the LSB.");
            }

            // Read in the unit's serial number.
            Span<byte> rawData = stackalloc byte[2] { 0, 0 };
            ReadBytes(Register.UNIT_ID, rawData);

            // Write serial number to I2C_ID.
            WriteRegister(Register.I2C_ID_HIGH, rawData[1]);
            WriteRegister(Register.I2C_ID_LOW, rawData[0]);
            
            // Write the new address.
            WriteRegister(Register.I2C_SEC_ADDR, address);

            // Disable the default address
            WriteRegister(Register.I2C_CONFIG, 0x08);
        }

        public PowerOption PowerOption
        {
            get
            {
                Span<byte> rawData = stackalloc byte[1] { 0 };
                ReadBytes(Register.POWER_CONTROL, rawData);
                return (PowerOption)rawData[0];
            }
            set
            {
                // Bit 0 disables receiver circuit
                WriteRegister(Register.POWER_CONTROL, (byte)value);
            }
        }
    
        /// <summary>
        /// Get the distance measurement in cm.
        /// </summary>
        public ushort GetDistanceMeasurement()
        {
            Span<byte> rawData = stackalloc byte[2] { 0, 0 };
            ReadBytes(Register.FULL_DELAY, rawData);
            return BinaryPrimitives.ReadUInt16BigEndian(rawData);
        }

        /// <summary>
        /// Get the system status
        /// </summary>
        public SystemStatus GetStatus()
        {
            Span<byte> rawData = stackalloc byte[1] { 0 };
            ReadBytes(Register.STATUS, rawData);
            return (SystemStatus)rawData[0];
        }

        #region I2C

        internal void WriteRegister(Register reg, byte data)
        {
            Span<byte> dataout = stackalloc byte[] { (byte)reg, data };
            _i2cDevice.Write(dataout);
        }

        internal byte ReadByte(Register reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            return _i2cDevice.ReadByte();
        }

        internal void ReadBytes(Register reg, Span<byte> readBytes)
        {
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(readBytes);
        }

        /// <summary>
        /// Cleanup everything
        /// </summary>
        public void Dispose()
        {
            if (_gpioController != null && _powerEnablePin.HasValue)
            {
                _gpioController.ClosePin(_powerEnablePin.Value);
            }

            _gpioController?.Dispose();
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        #endregion
    }
}
