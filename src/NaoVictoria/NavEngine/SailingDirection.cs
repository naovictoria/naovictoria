using NaoVictoria.NavEngine.Sensors;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.NavEngine
{
    public class SailingDirection
    {
        private ICurrentDirectionSensor _currentDirectionSensor;
        private ICurrentWindDirectionSensor _currentWindDirectionSensor;

        public SailingDirection(ICurrentDirectionSensor currentDirectionSensor, ICurrentWindDirectionSensor currentWindDirectionSensor)
        {
            _currentDirectionSensor = currentDirectionSensor;
            _currentWindDirectionSensor = currentWindDirectionSensor;
        }

        public double GetDirectionInRadians(double desiredDirection)
        {
            // TODO: Determine the best direction to move
            return 0.0;
        }
    }
}
