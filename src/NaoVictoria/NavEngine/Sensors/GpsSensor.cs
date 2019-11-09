using NaoVictoria.NavEngine.Models;
using NaoVictoria.Sim868Driver;
using NaoVictoria.Sim868Driver.Gps;
using NaoVictoria.Sim868Driver.Models;
using System;
using System.Threading.Tasks;

namespace NaoVictoria.NavEngine.Sensors
{
    public class GpsSensor : ICurrentPositionSensor
    {
        private readonly GpsApi _gpsAPi;
        private GnssNavInfo _lastNavInfo = new GnssNavInfo();

        public GpsSensor(Driver driver)
        {
            _gpsAPi = new GpsApi(driver, TimeSpan.FromSeconds(15));
            _gpsAPi.DataAvailable += GpsAPi_DataAvailable;
            var task = _gpsAPi.StartAsync();
            Task.WaitAll(new Task[] { task });
        }

        private Task GpsAPi_DataAvailable(object sender, GnssNavInfo navInfo)
        {
            _lastNavInfo = navInfo;
            return Task.CompletedTask;
        }

        public GeoPoint GetReading()
        {
            return new GeoPoint(_lastNavInfo.Longitude, _lastNavInfo.Latitude);
        }
    }
}
