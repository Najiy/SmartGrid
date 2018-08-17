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

            var maxCoord = new GeoCoordinate()
            {
                Latitude = (decimal)100
               ,
                Longitude = (decimal)100
            };
            var minCoord = new GeoCoordinate()
            {
                Latitude = (decimal)-100,
                Longitude = (decimal)-100
            };
            XmlDocument or = new XmlDocument();
            or.Load(@"D:\BiRT\data\OSOpenRoads_NW.gml");
            XmlDocument osm = new XmlDocument();
            osm.Load(@"D:\BiRT\SmartGrid\SmartGrid\bin\Debug\netcoreapp2.0\NW.osm");
            //smartgrid will only take values within the bounds.
            SmartGrid BSmartGrid = SmartGrid.FromOSOpenRoads(or, maxCoord, minCoord, new SmartGrid());
            BSmartGrid = SmartGrid.FromOSM(osm, maxCoord, minCoord, BSmartGrid);




            SmartGrid.GeneratePNG(new OsmTile() { XCoord = 31830, YCoord = 20752 }, "D:/BiRT/SmartGrid/SmartGrid/bin/Debug/netcoreapp2.0/SmartGrid/JSON/31830/20752.json");
            //            WriteJson.CreateJsons(BSmartGrid);



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

