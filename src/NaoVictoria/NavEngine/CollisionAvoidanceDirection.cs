using System;
using NaoVictoria.NavEngine.Sensors;

namespace NaoVictoria.NavEngine
{
    public class CollisionAvoidanceDirection
    {
        private bool _isInCollisionAvoidance;
        private ICurrentDirectionSensor _currentDirectionSensor;
        private ICollisionSensor _collisionSensor;

        public CollisionAvoidanceDirection(ICurrentDirectionSensor currentDirectionSensor, ICollisionSensor collisionSensor)
        {
            _currentDirectionSensor = currentDirectionSensor;
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
