using NaoVictoria.Models;
using System;
using System.Collections.Generic;

namespace NaoVictoria.NavEngine
{
    public class LandCollisionAvoidance
    {
        private object _destinationPoint;
        private IEnumerable<GeoPoint> _worldOceanPoints;
        private bool _isInCollisionAvoidance;

        public LandCollisionAvoidance(IEnumerable<GeoPoint> worldOceanPoints)
        {
            this._worldOceanPoints = worldOceanPoints;
        }

        public LandCollisionAvoidance(GeoPoint destinationPoint, IEnumerable<GeoPoint> worldOceanPoints)
        {
            _destinationPoint = destinationPoint;
            _worldOceanPoints = worldOceanPoints;
        }

        public double GetDirectionInRadians(GeoPoint nextRouteCheckPoint)
        {
            // TODO: Determine a path that avoids land to destination point.
            throw new NotImplementedException();
        }

        public bool IsInCollisionAvoidance()
        {
            return _isInCollisionAvoidance;
        }
    }
}
