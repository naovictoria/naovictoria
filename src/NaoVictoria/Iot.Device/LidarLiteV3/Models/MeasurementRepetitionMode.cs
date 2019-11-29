using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.TimeOfFlight.Models.LidarLiteV3
{
    /// <summary>
    /// Measurement repeition modes
    /// </summary>
    public enum MeasurementRepetitionMode
    {
        /// <summary>
        /// Disabled, measurements are done once per command.
        /// </summary>
        Off,

        /// <summary>
        /// Measurements are done repetitively per command defined in OUTER_LOOP_COUNT.
        /// </summary>
        Count,

        /// <summary>
        /// Measurements are done infinitely.
        /// </summary>
        Indefintely
    }
}
