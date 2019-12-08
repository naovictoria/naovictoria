using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.Devices.Interfaces
{
    public interface IRudderControl
    {
        void MoveTo(double angleRadians);
    }
}
