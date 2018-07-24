using System;
using System.Collections.Generic;
using System.Xml;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.IO;
using OSOpenRoads;
using OSMRoads;

namespace SmartGrid
{
    class Program
    {
        static void Print(XmlNode node, string prefix = "", Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.None) {
            Console.Write($"{prefix} {Newtonsoft.Json.JsonConvert.SerializeXmlNode(node, formatting)}");
        }

        static void Main(string[] args)
        {
            //string fileFormat = "";

            //while (!(fileFormat == "OSM" || fileFormat == "OR"))
            //{
            //    Console.WriteLine("Loading OSM or OpenRoad? (OSM||OR)");
            //    fileFormat = Console.ReadLine().ToUpper();
            //}
            //if (fileFormat == "OSM") OsmLoad();
            //else OpenRoadLoad();

            MapProcessor.ProcessFolder(verboseLoad: true, verboseWrite: true);

            Console.WriteLine(" end");
            Console.ReadLine();
        }

        private static void OpenRoadLoad()
        {
            OSOpenRoads.OSOpenRoads orsg = new OSOpenRoads.OSOpenRoads();
            XmlDocument gml = new XmlDocument();
           
            string filepath="";
            while (!File.Exists(filepath))
            {
                Console.WriteLine("Enter the .gml filepath");
                filepath = Console.ReadLine();
            }
            gml.Load(filepath);
            orsg.LoadFile(gml, true);
            var extension = QueryExtension();
            orsg.WriteToFile(extension: extension, verbose: true);

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