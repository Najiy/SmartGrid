using System;
using System.Collections.Generic;
using System.Xml;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.IO;

namespace SmartGrid
{
    class Program
    {
        static void print(XmlNode node, string prefix = "", Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.None) {
            Console.Write($"{prefix} {Newtonsoft.Json.JsonConvert.SerializeXmlNode(node, formatting)}");
        }

        static void Main(string[] args)
        {
            SmartGrid sm = new SmartGrid();
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

            sm.LoadOSM(osm, verbose: verbose);

            Console.WriteLine(" osm loaded");

            var extension = "";
            while (extension != "all" && extension != "json" && extension != "csv")
            {
                Console.WriteLine("extensions to output (all|json|csv): ");
                extension = Console.ReadLine();
            }

            Console.WriteLine(" writing to SmartGrid folder ...");
            sm.WriteToFile(extension: extension);


            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}