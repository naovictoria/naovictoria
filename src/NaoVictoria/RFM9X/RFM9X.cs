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
            int frequencyMhz,
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

            _resetPinNumber = resetPinNumber;
            _device = SpiDevice.Create(settings);
            _controller = new GpioController(PinNumberingScheme.Board);
            _controller.OpenPin(_resetPinNumber, PinMode.InputPullUp);

            Reset();

            if (Version != 0x12)
            {
                throw new InvalidOperationException("Failed to find rfm9x with the expected version.");
            }

            OperationMode = OperationModeFlag.SLEEP;
            Thread.Sleep(10);
            IsLongRange = true;

            if (OperationMode != OperationModeFlag.SLEEP || !IsLongRange)
            {
                throw new InvalidOperationException("Failed to configure radio for LoRa mode.");
            }

            if (frequencyMhz > 525)
            {
                Console.WriteLine("before low freq mode: " + ReadRegister(Register.OP_MODE));
                IsLowFreqMode = false;
                Console.WriteLine("after low freq mode: " + ReadRegister(Register.OP_MODE));
            }

            WriteRegister(Register.FIFO_TX_BASE_ADDR, 0x00);
            WriteRegister(Register.FIFO_RX_BASE_ADDR, 0x00);

            Console.WriteLine("before standby: " + ReadRegister(Register.OP_MODE));
            OperationMode = OperationModeFlag.STANDBY;
            Thread.Sleep(1000);
            Console.WriteLine("after standby: " + ReadRegister(Register.OP_MODE));

            Console.WriteLine("before signal bandwidth: " + ReadRegister(Register.MODEM_CONFIG1));
            SignalBandwidth = SignalBandwidthFlag.BW_125000;
            Thread.Sleep(1000);
            Console.WriteLine("after signal bandwidth: " + ReadRegister(Register.MODEM_CONFIG1));
            CodingRate = 5;
            SpreadingFactor = 7;
            EnableCrc = false;
            PreambleLength = preambleLength;
            FrequencyMhz = frequencyMhz;
            TxPower = 13;

            Console.WriteLine("isLowFreq:" + IsLowFreqMode);
            Console.WriteLine("OperationMode:" + OperationMode);
            Console.WriteLine("SignalBandwidth:" + SignalBandwidth);
            Console.WriteLine("CodingRate:" + CodingRate);
            Console.WriteLine("SpreadingFactor:" + SpreadingFactor);
            Console.WriteLine("EnableCrc:" + EnableCrc);
            Console.WriteLine("PreambleLength:" + PreambleLength);
            Console.WriteLine("FrequencyMhz:" + FrequencyMhz);
            Console.WriteLine("TxPower:" + TxPower);
        }

        public SignalBandwidthFlag SignalBandwidth {
            get {
                var bwId = (ReadRegister(Register.MODEM_CONFIG1) & 0xf0) >> 4;
                return (SignalBandwidthFlag)(bwId);
            }

            set {
                byte oldValue = ReadRegister(Register.MODEM_CONFIG1);
                Console.WriteLine("writing sbf: " + (byte)((oldValue & ~0xf0) | (int)value << 4));
                WriteRegister(Register.OP_MODE, (byte)((oldValue & ~0xf0) | (int)value << 4));
            }
        }

        public int CodingRate {
            get {
                var crId = (ReadRegister(Register.MODEM_CONFIG1) & 0x0e) >> 4;
                return crId + 4;
            }

            set {
                byte oldValue = ReadRegister(Register.MODEM_CONFIG1);
                var crId = value - 4;
                WriteRegister(Register.OP_MODE, (byte)((oldValue & ~0x0e) | crId << 4));
            }
        }

        public int SpreadingFactor {
            get {
                var crId = (ReadRegister(Register.MODEM_CONFIG2) & 0xf0) >> 4;
                return crId;
            }

            set {
                WriteRegister(Register.DETECTION_OPTIMIZE, (byte)(value == 6 ? 0xc5 : 0xc3));
                WriteRegister(Register.DETECTION_THRESHOLD, (byte)(value == 6 ? 0x0c : 0x0a));

                byte oldValue = ReadRegister(Register.MODEM_CONFIG2);
                WriteRegister(Register.OP_MODE, (byte)((oldValue & ~0xf0) | oldValue << 4));
            }
        }

        public bool EnableCrc {
            get {
                return (ReadRegister(Register.MODEM_CONFIG2) & 0x04) >> 2 == 1;
            }

            set {
                byte oldValue = ReadRegister(Register.MODEM_CONFIG2);
                WriteRegister(Register.OP_MODE, (byte)((oldValue & ~0x04) | (value ? (1 << 2) : 0)));
            }
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
            OperationMode = OperationModeFlag.RX;
            Dio0Mapping = 0b00; // Interupt on RX done.
        }

        public void Transmit()
        {
            OperationMode = OperationModeFlag.TX;
            Dio0Mapping = 0b01; // Interupt on TX done.
        }

        public byte Version {
            get {
                return ReadRegister(Register.VERSION);
            }
        }

        public OperationModeFlag OperationMode {
            get {
                return (OperationModeFlag)(ReadRegister(Register.OP_MODE) & 0b111);
            }

            set {
                byte oldValue = ReadRegister(Register.OP_MODE);
                Console.WriteLine("writing: " + (byte)((oldValue & ~0b111) | (byte)value));
                WriteRegister(Register.OP_MODE, (byte)((oldValue & ~0b111) | (byte)value));
            }
        }

        public bool IsLowFreqMode {
            get {
                return ((ReadRegister(Register.OP_MODE) >> 3) & 1) == 1;
            }

            set {
                byte oldValue = ReadRegister(Register.OP_MODE);
                WriteRegister(Register.OP_MODE, (byte)((oldValue & ~(1 << 3)) | (value ? (1 << 3) : 0)));
            }
        }

        public bool IsLongRange {
            get {
                return ((ReadRegister(Register.OP_MODE) >> 7) & 1) == 1;
            }

            set {
                byte oldValue = ReadRegister(Register.OP_MODE);
                WriteRegister(Register.OP_MODE, (byte)((oldValue & ~(1 << 7)) | (value ? (1 << 7) : 0)));
            }
        }

        public int OutputPower {
            get {
                return ReadRegister(Register.PA_CONFIG) & 0b1111;
            }

            set {
                byte oldValue = ReadRegister(Register.PA_CONFIG);
                WriteRegister(Register.PA_CONFIG, (byte)((oldValue & ~0b1111) | (byte)value));
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
                var msb = ReadRegister(Register.PREAMBLE_MSB);
                var lsb = ReadRegister(Register.PREAMBLE_LSB);
                return (msb << 8) | lsb;
            }

            set {
                WriteRegister(Register.PREAMBLE_MSB, (byte)(value >> 8));
                WriteRegister(Register.PREAMBLE_LSB, (byte)(value & 0x0f));
            }
        }

        public int FrequencyMhz {
            get {
                var lsb = ReadRegister(Register.FRF_LSB);
                var mid = ReadRegister(Register.FRF_MID);
                var msb = ReadRegister(Register.FRF_MSB);
                var frf = (msb << 16) | (mid << 8) | lsb;

                // The crystal oscillator frequency of the module
                var _RH_RF95_FXOSC = 32000000.0;

                // The Frequency Synthesizer step = RH_RF95_FXOSC / 2^^19
                var _RH_RF95_FSTEP = (_RH_RF95_FXOSC / 524288);

                return (int)((frf * _RH_RF95_FSTEP) / 1000000.0);
            }

            set {
                // The crystal oscillator frequency of the module
                var _RH_RF95_FXOSC = 32000000.0;

                // The frequency synthesizer step = RH_RF95_FXOSC / 2^^19
                var _RH_RF95_FSTEP = (_RH_RF95_FXOSC / 524288);

                var frf = (int)((value * 1000000.0) / _RH_RF95_FSTEP);

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
                    if (value > 20)
                    {
                        EnablePaBoost = true;
                        value = -3;
                    }
                    else
                    {
                        EnablePaBoost = false;
                    }
                    PaSelect = true;
                    OutputPower = (value - 5) & 0x0f;
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
                byte oldValue = ReadRegister(Register.PA_DAC);
                WriteRegister(Register.PA_DAC, (byte)((oldValue & ~0b111) | (value ? 0b100 : 0b111)));
            }
        }

        public bool PaSelect {
            get {
                return ((ReadRegister(Register.PA_CONFIG) >> 7) & 1) == 1;
            }

            set {
                var oldValue = ReadRegister(Register.PA_CONFIG);
                WriteRegister(Register.PA_CONFIG, (byte)((oldValue & ~(1 << 7)) | (value ? (1 << 7) : 0)));
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
