using NaoVictoria.NavEngine.Sensors;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.NavEngine
{
    public class Locomotion
    {
        private ICurrentDirectionSensor _currentDirectionSensor;
        private ICurrentWindDirectionSensor _currentWindDirectionSensor;
        
        public Locomotion(ICurrentDirectionSensor currentDirectionSensor, ICurrentWindDirectionSensor currentWindDirectionSensor)
        {
            _currentDirectionSensor = currentDirectionSensor;
            _currentWindDirectionSensor = currentWindDirectionSensor;
        }

        public void RotateTo(double newDirection)
        {
            // TODO: USe current direction and wind direction to determine the optional rudder and sail positions.
        }

    }
}
