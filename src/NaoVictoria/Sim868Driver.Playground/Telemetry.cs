using System;
using System.Collections.Generic;
using System.Text;

namespace Sim868HttpClient
{
    public class TelemetryData
    {
        public long Timestamp { get; internal set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Notes { get; set; }
    }
}
