using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;

namespace SmartGrid
{
    internal class Program
    {
        private static void Print(XmlNode node, string prefix = "", Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.None)
        {
            Console.Write($"{prefix} {Newtonsoft.Json.JsonConvert.SerializeXmlNode(node, formatting)}");
        }

        private static void Main(string[] args)
        {

            //            string fileFormat = "";

            //            while (!(fileFormat == "OSM" || fileFormat == "OR"))
            //            {
            //                Console.WriteLine("Loading OSM or OpenRoad? (OSM||OR)");
            //                fileFormat = Console.ReadLine().ToUpper();
            //            }
            //            if (fileFormat == "OSM") OsmLoad();
            //            else OpenRoadLoad();
            SmartGrid smartGrid = SmartGrid.FomOSOpenRoads(OpenRoadLoad(@"D:\BiRT\data\OSOpenRoads_NA.gml"));
            var a = smartGrid.RoadNodes.First().Key;
            var b = smartGrid.RoadNodes.ElementAt(4).Key;
            var c = smartGrid.RoadNodes.ElementAt(6).Key;
            Console.WriteLine(smartGrid.GetAngle(a,b,c));
            MapProcessor.ProcessFolder(verboseLoad: false, verboseWrite: false);

            Console.WriteLine(" end");
            Console.ReadLine();
        }

        private static OSOpenRoads.OSOpenRoads OpenRoadLoad(string filepath)
        {
            OSOpenRoads.OSOpenRoads orsg = new OSOpenRoads.OSOpenRoads();
            XmlDocument gml = new XmlDocument();

            while (!File.Exists(filepath))
            {
                Console.WriteLine("Enter the .gml filepath");
                filepath = Console.ReadLine();
            }
            gml.Load(filepath);
            orsg.LoadFile(gml, true);
            //var extension = QueryExtension();
            //orsg.WriteToFile(extension: extension, verbose: true);

            return orsg;
        }

        private static void OsmLoad()
        {
            OSMRoads.OSMRoads sm = new OSMRoads.OSMRoads();
            XmlDocument osm = new XmlDocument();

            if (!File.Exists($@"map.osm"))
            {
                var osmpath = "null";

                while (!File.Exists(osmpath))
                {
                    Console.WriteLine("Enter .osm filepath: ");
                    osmpath = Console.ReadLine();
                }
            }
            else
            {
                osm.Load($@"map.osm");
            }

            Console.WriteLine("Verbose on load (y/n): ");
            var verbose = (Console.ReadLine().ToLower() == "y");

            sm.LoadFile(osm, verbose: verbose);

            Console.WriteLine(" osm loaded");

            var extension = QueryExtension();
            sm.WriteToFile(extension: extension);

            Console.WriteLine("Done");
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