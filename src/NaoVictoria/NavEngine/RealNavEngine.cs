using NaoVictoria.NavEngine.Models;
using NaoVictoria.NavEngine.Sensors;
using NaoVictoria.NavEngine.Utils;
using System.Collections.Generic;

namespace NaoVictoria.NavEngine
{
    public class RealNavEngine : INavEngine
    {
        ICurrentDirectionSensor _currentDirectionSensor;
        ICurrentPositionSensor _currentPositionSensor;
        ICurrentWindDirectionSensor _currentWindDirectionSensor;

        IEnumerable<GeoPoint> _worldOceanMap;
        IEnumerable<GeoPoint> _globalPlan;

        public RealNavEngine(ICurrentDirectionSensor currentDirectionSensor,
            ICurrentPositionSensor currentPositionSensor, 
            ICurrentWindDirectionSensor currentWindDirectionSensor,
            IEnumerable<GeoPoint> worldOceanMap,
            IEnumerable<GeoPoint> globalPlan)
        {
            _currentDirectionSensor = currentDirectionSensor;
            _currentPositionSensor = currentPositionSensor;
            _currentWindDirectionSensor = currentWindDirectionSensor;

            _worldOceanMap = worldOceanMap;
            _globalPlan = globalPlan;
        }

        public void Navigate()
        {
            RoutePlanner routePlanner = new RoutePlanner(_currentPositionSensor, _globalPlan);
            LandCollisionAvoidance landCollisionAvoidance = new LandCollisionAvoidance(_worldOceanMap);
            CollisionAvoidanceDirection collisionAvoidance = new CollisionAvoidanceDirection(_currentDirectionSensor);
            SailingDirection sailingDirection = new SailingDirection(_currentDirectionSensor, _currentWindDirectionSensor);
            Locomotion locomotion = new Locomotion(_currentDirectionSensor, _currentWindDirectionSensor);
            double directionRadian;

            var nextRouteCheckPoint = routePlanner.GetNextClosestCheckpoint();

            directionRadian = _currentPositionSensor.GetReading().BearingTo(nextRouteCheckPoint);

            if (landCollisionAvoidance.IsInCollisionAvoidance())
            {
                directionRadian = landCollisionAvoidance.GetDirectionInRadians(nextRouteCheckPoint);
            }

            if (collisionAvoidance.IsInCollisionAvoidance())
            {
                directionRadian = collisionAvoidance.GetDirectionInRadians();
            }

            directionRadian = sailingDirection.GetDirectionInRadians(directionRadian);

            locomotion.RotateTo(directionRadian);
        }
    }
}
