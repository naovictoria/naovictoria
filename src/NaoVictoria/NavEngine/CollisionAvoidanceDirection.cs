using System;
using NaoVictoria.NavEngine.Sensors;

namespace NaoVictoria.NavEngine
{
    public class CollisionAvoidanceDirection
    {
        private bool _isInCollisionAvoidance;
        private ICurrentOrientationSensor _currentOrientationSensor;
        private ICollisionSensor _collisionSensor;

        public CollisionAvoidanceDirection(ICurrentOrientationSensor currentOrientationSensor, ICollisionSensor collisionSensor)
        {
            _currentOrientationSensor = currentOrientationSensor;
            _collisionSensor = collisionSensor;
        }

        public double GetDirectionInRadians()
        {
            // TODO: Determine a path that avoid colliding with detected objects.
            throw new NotImplementedException();
        }

        public bool IsInCollisionAvoidance()
        {
            return _isInCollisionAvoidance;
        }
    }
}
