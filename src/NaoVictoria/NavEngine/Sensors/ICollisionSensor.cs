﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria.NavEngine.Sensors
{
    public interface ICollisionSensor
    {
        double GetDistanceToObject();
    }
}
