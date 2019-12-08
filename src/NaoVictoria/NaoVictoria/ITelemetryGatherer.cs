using NaoVictoria.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaoVictoria
{
    public interface ITelemetryGatherer
    {
        TelemetryData Gather();
    }
}
