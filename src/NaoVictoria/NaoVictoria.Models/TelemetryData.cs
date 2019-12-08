using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.Models
{
    public class TelemetryData
    {
        public long Timestamp { get; set; }
        public string Notes { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public int BowDistanceToCollisionCm { get; set; }
        public double OrientationHeading { get; set; }
        public double OrientationPitch { get; set; }
        public double OrientationRoll { get; set; }
        public double WindVaneDirection { get; set; }
    }
}
