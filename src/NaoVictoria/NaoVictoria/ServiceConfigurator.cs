using Microsoft.Extensions.DependencyInjection;
using NaoVictoria.Devices;
using NaoVictoria.Devices.Interfaces;
using NaoVictoria.NavEngine;

namespace NaoVictoria
{
    public class ServiceConfigurator
    {
        IServiceCollection _services;
        public ServiceConfigurator(IServiceCollection services)
        {
            _services = services;
        }

        public void Configure()
        {
            // _navEngine = new NavEngine.NavEngine(
            //     _orientationSensor,
            //     _gpsSensor,
            //     _windVaneSensor,
            //     _bowCollisionDetector,
            //     _rudderControl,
            //     _mainSailControl,
            //     _jibSailControl,
            //     worldOceanMap,
            //     globalPlan);

            // var sim868Driver = new Sim868Driver(new System.IO.Ports.SerialPort(), "/dev/ttyUSB0", 29);

            _services.AddSingleton<ICurrentOrientationSensor, OrientationSensor>();
            //_services.AddSingleton<ICurrentOrientationSensor, SimulatedOrientationSensor>();


            //_services.AddSingleton<ICurrentPositionSensor, GpsSensor>((ctx) =>
            //{
            //    return new GpsSensor(sim868Driver);
            //});
            _services.AddSingleton<ICurrentPositionSensor, SimulatedGpsSensor>();

            //_services.AddSingleton<ICurrentWindDirectionSensor, WindVaneSensor>();
            _services.AddSingleton<ICurrentWindDirectionSensor, SimulatedWindSensor>();

            _services.AddSingleton<ICollisionSensor, LidarSensor>();
            //_services.AddSingleton<ICollisionSensor, SimulatedCollisionSensor>();

            _services.AddSingleton<IMainSailControl, MainSailControl>();
            //_services.AddSingleton<IMainSailControl, SimulatedMainSailControl>();

             _services.AddSingleton<IJibSailControl, JibSailControl>();
            //_services.AddSingleton<IJibSailControl, SimulatedJibSailControl>();

            _services.AddSingleton<IRudderControl, RudderControl>();
            //_services.AddSingleton<IRudderControl, SimulatedRudderControl>();

            //IEnumerable< GeoPoint > worldOceanMap,
            //IEnumerable<GeoPoint> globalPlan

            _services.AddSingleton<ITelemetryGatherer, TelemetryGatherer>();
            _services.AddSingleton<ITelemetrySender, LoraTelemetrySender>();

            _services.AddSingleton<INavEngine, NavEngine.NavEngine>();
            _services.AddSingleton<INaoVictoriaEngine, NaoVictoriaEngine>();
            _services.AddHostedService<Worker>();
        }
    }
}
