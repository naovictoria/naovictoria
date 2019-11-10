using NaoVictoria.NavEngine.Controls;
using NaoVictoria.NavEngine.Models;
using NaoVictoria.NavEngine.Sensors;
using NaoVictoria.NavEngine.Utils;
using System.Collections.Generic;

namespace NaoVictoria.NavEngine
{
    public class RealNavEngine : INavEngine
    {
        ICurrentOrientationSensor _currentOrientationSensor;
        ICurrentPositionSensor _currentPositionSensor;
        ICurrentWindDirectionSensor _currentWindDirectionSensor;
        ICollisionSensor _collisionSensor;
        MainSailControl _mainSailControl;
        JibSailControl _jibSailControl;
        RudderControl _rudderControl;

        IEnumerable<GeoPoint> _worldOceanMap;
        IEnumerable<GeoPoint> _globalPlan;

        public RealNavEngine(
            ICurrentOrientationSensor currentOrientationSensor,
            ICurrentPositionSensor currentPositionSensor, 
            ICurrentWindDirectionSensor currentWindDirectionSensor,
            ICollisionSensor collisionSensor,
            RudderControl rudderControl,
            MainSailControl mainSailControl,
            JibSailControl jibSailControl,
            IEnumerable<GeoPoint> worldOceanMap,
            IEnumerable<GeoPoint> globalPlan)
        {
            _currentOrientationSensor = currentOrientationSensor;
            _currentPositionSensor = currentPositionSensor;
            _currentWindDirectionSensor = currentWindDirectionSensor;
            _collisionSensor = collisionSensor;
            _rudderControl = rudderControl;
            _mainSailControl = mainSailControl;
            _jibSailControl = jibSailControl;
            _worldOceanMap = worldOceanMap;
            _globalPlan = globalPlan;
        }

        public void Navigate()
        {
            RoutePlanner routePlanner = new RoutePlanner(_currentPositionSensor, _globalPlan);
            LandCollisionAvoidance landCollisionAvoidance = new LandCollisionAvoidance(_worldOceanMap);
            CollisionAvoidanceDirection collisionAvoidance = new CollisionAvoidanceDirection(_currentOrientationSensor, _collisionSensor);
            SailingDirection sailingDirection = new SailingDirection(_currentOrientationSensor, _currentWindDirectionSensor);
            Locomotion locomotion = new Locomotion(_currentOrientationSensor, _currentWindDirectionSensor);
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
