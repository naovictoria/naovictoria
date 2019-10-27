using System;
using NaoVictoria.NavEngine.Sensors;

namespace NaoVictoria.NavEngine
{
    public class CollisionAvoidanceDirection
    {
        private bool _isInCollisionAvoidance;
        private ICurrentDirectionSensor _currentDirectionSensor;

        public CollisionAvoidanceDirection(ICurrentDirectionSensor currentDirectionSensor)
        {
            _currentDirectionSensor = currentDirectionSensor;
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
