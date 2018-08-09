﻿using System;
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
            XmlDocument or = new XmlDocument();
            or.Load(@"D:\BiRT\data\OSOpenRoads_HZ.gml");
            XmlDocument osm = new XmlDocument();
            osm.Load(@"D:\BiRT\SmartGrid\SmartGrid\bin\Debug\netcoreapp2.0\map.osm");
            //edit for different bounds
            var maxCoord = new GeoCoordinate()
            {
                Latitude = 110,
                Longitude = 110
            };
            var minCoord = new GeoCoordinate()
            {
                Latitude = (decimal)-110,
                Longitude = (decimal)-110
            };
            SmartGrid BSmartGrid = SmartGrid.FromOSM(osm,maxCoord,minCoord,new SmartGrid());
            BSmartGrid = SmartGrid.FromOSOpenRoads(or,maxCoord,minCoord,BSmartGrid);
            //FromOSOpenRoads(or,maxCoord,minCoord);

            //      SmartGrid BSmartGrid =  SmartGrid.FromOSM(OsmLoad());
            //            ASmartGrid.RoadNodes.ToList().ForEach(x => BSmartGrid.RoadNodes.Add(x.Key, x.Value));
            //            ASmartGrid.RoadLinks.ToList().ForEach(x=>BSmartGrid.RoadLinks.Add(x.Key,x.Value));
            //dictionaryFrom.ToList().ForEach(x => dictionaryTo.Add(x.Key, x.Value));

            BSmartGrid.GeneratePNG();

            var a = BSmartGrid.RoadNodes.First().Key;
            var b = BSmartGrid.RoadNodes.ElementAt(4).Key;
            var c = BSmartGrid.RoadNodes.ElementAt(6).Key;
            Console.WriteLine(BSmartGrid.GetAngle(a, b, c));
            MapProcessor.ProcessFolder(verboseLoad: false, verboseWrite: false);

            Console.WriteLine(" end");
            Console.ReadLine();
        }

//        private static OSOpenRoads.OSOpenRoads OpenRoadLoad(string filepath)
//        {
//            OSOpenRoads.OSOpenRoads orsg = new OSOpenRoads.OSOpenRoads();
//            XmlDocument gml = new XmlDocument();
//
//            while (!File.Exists(filepath))
//            {
//                Console.WriteLine("Enter the .gml filepath");
//                filepath = Console.ReadLine();
//            }
//            gml.Load(filepath);
//            orsg.LoadFile(gml, true);
//            //var extension = QueryExtension();
//            //orsg.WriteToFile(extension: extension, verbose: true);
//
//            return orsg;
//        }
//
//        private static OSMRoads.OSMRoads OsmLoad()
//        {
//            OSMRoads.OSMRoads sm = new OSMRoads.OSMRoads();
//            XmlDocument osm = new XmlDocument();
//
//            if (!File.Exists($@"map.osm"))
//            {
//                var osmpath = "null";
//
//                while (!File.Exists(osmpath))
//                {
//                    Console.WriteLine("Enter .osm filepath: ");
//                    osmpath = Console.ReadLine();
//                }
//            }
//            else
//            {
//                osm.Load($@"map.osm");
//            }
//
//            Console.WriteLine("Verbose on load (y/n): ");
//            var verbose = (Console.ReadLine().ToLower() == "y");
//
//            sm.LoadFile(osm, verbose: verbose);
//
//            Console.WriteLine(" osm loaded");
//
//            var extension = QueryExtension();
//            sm.WriteToFile(extension: extension);
//
//            Console.WriteLine("Done");
//
//            return sm;
//        }

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