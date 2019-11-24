using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.IO;
using System.Threading;

namespace RFM9X
{
    public class RFM9X
    {
        private readonly SpiDevice _device;
        private readonly int _resetPinNumber;
        private readonly GpioController _controller;

        public RFM9X(
            double frequency,
            int busId = 0,
            int chipSelectLine = 1,
            int resetPinNumber = 31,
            int preambleLength = 8,
            bool highPower = true,
            int clockFrequency = 5000000)
        {
            var settings = new SpiConnectionSettings(busId, chipSelectLine);
            settings.ClockFrequency = clockFrequency;
            settings.Mode = SpiMode.Mode0;
            settings.DataBitLength = 8;

            _resetPinNumber = resetPinNumber;
            _device = SpiDevice.Create(settings);
            _controller = new GpioController(PinNumberingScheme.Board);
            _controller.OpenPin(_resetPinNumber, PinMode.InputPullUp);

            Reset();

            var version = GetVersion();

            if (version != 0x12)
            {
                throw new InvalidOperationException("Failed to find rfm9x with the expected version.");
            }

            OperationMode |= OperationMode.OPMODE_SLEEP;

            Console.WriteLine("Operation mode: " + OperationMode);

            //Thread.Sleep(10);

            //LongRangeMode = true;

            //if (OperationMode != Mode.SLEEP || !LongRangeMode)
            //{
            //    throw new InvalidOperationException("Failed to configure radio for LoRa mode.");
            //}

            //if (frequency > 525)
            //{
            //    LowFrequencyMode = false;
            //}

            //_device.WriteByte((byte)Register.FIFO_TX_BASE_ADDR);
            //_device.WriteByte(0x00);
            //_device.WriteByte((byte)Register.FIFO_TX_BASE_ADDR);
            //_device.WriteByte(0x00);

            //OperationMode = Mode.STANDBY;

            //SignalBandwidth = Bandwidth.BW_125000;
            //// CodingRate = 5;
            //// SpreadingFactor = 7;
            //// EnableCrc = false;

            //_device.WriteByte((byte)Register.MODEM_CONFIG3);
            //_device.WriteByte(0x00);
            //// PreambleLength = preambleLength;

            //// FrequencyMhz = frequency;
            //// TxPower = 13;
        }

        #region Board elements

        public void Reset()
        {
            _controller.SetPinMode(_resetPinNumber, PinMode.Output);
            _controller.Write(_resetPinNumber, PinValue.Low);
            Thread.Sleep(1);
            _controller.SetPinMode(_resetPinNumber, PinMode.InputPullUp);
            Thread.Sleep(1);
        }

        public byte GetVersion()
        {
            Span<byte> buffer = stackalloc byte[] { (byte)Register.VERSION & ~0x80, 0x00 };
            _device.TransferFullDuplex(buffer, buffer);
            return buffer[1];
        }

        public OperationMode OperationMode {
            get 
            {
                Span<byte> buffer = stackalloc byte[] { (byte)Register.OP_MODE & ~0x80, 0x00 };
                _device.TransferFullDuplex(buffer, buffer);
                return (OperationMode)buffer[1];
            }

            set
            {
                Span<byte> buffer = stackalloc byte[] { (byte)Register.OP_MODE | 0x80, (byte)value };
                _device.TransferFullDuplex(buffer, buffer);
            }
        }

        public bool LowFrequencyMode {
            get {
                _device.WriteByte((byte)Register.OP_MODE);
                int opMode = _device.ReadByte();
                opMode = (opMode & 0b0000_1000) >> 3;
                return opMode == 1;
            }

            set {
                _device.WriteByte((byte)Register.OP_MODE);
                int opMode = _device.ReadByte();
                opMode &= 0b1111_0111;
                opMode |= ((value ? 1 : 0) & 0b1111_1111) << 3;
                _device.WriteByte((byte)Register.OP_MODE);
                _device.WriteByte((byte)opMode);
            }
        }

        public bool LongRangeMode 
        {
            get 
            {
                _device.WriteByte((byte)Register.OP_MODE);
                int opMode = _device.ReadByte();
                opMode = (opMode & 0b1000_0000) >> 7;
                return opMode == 1;
            }

            set
            {
                _device.WriteByte((byte)Register.OP_MODE);
                int opMode = _device.ReadByte();
                opMode &= 0b0111_1111;
                opMode |= ((value ? 1: 0) & 0b1111_1111) << 7;
                _device.WriteByte((byte)Register.OP_MODE);
                _device.WriteByte((byte)opMode);
            }
        }

        public Bandwidth SignalBandwidth
        {
            get 
            {
                _device.WriteByte((byte)Register.MODEM_CONFIG1);
                int bwId = _device.ReadByte();
                bwId = (bwId & 0b1111_0000) >> 4;
                return (Bandwidth)bwId;
            }
            set 
            {
                _device.WriteByte((byte)Register.MODEM_CONFIG1);
                int bwId = _device.ReadByte();
                bwId |= ((int)value & 0b0000_1111) << 4;
                _device.WriteByte((byte)Register.MODEM_CONFIG1);
                _device.WriteByte((byte)bwId);
            }
        }
}

    #endregion
}
