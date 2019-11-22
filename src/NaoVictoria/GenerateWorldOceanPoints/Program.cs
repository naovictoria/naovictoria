using System;

namespace NaoVictoria.Utilities.GenerateWorldOceanPoints
{
    class Program
    {
        static void Main(string[] args)
        {
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
