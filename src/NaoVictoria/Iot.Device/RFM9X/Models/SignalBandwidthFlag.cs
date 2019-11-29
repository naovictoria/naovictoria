using System;

namespace Iot.Device.RadioTransceiver.Models.RFM9X
{
    [Flags]
    public enum SignalBandwidthFlag
    {
        BW_7800 = 0,
        BW_10400 = 1,
        BW_15600 = 2,
        BW_20800 = 3,
        BW_31250 = 4,
        BW_41700 = 5,
        BW_62500 = 6,
        BW_125000 = 7,
        BW_250000 = 8
    };
}
