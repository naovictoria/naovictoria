using System;
using System.Collections.Generic;
using System.Text;
using NaoVictoria.Devices.Interfaces;

namespace NaoVictoria.NavEngine
{
    public class SailingDirection
    {
        private ICurrentOrientationSensor _currentOrientationSensor;
        private ICurrentWindDirectionSensor _currentWindDirectionSensor;

        public SailingDirection(ICurrentOrientationSensor currentOrientationSensor, ICurrentWindDirectionSensor currentWindDirectionSensor)
        {
            _currentOrientationSensor = currentOrientationSensor;
            _currentWindDirectionSensor = currentWindDirectionSensor;
        }

        public double GetDirectionInRadians(double desiredDirection)
        {
            // TODO: Determine the best direction to move
            return 0.0;
        }
    }
}
