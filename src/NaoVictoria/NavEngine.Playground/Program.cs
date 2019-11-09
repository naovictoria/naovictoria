using NaoVictoria.NavEngine.Models;
using NaoVictoria.NavEngine.Sensors;
using NaoVictoria.NavEngine.Utils;
using NaoVictoria.Sim868Driver;
using NetTopologySuite.Features;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            Driver driver = new Driver(new System.IO.Ports.SerialPort(), "/dev/ttyS0");

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
            

            //using (var fileStream = File.OpenRead(@"C:\Users\vorn\dev\planet-191014.osm.pbf"))
            //{
            //    // create source stream.
            //    OsmStreamSource source = new PBFOsmStreamSource(fileStream);

            //    // filter it to just a particular region first.
            //    // source = source.FilterBox(-71.446194f, 42.852301f, -69.405375f, 41.033076f);

            //    // filter all powerlines and keep all nodes.
            //    var filtered = from osmGeo in source
            //                   where osmGeo.Type == OsmSharp.OsmGeoType.Node || 
            //                   (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.Contains("natural", "coastline"))
            //                   select osmGeo;

            //    // convert to a feature stream.
            //    // WARNING: nodes that are partof powerlines will be kept in-memory.
            //    //          it's important to filter only the objects you need **before** 
            //    //          you convert to a feature stream otherwise all objects will 
            //    //          be kept in-memory.
            //    var features = filtered.ToComplete();

            //    FeatureCollection featureCollection = new FeatureCollection();

            //    using (StreamWriter file = new System.IO.StreamWriter(@"natural_coastlines.txt"))
            //    {
            //        foreach (ICompleteOsmGeo completeOsmGeo in features)
            //        {
            //            if (completeOsmGeo.Type == OsmGeoType.Way)
            //            {
            //                foreach (var node in ((CompleteWay)completeOsmGeo).Nodes)
            //                {
            //                    file.WriteLine(node.Longitude + "," + node.Latitude);
            //                }
            //            }
            //        }
            //    }
            //}
        }
    }
}
