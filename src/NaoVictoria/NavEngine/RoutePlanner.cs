using System.Collections.Generic;
using NaoVictoria.Devices.Interfaces;
using NaoVictoria.Models;

namespace NaoVictoria.NavEngine
{
    public class RoutePlanner
    {
        private ICurrentPositionSensor _currentPositionSensor;
        private IEnumerable<GeoPoint> _route;

        public RoutePlanner(ICurrentPositionSensor currentPositionSensor, IEnumerable<GeoPoint> route)
        {
            _currentPositionSensor = currentPositionSensor;
            _route = route;
        }
        
        public GeoPoint GetNextClosestCheckpoint()
        {
            return new GeoPoint(0.0f, 0.0f);
        }
    }
}
