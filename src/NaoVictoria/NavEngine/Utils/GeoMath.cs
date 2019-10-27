using NaoVictoria.NavEngine.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.NavEngine.Utils
{
    /// <summary>
    /// Extention methods for calculating between Geo coords.
    /// </summary>
    /// <remarks>
    /// Source: https://www.movable-type.co.uk/scripts/latlong.html
    /// </remarks>
    public static class GeoMath
    {
        public static double ToRadians(this double degrees)
        {
            return Math.PI * degrees / 180.0;
        }

        public static double ToDegrees(this double radians)
        {
            return (180.0 / Math.PI) * radians;
        }

        public static double DistanceTo(this GeoPoint point1, GeoPoint point2)
        {
            var R = 6371e3; // metres
            var φ1 = point1.Latitude.ToRadians();
            var φ2 = point2.Latitude.ToRadians();
            var Δφ = (point2.Latitude - point1.Latitude).ToRadians();
            var Δλ = (point2.Longitude - point1.Longitude).ToRadians();

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        public static double BearingTo(this GeoPoint point1, GeoPoint point2)
        {
            var φ1 = point1.Latitude.ToRadians();
            var φ2 = point2.Latitude.ToRadians();
            var Δλ = (point2.Longitude - point1.Longitude).ToRadians();

            var y = Math.Sin(Δλ) * Math.Cos(φ2);
            var x = Math.Cos(φ1) * Math.Sin(φ2) -
                    Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(Δλ);

            return Math.Atan2(y, x);
        }
    }
}
