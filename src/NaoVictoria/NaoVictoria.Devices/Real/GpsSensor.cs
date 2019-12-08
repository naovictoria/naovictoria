using NaoVictoria.Models;
using NaoVictoria.Devices.Interfaces;
using System;
using System.Threading.Tasks;
using NaoVictoria.Sim868.Gps;
using NaoVictoria.Sim868.Models;

namespace NaoVictoria.Devices
{
    public class GpsSensor : ICurrentPositionSensor
    {
        private readonly GpsApi _gpsAPi;
        private GnssNavInfo _lastNavInfo;

        public GpsSensor(Sim868Driver driver)
        {
            _gpsAPi = new GpsApi(driver, TimeSpan.FromSeconds(15));
            _lastNavInfo = new GnssNavInfo();
            _lastNavInfo.Latitude = 0.0;
            _lastNavInfo.Longitude = 0.0;
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
