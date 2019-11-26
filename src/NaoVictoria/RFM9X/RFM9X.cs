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
        private readonly bool _isHighPower;

        public RFM9X(
            double frequency,
            int busId = 0,
            int chipSelectLine = 1,
            int resetPinNumber = 31,
            int preambleLength = 8,
            bool isHighPower = true,
            int clockFrequency = 5000000)
        {
            _isHighPower = isHighPower;

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
            OperationMode = (OperationModeFlag)(((int)operationMode & 0b1111_1000) | (int)OperationModeFlag.OPMODE_SLEEP);
            Thread.Sleep(10);
            OperationMode |= OperationModeFlag.LONG_RANGE;
            Thread.Sleep(10);

            if (!OperationMode.HasFlag(OperationModeFlag.OPMODE_SLEEP) || !OperationMode.HasFlag(OperationModeFlag.LONG_RANGE))
            {
                throw new InvalidOperationException("Failed to configure radio for LoRa mode.");
            }

            if (frequency > 525)
            {
                OperationMode &= ~OperationModeFlag.LOW_FREQ_MODE;
            }

            WriteRegister(Register.FIFO_TX_BASE_ADDR, 0x00);
            WriteRegister(Register.FIFO_RX_BASE_ADDR, 0x00);

            OperationMode = (OperationModeFlag)(((int)operationMode & 0b1111_1000) | (int)OperationModeFlag.OPMODE_STANDBY);

            byte config1 = (byte)ModemConfig1;
            ModemConfig1 = (ModemConfig1Flag)((config1 & 0b0000_1111) | (int)ModemConfig1Flag.BW_125000);
            ModemConfig1 = (ModemConfig1Flag)((config1 & 0b1111_0001) | (int)ModemConfig1Flag.CR_5);
            // Also clears EnableCRC bit.
            ModemConfig2 = (ModemConfig2Flag)((config1 & 0b0000_1011) | (int)ModemConfig2Flag.SF_7);
            ModemConfig3 = 0x00;

            PreambleLength = preambleLength;
            TxPower = 13;
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

        public void Receive()
        {
            Listen();

            while(!RxDone)
            {
                Console.Write(".");
                Listen();
            }

            Console.WriteLine("Wow, this works!");
        }

        public void Listen()
        {
            OperationMode = OperationModeFlag.OPMODE_RX;
            Dio0Mapping = 0b00; // Interupt on RX done.
        }

        public void Transmit()
        {
            OperationMode = OperationModeFlag.OPMODE_TX;
            Dio0Mapping = 0b01; // Interupt on TX done.
        }

        public byte Version {
            get {
                return ReadRegister(Register.VERSION);
            }
        }

        public OperationModeFlag OperationMode {
            get {
                return (OperationModeFlag)ReadRegister(Register.OP_MODE);
            }

            set {
                WriteRegister(Register.OP_MODE, (byte)value);
            }
        }

        public ModemConfig1Flag ModemConfig1 {
            get {
                return (ModemConfig1Flag)ReadRegister(Register.MODEM_CONFIG1);
            }

            set {
                WriteRegister(Register.MODEM_CONFIG1, (byte)value);
            }
        }

        public ModemConfig2Flag ModemConfig2 {
            get {
                return (ModemConfig2Flag)ReadRegister(Register.MODEM_CONFIG2);
            }

            set {
                WriteRegister(Register.DETECTION_OPTIMIZE, (byte)(value == ModemConfig2Flag.SF_6 ? 0xc5 : 0xc3));
                WriteRegister(Register.DETECTION_THRESHOLD, (byte)(value == ModemConfig2Flag.SF_6 ? 0x0c : 0x0a));
                WriteRegister(Register.MODEM_CONFIG2, (byte)value);
            }
        }

        public byte ModemConfig3 {
            get {
                return ReadRegister(Register.MODEM_CONFIG3);
            }

            set {
                WriteRegister(Register.MODEM_CONFIG3, value);
            }
        }

        public int PreambleLength {
            get {
                var lsb = ReadRegister(Register.PREAMBLE_LSB);
                var msb = ReadRegister(Register.PREAMBLE_MSB);
                return (msb << 8) | lsb;
            }

            set {
                WriteRegister(Register.PREAMBLE_MSB, (byte)(value >> 8));
                WriteRegister(Register.PREAMBLE_LSB, (byte)(value & 0x0f));
            }
        }

        public double FrequencyMhz {
            get {
                var lsb = ReadRegister(Register.FRF_LSB);
                var mid = ReadRegister(Register.FRF_MID);
                var msb = ReadRegister(Register.FRF_MSB);
                var frf = (msb << 16) | (msb << 8) | lsb;

                // The crystal oscillator frequency of the module
                var _RH_RF95_FXOSC = 32000000.0;

                // The Frequency Synthesizer step = RH_RF95_FXOSC / 2^^19
                var _RH_RF95_FSTEP = (_RH_RF95_FXOSC / 524288);

                return (frf * _RH_RF95_FSTEP) / 1000000.0;
            }

            set {
                // The crystal oscillator frequency of the module
                var _RH_RF95_FXOSC = 32000000.0;

                // The frequency synthesizer step = RH_RF95_FXOSC / 2^^19
                var _RH_RF95_FSTEP = (_RH_RF95_FXOSC / 524288);

                var frf = (int)((value * 1000000.0) / _RH_RF95_FSTEP) & 0xFFFFFF;

                var msb = (byte)(frf >> 16);
                var mid = (byte)((frf >> 8) & 0x0f);
                var lsb = (byte)(frf & 0x0f);

                WriteRegister(Register.FRF_MSB, msb);
                WriteRegister(Register.FRF_MID, mid);
                WriteRegister(Register.FRF_LSB, lsb);
            }
        }

        public int TxPower {
            get {
                if (_isHighPower)
                {
                    return OutputPower + 5;
                }

                return OutputPower - 1;
            }

            set {
                if (_isHighPower)
                {
                    int txPower = value;
                    if (txPower > 20)
                    {
                        EnablePaBoost = true;
                        txPower = -3;
                    }
                    else
                    {
                        EnablePaBoost = false;
                    }
                    PaSelect = true;
                    OutputPower = (txPower - 5) & 0x0f;
                }
                else
                {
                    PaSelect = false;
                    MaxPower = 0b0111;
                    OutputPower = (value + 1) & 0x0f;
                }
            }
        }

        public bool EnablePaBoost {
            get {
                return (ReadRegister(Register.PA_DAC) & 0b111) == 0b100;
            }

            set {
                if (value)
                {
                    WriteRegister(Register.PA_DAC, (byte)((ReadRegister(Register.PA_DAC) & ~0b111) & 0b0100));
                }
                else
                {
                    WriteRegister(Register.PA_DAC, (byte)((ReadRegister(Register.PA_DAC) & ~0b111) & 0b0111));
                }
            }
        }

        public bool PaSelect {
            get {
                return ((ReadRegister(Register.PA_CONFIG) << 7) & 0x01) == 1;
            }

            set {
                var paConfig = ReadRegister(Register.PA_CONFIG) & 0xbf;
                if (value)
                {
                    WriteRegister(Register.PA_CONFIG, (byte)(paConfig | 0x40));
                }
            }
        }

        public int MaxPower {
            get {
                return (ReadRegister(Register.PA_CONFIG) << 4) & 0x07;
            }

            set {
                // clear MaxPower
                var paConfig = ReadRegister(Register.PA_CONFIG) & 0x8f;
                WriteRegister(Register.PA_CONFIG, (byte)(paConfig | (value << 4)));
            }
        }

        public int OutputPower {
            get {
                return ReadRegister(Register.PA_CONFIG) & 0x0f;
            }

            set {
                // clear current output power
                var paConfig = ReadRegister(Register.PA_CONFIG) & 0xf0;
                WriteRegister(Register.PA_CONFIG, (byte)(paConfig | value));
            }
        }

        public int Dio0Mapping {
            get {
                return (ReadRegister(Register.DIO_MAPPING1) << 6) & 0b11;
            }

            set {
                var dioMapping1 = ReadRegister(Register.DIO_MAPPING1) & 0b0011_1111;
                WriteRegister(Register.DIO_MAPPING1, (byte)(dioMapping1 | (value << 6)));
            }
        }

        public bool TxDone {
            get {
                return ((ReadRegister(Register.IRQ_FLAGS) << 3) & 0b1) == 0b1;
            }

            set {
                var dioMapping1 = ReadRegister(Register.IRQ_FLAGS) & 0b1011_1111;
                WriteRegister(Register.DIO_MAPPING1, (byte)(dioMapping1 | ((value ? 1 : 0) << 3)));
            }
        }

        public bool RxDone {
            get {
                return ((ReadRegister(Register.IRQ_FLAGS) << 6) & 0b1) == 0b1;
            }

            set {
                var dioMapping1 = ReadRegister(Register.IRQ_FLAGS) & 0b1011_1111;
                WriteRegister(Register.DIO_MAPPING1, (byte)(dioMapping1 | ((value ? 1 : 0) << 6)));
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
    }

    #endregion
}
