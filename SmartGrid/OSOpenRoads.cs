using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Xml;
using SmartGrid;

namespace OSOpenRoads
{
    public struct Bounds
    {
        public double MinLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLat { get; set; }
        public double MaxLon { get; set; }
    }
    public struct RoadNode
    {
        public string Id { get; set; }
        public bool BeginLifespanVersion { get; set; }
        public bool InNetwork { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string FormOfRoadNode { get; set; }
        public string RoadType { get; set; }

    }
    public struct RoadLink
    {
        public string Id { get; set; }
        public string StartNode { get; set; }
        public string EndNode { get; set; }
        public bool BeginLifespanVersion { get; set; }
        public string CentrelineGeometry { get; set; }
        public bool Fictitious { get; set; }
        public bool InNetwork { get; set; }
        public string RoadClassification { get; set; }
        public string RoadFunction { get; set; }
        public string RoadType { get; set; }
        public string FormOfWay { get; set; }
        public string RoadClassificationNumber { get; set; }
        public string Length { get; set; }
        public string UnitsOfMeasure { get; set; }
        public bool Loop { get; set; }
        public bool PrimaryRoute { get; set; }
        public bool TrunkRoad { get; set; }
        public string RoadName { get; set; }
        public string RoadNameToid { get; set; }
        public string RoadNumberToid { get; set; }
    }

    public class OSOpenRoads : IMapParser
    {
        public Bounds Bounds;
        public List<RoadLink> RoadLinks = new List<RoadLink>();
        public List<RoadNode> RoadNodes = new List<RoadNode>();

