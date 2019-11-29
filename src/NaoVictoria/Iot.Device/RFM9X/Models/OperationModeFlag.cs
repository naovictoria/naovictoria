using System;

namespace Iot.Device.RadioTransceiver.Models.RFM9X
{
    [Flags]
    public enum OperationModeFlag
    {
        SLEEP = 0b0000_0000,
        STANDBY = 0b0000_0001,
        FS_TX = 0b0000_0010,
        TX = 0b0000_0011,
        FS_RX = 0b0000_0100,
        RX = 0b0000_0101
    }
}
