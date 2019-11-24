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

            var version = Version;

            if (version != 0x12)
            {
                throw new InvalidOperationException("Failed to find rfm9x with the expected version.");
            }

            var operationMode = OperationMode;
            OperationMode = (OperationMode)(((int)operationMode & 0b111_1000) | (int)OperationMode.OPMODE_SLEEP);
            Thread.Sleep(10);
            OperationMode |= OperationMode.LONG_RANGE;
            Thread.Sleep(10);
            
            if (!OperationMode.HasFlag(OperationMode.OPMODE_SLEEP) || !OperationMode.HasFlag(OperationMode.LONG_RANGE))
            {
                throw new InvalidOperationException("Failed to configure radio for LoRa mode.");
            }

            if (frequency > 525)
            {
                OperationMode &= ~OperationMode.LOW_FREQ_MODE;
            }

            WriteRegister(Register.FIFO_TX_BASE_ADDR, 0x00);
            WriteRegister(Register.FIFO_RX_BASE_ADDR, 0x00);

            OperationMode = (OperationMode)(((int)operationMode & 0b111_1000) | (int)OperationMode.OPMODE_STANDBY);

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
        
        public byte Version
        {
            get {
                return ReadRegister(Register.VERSION);
            }
        }

        public OperationMode OperationMode {
            get {
                return (OperationMode)ReadRegister(Register.OP_MODE);
            }

            set {
                WriteRegister(Register.OP_MODE, (byte)value);
            }
        }

        private byte ReadRegister(Register register)
        {
            Span<byte> buffer = stackalloc byte[] { (byte)((int)register & ~0x80), 0x00 };
            _device.TransferFullDuplex(buffer, buffer);
            return buffer[1];
        }

        private void WriteRegister(Register register, byte value)
        {
            Span<byte> buffer = stackalloc byte[] { (byte)((int)register | 0x80), (byte)value };
            _device.TransferFullDuplex(buffer, buffer);
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
