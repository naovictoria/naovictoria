using NaoVictoria.NavEngine.Sensors;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.NavEngine
{
    public class Locomotion
    {
        private ICurrentOrientationSensor _currentOrientationSensor;
        private ICurrentWindDirectionSensor _currentWindDirectionSensor;
        
        public Locomotion(ICurrentOrientationSensor currentOrientationSensor, ICurrentWindDirectionSensor currentWindDirectionSensor)
        {
            _currentOrientationSensor = currentOrientationSensor;
            _currentWindDirectionSensor = currentWindDirectionSensor;
        }

        public void RotateTo(double newDirection)
        {
            // TODO: USe current direction and wind direction to determine the optional rudder and sail positions.
        }

    }
}
