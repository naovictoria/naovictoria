using NaoVictoria.Models;
using System.Threading.Tasks;

namespace NaoVictoria
{
    public interface ITelemetrySender
    {
        Task SendTelementry(TelemetryData data);
    }
}
