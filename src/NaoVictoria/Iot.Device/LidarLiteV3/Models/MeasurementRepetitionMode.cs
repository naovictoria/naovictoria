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
        Repeat,

        /// <summary>
        /// Measurements are done infinitely.
        /// </summary>
        RepeatIndefintely
    }
}