        public void LoadFile(XmlDocument or, bool verbose = false)
        {

            XmlNode node = or.DocumentElement.FirstChild;
            for (int a = 0; a < or.DocumentElement.ChildNodes.Count; a++)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    var currentNode = node.ChildNodes.Item(i);
                    switch (currentNode.Name)
                    {
                        case "gml:Envelope":
                            for (int z = 0; z < currentNode.ChildNodes.Count; z++)
                            {
                                var name = currentNode.ChildNodes.Item(z).Name;
                                switch (name)
                                {
                                    case "gml:lowerCorner":
                                        var LowerCorner = currentNode.ChildNodes.Item(0).InnerText.Split(" ");
                                        Bounds.MinLat = double.Parse(LowerCorner[0]);
                                        Bounds.MinLon = double.Parse(LowerCorner[1]);
                                        break;
                                    case "gml:upperCorner":
                                        var UpperCorner = currentNode.ChildNodes.Item(1).InnerText.Split(" ");
                                        Bounds.MaxLat = double.Parse(UpperCorner[0]);
                                        Bounds.MaxLon = double.Parse(UpperCorner[1]);
                                        break;
                                }

                                if (verbose)
                                    Console.WriteLine($"Bounds << MaxLat {Bounds.MaxLat}  MaxLon {Bounds.MaxLon}  MinLat {Bounds.MinLat}  MinLon {Bounds.MinLon}");
                            }
                            break;

                        case "road:RoadNode":
                            var roadNode = new RoadNode();
                            roadNode.Id = currentNode.Attributes.GetNamedItem("gml:id").Value;
                            for (int z = 0; z < currentNode.ChildNodes.Count; z++)
                            {

                                switch (currentNode.ChildNodes.Item(z).Name)
                                {
                                    case "net:beginLifespanVersion":
                                        roadNode.BeginLifespanVersion =
                                            Convert.ToBoolean(currentNode.ChildNodes.Item(z).Attributes.GetNamedItem("xsi:nil").Value);
                                        break;
                                    case "net:inNetwork":
                                        roadNode.InNetwork =
                                            Convert.ToBoolean(currentNode.ChildNodes.Item(z).Attributes.GetNamedItem("xsi:nil").Value);
                                        break;
                                    case "net:geometry":
                                        var location = currentNode.ChildNodes.Item(z).InnerText.Split(" ");
                                        roadNode.Latitude = double.Parse(location[0]);
                                        roadNode.Longitude = double.Parse(location[1]);
                                        break;
                                    case "tn-ro:formOfRoadNode":
                                        roadNode.FormOfRoadNode = currentNode.ChildNodes.Item(z).Attributes
                                            .GetNamedItem("codeSpace").Value;
                                        roadNode.RoadType = currentNode.ChildNodes.Item(z).InnerText;
                                        break;
                                }

                            }
                            RoadNodes.Add(roadNode);

                            if (verbose)
                                Console.WriteLine($"Node << {roadNode.Id} {roadNode.Latitude} {roadNode.Longitude} {roadNode.RoadType}");

                            break;
                        case "road:RoadLink":
                            {
                                var roadLink = new RoadLink { Id = currentNode.Attributes.GetNamedItem("gml:id").Value };
                                var blv = currentNode.ChildNodes.Where(x => x.Name == "net:beginLifespanVersion");
                                roadLink.BeginLifespanVersion =
                                    Convert.ToBoolean(blv.FirstOrDefault()?.Attributes.GetNamedItem("xsi:nil").Value);
                                var inn = currentNode.ChildNodes.Where(x => x.Name == "net:inNetwork");
                                roadLink.InNetwork =
                                    Convert.ToBoolean(inn.FirstOrDefault()?.Attributes.GetNamedItem("xsi:nil").Value);
                                var clg = currentNode.ChildNodes.Where(x => x.Name == "net:centrelineGeometry");
                                roadLink.CentrelineGeometry = clg.FirstOrDefault()?.InnerText;
                                var fic = currentNode.ChildNodes.Where(x => x.Name == "net:fictitious");
                                roadLink.Fictitious = Convert.ToBoolean(fic.FirstOrDefault()?.InnerText);
                                var sn = currentNode.ChildNodes.Where(x => x.Name == "net:startNode");
                                roadLink.StartNode = sn.FirstOrDefault()?.Attributes.GetNamedItem("xlink:href").Value;
                                var en = currentNode.ChildNodes.Where(x => x.Name == "net:endNode");
                                roadLink.StartNode = en.FirstOrDefault()?.Attributes.GetNamedItem("xlink:href").Value;
                                var rc = currentNode.ChildNodes.Where(x => x.Name == "road:roadClassification");
                                roadLink.RoadClassification = rc.FirstOrDefault()?.InnerText;
                                var rf = currentNode.ChildNodes.Where(x => x.Name == "road:roadFunction");
                                roadLink.RoadFunction = rf.FirstOrDefault()?.InnerText;
                                var fow = currentNode.ChildNodes.Where(x => x.Name == "road:formOfWay");
                                roadLink.FormOfWay = fow.FirstOrDefault()?.InnerText;
                                var len = currentNode.ChildNodes.Where(x => x.Name == "road:length");
                                roadLink.Length = len.FirstOrDefault()?.InnerText;
                                roadLink.UnitsOfMeasure = len.FirstOrDefault()?.Attributes.GetNamedItem("uom").Value;
                                var loop = currentNode.ChildNodes.Where(x => x.Name == "road:loop");
                                roadLink.Loop = Convert.ToBoolean(loop.FirstOrDefault()?.InnerText);
                                var pr = currentNode.ChildNodes.Where(x => x.Name == "road:primaryRoute");
                                roadLink.PrimaryRoute = Convert.ToBoolean(pr.FirstOrDefault()?.InnerText);
                                var tr = currentNode.ChildNodes.Where(x => x.Name == "road:trunkRoad");
                                roadLink.TrunkRoad = Convert.ToBoolean(tr.FirstOrDefault()?.InnerText);
                                var rn = currentNode.ChildNodes.Where(x => x.Name.Contains("road:name"));
                                roadLink.RoadName = rn.FirstOrDefault()?.InnerText;
                                var nametoid = currentNode.ChildNodes.Where(x => x.Name == "road:roadNameTOID");
                                roadLink.RoadNameToid =
                                    nametoid.FirstOrDefault()?.Attributes.GetNamedItem("xlink:href").Value;
                                var numbertoid = currentNode.ChildNodes.Where(x => x.Name == "road:roadNumberTOID");
                                roadLink.RoadNumberToid =
                                    numbertoid.FirstOrDefault()?.Attributes.GetNamedItem("xlink:href").Value;

                                if (verbose)
                                    Console.WriteLine($"Link << {roadLink.Id} {roadLink.RoadName} {roadLink.RoadType} {roadLink.RoadClassification}");

                                RoadLinks.Add(roadLink);
                                break;
                            }
                    }



                }
                node = node.NextSibling;
            }

        }

        public void LoadFile(string osmPath, bool verbose = false)
        {
            XmlDocument osm = new XmlDocument();
            osm.Load(osmPath);
            LoadFile(osm, verbose);
        }

