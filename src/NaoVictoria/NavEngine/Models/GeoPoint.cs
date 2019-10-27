using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.NavEngine.Models
{
    public class GeoPoint
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public GeoPoint(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }
}
