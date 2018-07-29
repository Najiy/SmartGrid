using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace SmartGrid
{
    internal interface IMapParser
    {
        void LoadFile(XmlDocument or, bool verbose = false);

        void LoadFile(string osmPath, bool verbose = false);

        void WriteCsvs(string outFolder, bool verbose = false, string filenamePostfix = "");

        void WriteJsons(string outFolder, string filenamePostfix = "");

        void WriteToFile(string outFolder = "ORSmartGrid", string extension = "json", string filenamePostfix = "", bool verbose = false, bool pauseWhenDone = false);
    }

    public class MapProcessor
    {
        public static void ProcessFolder(string outFolder = "MapProcessorResult", string inFolder = "MapProcessorInputs", bool verboseLoad = false, bool verboseWrite = false)
        {
            Directory.CreateDirectory(outFolder);
            Directory.CreateDirectory(inFolder);

            //foreach (var item in Directory.GetFiles(inFolder))
            //{
            //    Console.WriteLine(item);
            //}

            //Console.ReadLine();

            //List<string> files = Directory.GetFiles(inFolder).ToList();

            //files.RemoveAll(x=>Path.GetExtension(x) != "osm" || Path.GetExtension(x) != "gml");

            foreach (var path in Directory.GetFiles(inFolder).ToList())
            {
                switch (Path.GetExtension(path))
                {
                    case ".osm":
                        Console.WriteLine($"OSM {Path.GetFileNameWithoutExtension(path)}");
                        OSMRoads.OSMRoads osmgrid = new OSMRoads.OSMRoads();

                        osmgrid.LoadFile(path);
                        osmgrid.WriteToFile();

                        Console.ReadLine();
                        break;

                    case ".gml":
                        Console.WriteLine($"GML {Path.GetFileNameWithoutExtension(path)}");
                        Console.ReadLine();
                        break;

                    default:
                        Console.WriteLine("unknown extension");
                        Console.ReadLine();
                        break;
                }
            }
        }
    }
}