        public void WriteCsvs(string outFolder, bool verbose = false, string filenamePostfix = "")
        {
            Directory.CreateDirectory($"{outFolder}\\CSV");
            using (var fileStream = File.CreateText($"{outFolder}\\CSV\\Bounds_{filenamePostfix}.csv"))
            {
                fileStream.WriteLine($"minlat,minlon,maxlat,maxlon");

                fileStream.WriteLine($"{Bounds.MinLat},{Bounds.MinLon},{Bounds.MaxLat},{Bounds.MaxLon}");
            }

            var nodecount = RoadNodes.Count;
            var linkcount = RoadLinks.Count;
            var totalcount = nodecount + linkcount;


            using (var fileStream = File.CreateText($"{outFolder}\\CSV\\RoadLinks_{filenamePostfix}.csv"))
            {
                fileStream.WriteLine("id,start node,end node,begin lifespan version,centerline geometry" +
                                     ",fictitious,In Network, Road Classification,Road function,Road type,FormOfWay," +
                                     "RoadClassificationNumber,Length,Units,Loop,PrimaryRoute,TrunkRoad,RoadName," +
                                     "RoadNameTOID,RoadNumberTOID");

                var i = 0;
                foreach (var link in RoadLinks)
                {
                    fileStream.WriteLine($"{link.Id},{link.StartNode},{link.EndNode},{link.BeginLifespanVersion},{link.CentrelineGeometry}" +
                                         $",{link.Fictitious},{link.InNetwork},{link.RoadClassification},{link.RoadClassification},{link.RoadType}" +
                                         $",{link.FormOfWay},{link.RoadClassificationNumber},{link.Length},{link.UnitsOfMeasure},{link.Loop}," +
                                         $"{link.PrimaryRoute},{link.TrunkRoad},{link.RoadName},{link.RoadNameToid},{link.RoadNumberToid}");

                    i++;
                    if (verbose)
                        Console.WriteLine($" {i}/{totalcount} writing link {link.Id}");
                }
            }
            using (var fileStream = File.CreateText(($"{outFolder}\\CSV\\RoadNodes_{filenamePostfix}.csv")))
            {
                fileStream.WriteLine("ID,Latitude,Longitude,BeginLifespanVersion,InNetwork,FormOfRoadNode,RoadType");

                var i = linkcount;
                foreach (var node in RoadNodes)
                {
                    fileStream.WriteLine($"{node.Id},{node.Latitude},{node.Longitude},{node.BeginLifespanVersion},{node.InNetwork},{node.FormOfRoadNode},{node.RoadType}");

                    i++;
                    if (verbose)
                        Console.WriteLine($" {i}/{totalcount} writing node {node.Id} {node.Latitude} {node.Longitude}");
                }
            }
        }

        public void WriteJsons(string outFolder, string filenamePostfix = "")
        {
            Directory.CreateDirectory($"{outFolder}\\JSON");
            File.WriteAllText($"{outFolder}\\JSON\\Bounds_{filenamePostfix}.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(Bounds, Newtonsoft.Json.Formatting.Indented));
            File.WriteAllText($"{outFolder}\\JSON\\Nodes_{filenamePostfix}.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(RoadNodes, Newtonsoft.Json.Formatting.Indented));
            File.WriteAllText($"{outFolder}\\JSON\\Ways_{filenamePostfix}.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(RoadLinks, Newtonsoft.Json.Formatting.Indented));
        }

        public void WriteToFile(string outFolder = "ORSmartGrid", string extension = "json", string filenamePostfix = "", bool verbose = false, bool pauseWhenDone = false)
        {
            Directory.CreateDirectory(outFolder);

            switch (extension)
            {
                case "json":
                    WriteJsons(outFolder, filenamePostfix: filenamePostfix);
                    break;
                case "csv":
                    WriteCsvs(outFolder, verbose: verbose, filenamePostfix: filenamePostfix);
                    break;
                case "all":
                    WriteJsons(outFolder, filenamePostfix: filenamePostfix);
                    WriteCsvs(outFolder, verbose: verbose, filenamePostfix: filenamePostfix);
                    break;
            }

            if (pauseWhenDone)
            {
                Console.WriteLine($" done writing to {extension}");
                Console.ReadLine();
            }
        }
    }
}
