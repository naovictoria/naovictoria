using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.Devices.Interfaces
{
    public interface IJibSailControl : ISailControl
    {
        void MoveTo(double sailAngle);
    }
}
