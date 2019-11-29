using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.NavEngine.Sensors.LidarLiteV3
{
    public enum PowerOption
    {
        Default = 0x80,
        DisableReceiverCircuit = 0x00,
        Sleep = 0x04
    }
}
