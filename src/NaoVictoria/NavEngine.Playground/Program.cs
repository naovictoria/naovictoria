using NaoVictoria.Devices;
using NaoVictoria.Models;
using NaoVictoria.NavEngine.Utils;
using NaoVictoria.Sim868.Gps;
using System.Collections.Generic;

namespace NaoVictoria.NavEngine.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            List<GeoPoint> plan = new List<GeoPoint>() {
                new GeoPoint(-70.49648,41.95163),
                new GeoPoint(-70.29465,41.81431),
                new GeoPoint(-70.20813,41.52503)
            };

            List<GeoPoint> oceanWorldMap = new List<GeoPoint>()
            {

            };

            Sim868Driver driver = new Sim868Driver(new System.IO.Ports.SerialPort(), "/dev/ttyS0", 29);

            GpsSensor gpsSensor = new GpsSensor(driver);
            OrientationSensor compassSensor = new OrientationSensor();
            WindVaneSensor windVaneSensor = new WindVaneSensor();
            LidarSensor lidarSensor = new LidarSensor();

            RoutePlanner routePlanner = new RoutePlanner(gpsSensor, plan);
            LandCollisionAvoidance landCollisionAvoidance = new LandCollisionAvoidance(oceanWorldMap);
            CollisionAvoidanceDirection collisionAvoidance = new CollisionAvoidanceDirection(compassSensor, lidarSensor);
            SailingDirection sailingDirection = new SailingDirection(compassSensor, windVaneSensor);
            Locomotion locomotion = new Locomotion(compassSensor, windVaneSensor);

            while (true)
            {
                double directionRadian;

                var nextRouteCheckPoint = routePlanner.GetNextClosestCheckpoint();

                directionRadian = gpsSensor.GetReading().BearingTo(nextRouteCheckPoint);

                if (landCollisionAvoidance.IsInCollisionAvoidance())
                {
                    directionRadian = landCollisionAvoidance.GetDirectionInRadians(nextRouteCheckPoint);
                }

                if (collisionAvoidance.IsInCollisionAvoidance())
                {
                    directionRadian = collisionAvoidance.GetDirectionInRadians();
                }

                directionRadian = sailingDirection.GetDirectionInRadians(directionRadian);

                locomotion.RotateTo(directionRadian);
            }            
        }
    }
}
