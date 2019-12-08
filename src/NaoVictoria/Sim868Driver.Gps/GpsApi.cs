using NaoVictoria.Sim868.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NaoVictoria.Sim868.Gps
{
    public delegate Task DataAvailableEventHandler(object sender, GnssNavInfo navInfo);

    public class GpsApi
    {
        Sim868Driver _driver;
        CancellationTokenSource _taskCancellationTokenSource = new CancellationTokenSource();
        Task _pollerTask;
        TimeSpan _waitBetweenUpdates;

        public event DataAvailableEventHandler DataAvailable;
        
        public GpsApi(Sim868Driver driver, TimeSpan waitBetweenUpdates)
        {
            _driver = driver;
            _waitBetweenUpdates = waitBetweenUpdates;
        }

        public async Task StartAsync()
        {
            if(_pollerTask != null)
            {
                // Already running!
                return;
            }

            // Activate the GPS module
            await _driver.ActivateGpsModuleAsync();

            var token = _taskCancellationTokenSource.Token;

            _pollerTask = Task.Run(async () =>
            {
                token.ThrowIfCancellationRequested();

                while (!token.IsCancellationRequested)
                {
                    if (DataAvailable != null)
                    {
                        var data = await _driver.GetGnssNavInfoAsync();
                        await DataAvailable(this, data);
                    }
                    await Task.Delay(_waitBetweenUpdates);
                }
            }, token);
        }

        public async Task StopAsync()
        {
            _taskCancellationTokenSource.Cancel();

            try
            {
                await _pollerTask;
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                _pollerTask.Dispose();
                _pollerTask = null;
                await _driver.ActivateGpsModuleAsync(false);
            }
        }
    }
}
