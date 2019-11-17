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
        private readonly int _resetPin;

        public RFM9X(
            int frequency,
            int busId = 0, 
            int chipSelectLine = 1,
            int resetPin = 0,
            int preambleLength = 8,
            bool highPower = true,
            int clockFrequency = 5000000)
        {
            var settings = new SpiConnectionSettings(busId, chipSelectLine);
            settings.ClockFrequency = clockFrequency;
            settings.Mode = SpiMode.Mode0;
            settings.DataBitLength = 8;
   
            _device = SpiDevice.Create(settings);

            GpioController controller = new GpioController(PinNumberingScheme.Board);
            controller.OpenPin(_resetPin, PinMode.InputPullUp);
            controller.ClosePin(_resetPin);

            Reset();

            var version = GetVersion();

            if(version != 0x12)
            {
                throw new InvalidOperationException("Failed to find rfm9x with the expected version.");
            }

            Sleep();

            Thread.Sleep(10);

            LongRangeMode = true;

            if(OperationMode != SleepMode || !LongRangeMode)
            {
                throw new InvalidOperationException("Failed to configure radio for LoRa mode.");
            }

            if(frequency > 525)
            {
                LowFrequencyMode = false;
            }

            _device.WriteByte(Register.FIFO_TX_BASE_ADDR, 0x00);
            _device.WriteByte(Register.FIFO_TX_BASE_ADDR, 0x00);

            Idle();

            SignalBandwidth = 125000;

        }

        #region Board elements

        public void Reset()
        {
            GpioController controller = new GpioController(PinNumberingScheme.Board);
            controller.OpenPin(_resetPin, PinMode.Output);
            controller.Write(_resetPin, PinValue.Low);
            System.Threading.Thread.Sleep(1);
            controller.OpenPin(_resetPin, PinMode.InputPullUp);
            controller.ClosePin(_resetPin);
            System.Threading.Thread.Sleep(1);
        }

        /// <summary>
        /// Read the 20 charactor BrickPi3 manufacturer name
        /// </summary>
        /// <returns>BrickPi3 manufacturer name string</returns>
        public byte GetVersion()
        {
            _device.WriteByte((byte)Register.VERSION);
            return _device.ReadByte();
        }

        #endregion
    }
}
