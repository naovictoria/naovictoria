using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.TimeOfFlight.Models.LidarLiteV3
{
    public enum PowerOption
    {
        Default = 0x80,
        DisableReceiverCircuit = 0x00,
        Sleep = 0x04
    }
}
