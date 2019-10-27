using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NaoVictoria
{
    internal class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly INaoVictoriaEngine _engine;

        public Worker(ILogger<Worker> logger, INaoVictoriaEngine engine)
        {
            _logger = logger;
            _engine = engine;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _engine.DoWork();
                await Task.Delay(500, stoppingToken);
            }
        }
    }
}
