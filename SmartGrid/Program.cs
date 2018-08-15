using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using OsmSharp;
namespace SmartGrid
{
    internal class Program
    {
        private static void Print(XmlNode node, string prefix = "",
            Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.None)
        {
            Console.Write($"{prefix} {Newtonsoft.Json.JsonConvert.SerializeXmlNode(node, formatting)}");
        }

        private static void Main(string[] args)
        {

            //take latlon and find osm tile bounds defined for it
            var latLon = LatLonConversions.ConvertOSToLatLon(181717.46, 658994.99);

            OsmTileConversion conversion = new OsmTileConversion();
            //
            var OsmTile = conversion.CoordToOsmTile(new GeoCoordinate()
            {
                Latitude = (decimal)latLon.Latitude,
                Longitude = (decimal)latLon.Longitude
            }, 16);
            //set tile bounds as bounds for smartgrid object
//            var maxCoord = conversion.OsmTileToCoord(new KeyValuePair<int, int>(OsmTile.Key, OsmTile.Value - 1), 16);
//            var minCoord =
//                conversion.OsmTileToCoord(new KeyValuePair<int, int>(OsmTile.Key - 1, OsmTile.Value), 16);
            //to be used if bounds aren't necessary
                        var maxCoord = new GeoCoordinate()
                        {
                            Latitude = 1000,
                            Longitude = 1000
                        };
                        var minCoord = new GeoCoordinate()
                        {
                            Latitude = -1000,
                            Longitude = -1000
                        };
            XmlDocument or = new XmlDocument();
            or.Load(@"D:\BiRT\data\OSOpenRoads_NW.gml");
            XmlDocument osm = new XmlDocument();
            osm.Load(@"D:\BiRT\SmartGrid\SmartGrid\bin\Debug\netcoreapp2.0\map (1).osm");
            //smartgrid will only take values within the bounds.
            //            SmartGrid BSmartGrid = SmartGrid.FromOSOpenRoads(or, maxCoord, minCoord, new SmartGrid());
            SmartGrid BSmartGrid = SmartGrid.FromOSM(osm, maxCoord, minCoord, new SmartGrid());

            //            BSmartGrid.GeneratePNG(OsmTile);
            WriteJson.CreateJsons(BSmartGrid);

            MapProcessor.ProcessFolder(verboseLoad: false, verboseWrite: false);

            Console.WriteLine(" end");
            Console.ReadLine();
        }

        private static string QueryExtension()
        {
            var extension = "";
            while (extension != "all" && extension != "json" && extension != "csv")
            {
                Console.WriteLine("extensions to output (all|json|csv): ");
                extension = Console.ReadLine().ToLower();
            }

            Console.WriteLine(" writing to SmartGrid folder ...");
            return extension;
        }
    }
}

