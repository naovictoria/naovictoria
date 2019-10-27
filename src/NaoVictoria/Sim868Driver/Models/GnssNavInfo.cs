using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.Sim868Driver.Models
{
    public class GnssNavInfo
    {
        public int GnssRunStatus { get; set; }
        public int FixStatus { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        /// <summary>
        /// MSL (Mean Sea Level) Altitude
        /// </summary>
        public double MslAltitude { get; set; }
        
        public double SpeedOverGround { get; set; }
        public double CourseOverGround { get; set; }
        public double FixMode { get; set; }
        /// <summary>
        /// HDOP (Geodesy)
        /// </summary>
        public double HDop { get; set; }
        
        /// <summary>
        /// PDOP (Position Dilution of Precision)
        /// </summary>
        public double PDop { get; set; }

        /// <summary>
        /// DOP (Dilution of Precision)
        /// </summary>
        public double VDop { get; set; }
        public int GpsSatInView { get; set; }
        public int GnssSatUsed { get; set; }
        public int GlonassSatUSed { get; set; }

        /// <summary>
        /// C/N0 Max
        /// </summary>
        public int CnoMax { get; set; }

        /// <summary>
        /// Horizontal Position Accuracy
        /// </summary>
        public double Hpa { get; set; }

        /// <summary>
        /// Vertical Position Accuracy
        /// </summary>
        public double Vpa { get; set; }
    }
}
