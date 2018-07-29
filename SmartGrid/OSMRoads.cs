using SmartGrid;
using System;
using System.Collections.Generic;
//using System.Device.Location;
using System.IO;
using System.Xml;

namespace OSMRoads
{
    public struct Bounds
    {
        public double MinLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLat { get; set; }
        public double MaxLon { get; set; }
    }

    public struct Node
    {
        public UInt64 ID { get; set; }
        public bool Visible { get; set; }
        public int Version { get; set; }
        public UInt64 Changeset { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public UInt64 Uid { get; set; }
        public GeoCoordinate Coordinate { get; set; }

        public Dictionary<string, string> Tag { get; set; }
    }

    public struct Way
    {
        public UInt64 ID { get; set; }
        public bool Visible { get; set; }
        public int Version { get; set; }
        public UInt64 Changeset { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public UInt64 Uid { get; set; }
        public List<UInt64> NodeRefs { get; set; }
        public Dictionary<string, string> Tag { get; set; }
    }

    public struct RelationMember
    {
        public string Type { get; set; }
        public UInt64 Ref { get; set; }
        public string Role { get; set; }
    }

    public struct Relation
    {
        public UInt64 ID { get; set; }
        public bool Visible { get; set; }
        public int Version { get; set; }
        public UInt64 Changeset { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public UInt64 Uid { get; set; }
        public List<RelationMember> Members { get; set; }
        public Dictionary<string, string> Tag { get; set; }
    }

    public class OSMRoads : IMapParser
    {
        public Bounds Bounds = new Bounds();
        public List<Node> Nodes = new List<Node>();
        public List<Way> Ways = new List<Way>();
        public List<Relation> Relations = new List<Relation>();

        public void LoadFile(XmlDocument osm, bool verbose = false)
        {
            for (int i = 0; i < osm.FirstChild.NextSibling.ChildNodes.Count; i++)
            {
                var osmItem = osm.FirstChild.NextSibling.ChildNodes.Item(i);

                switch (osmItem.Name.ToLower())
                {
                    case "bounds":

                        Bounds.MaxLat = double.Parse(osmItem.Attributes.GetNamedItem("maxlat").Value);
                        Bounds.MaxLon = double.Parse(osmItem.Attributes.GetNamedItem("maxlon").Value);
                        Bounds.MinLat = double.Parse(osmItem.Attributes.GetNamedItem("minlat").Value);
                        Bounds.MinLon = double.Parse(osmItem.Attributes.GetNamedItem("minlon").Value);

                        if (verbose)
                            Console.WriteLine(
                                $"Bounds << {Bounds.MinLat} {Bounds.MinLon} {Bounds.MaxLat} {Bounds.MaxLon}");

                        break;

                    case "node":

                        var n = new Node()
                        {
                            ID = ulong.Parse(osmItem.Attributes.GetNamedItem("id").Value),
                            Visible = bool.Parse(osmItem.Attributes.GetNamedItem("visible").Value),
                            Version = int.Parse(osmItem.Attributes.GetNamedItem("version").Value),
                            Changeset = ulong.Parse(osmItem.Attributes.GetNamedItem("changeset").Value),
                            Timestamp = DateTime.Parse(osmItem.Attributes.GetNamedItem("timestamp").Value),
                            User = osmItem.Attributes.GetNamedItem("user").Value,
                            Uid = ulong.Parse(osmItem.Attributes.GetNamedItem("uid").Value),
                            Tag = new Dictionary<string, string>()
                        };
                        GeoCoordinate coordinate = new GeoCoordinate()
                        {
                            Latitude = double.Parse(osmItem.Attributes.GetNamedItem("lat").Value),
                            Longitude = double.Parse(osmItem.Attributes.GetNamedItem("lon").Value)
                        };
                        n.Coordinate = coordinate;
                        if (osmItem.HasChildNodes)
                            for (int j = 0; j < osmItem.ChildNodes.Count; j++)
                            {
                                var n_j = osmItem.ChildNodes[j];

                                switch (n_j.Name)
                                {
                                    case "tag":
                                        n.Tag.Add(n_j.Attributes.GetNamedItem("k").Value,
                                            n_j.Attributes.GetNamedItem("v").Value);
                                        break;

                                    default:
                                        if (verbose)
                                        {
                                            Console.WriteLine("undefined way child node");
                                            Console.ReadLine();
                                        }
                                        break;
                                }
                            }

                        if (verbose)
                            Console.WriteLine(
                                $"Nodes <<   {n.ID,-12} {n.Coordinate.Latitude,-12} {n.Coordinate.Longitude,-12} {n.Timestamp,-20}  {n.User}");

                        Nodes.Add(n);

                        break;

                    case "way":

                        var w = new Way()
                        {
                            ID = ulong.Parse(osmItem.Attributes.GetNamedItem("id").Value),
                            Visible = bool.Parse(osmItem.Attributes.GetNamedItem("visible").Value),
                            Version = int.Parse(osmItem.Attributes.GetNamedItem("version").Value),
                            Changeset = ulong.Parse(osmItem.Attributes.GetNamedItem("changeset").Value),
                            Timestamp = DateTime.Parse(osmItem.Attributes.GetNamedItem("timestamp").Value),
                            User = osmItem.Attributes.GetNamedItem("user").Value,
                            Uid = ulong.Parse(osmItem.Attributes.GetNamedItem("uid").Value),
                            NodeRefs = new List<ulong>(),
                            Tag = new Dictionary<string, string>()
                        };

                        if (osmItem.HasChildNodes)
                            for (int j = 0; j < osmItem.ChildNodes.Count; j++)
                            {
                                var w_j = osmItem.ChildNodes[j];

                                switch (w_j.Name)
                                {
                                    case "nd":
                                        w.NodeRefs.Add(ulong.Parse(w_j.Attributes.GetNamedItem("ref").Value));
                                        break;

                                    case "tag":
                                        w.Tag.Add(w_j.Attributes.GetNamedItem("k").Value,
                                            w_j.Attributes.GetNamedItem("v").Value);
                                        break;

                                    default:
                                        if (verbose)
                                        {
                                            Console.WriteLine("undefined way child node");
                                            Console.ReadLine();
                                        }
                                        break;
                                }
                            }

                        if (verbose)
                            Console.WriteLine(
                                $"Ways <<   {w.ID,-12} {w.NodeRefs.Count}-vertx  {w.Timestamp,-20}  {w.User}");

                        Ways.Add(w);

                        break;

                    case "relation":

                        var r = new Relation()
                        {
                            ID = ulong.Parse(osmItem.Attributes.GetNamedItem("id").Value),
                            Visible = bool.Parse(osmItem.Attributes.GetNamedItem("visible").Value),
                            Version = int.Parse(osmItem.Attributes.GetNamedItem("version").Value),
                            Changeset = ulong.Parse(osmItem.Attributes.GetNamedItem("changeset").Value),
                            Timestamp = DateTime.Parse(osmItem.Attributes.GetNamedItem("timestamp").Value),
                            User = osmItem.Attributes.GetNamedItem("user").Value,
                            Uid = ulong.Parse(osmItem.Attributes.GetNamedItem("uid").Value),
                            Members = new List<RelationMember>(),
                            Tag = new Dictionary<string, string>()
                        };

                        if (osmItem.HasChildNodes)
                            for (int j = 0; j < osmItem.ChildNodes.Count; j++)
                            {
                                var r_j = osmItem.ChildNodes[j];

                                switch (r_j.Name)
                                {
                                    case "member":
                                        r.Members.Add(new RelationMember()
                                        {
                                            Type = r_j.Attributes.GetNamedItem("type").Value,
                                            Ref = ulong.Parse(r_j.Attributes.GetNamedItem("ref").Value),
                                            Role = r_j.Attributes.GetNamedItem("role").Value,
                                        });
                                        break;

                                    case "tag":
                                        r.Tag.Add(r_j.Attributes.GetNamedItem("k").Value,
                                            r_j.Attributes.GetNamedItem("v").Value);
                                        break;

                                    default:
                                        if (verbose)
                                        {
                                            Console.WriteLine("undefined way child node");
                                            Console.ReadLine();
                                        }
                                        break;
                                }
                            }

                        if (verbose)
                            Console.WriteLine(
                                $"Relation <<   {r.ID,-12} {r.Members.Count}-members  {r.Timestamp,-20}  {r.User}");

                        Relations.Add(r);

                        break;

                    default:
                        Console.WriteLine(osmItem.Name.ToLower());
                        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(osmItem));
                        Console.ReadLine();
                        break;
                }
            }
        }

        public void LoadFile(string osmPath, bool verbose = false)
        {
            XmlDocument osm = new XmlDocument();
            osm.Load(osmPath);
            LoadFile(osm, verbose);
        }

        public void WriteJsons(string outFolder, string filenamePostfix = "")
        {
            Directory.CreateDirectory($"{outFolder}\\JSON");
            File.WriteAllText($"{outFolder}\\JSON\\Bounds_{filenamePostfix}.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(Bounds, Newtonsoft.Json.Formatting.Indented));
            File.WriteAllText($"{outFolder}\\JSON\\Nodes_{filenamePostfix}.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(Nodes, Newtonsoft.Json.Formatting.Indented));
            File.WriteAllText($"{outFolder}\\JSON\\Ways_{filenamePostfix}.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(Ways, Newtonsoft.Json.Formatting.Indented));
            File.WriteAllText($"{outFolder}\\JSON\\Relations_{filenamePostfix}.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(Relations, Newtonsoft.Json.Formatting.Indented));
        }

        public void WriteCsvs(string outFolder, bool verbose = false, string filenamePostfix = "")
        {
            var nodecount = Nodes.Count;
            var linkcount = Ways.Count;
            var relationscount = Relations.Count;
            var totalcount = nodecount + linkcount + relationscount;
            var i = 0;

            Directory.CreateDirectory($"{outFolder}\\CSV");
            using (var fileStream = File.CreateText($"{outFolder}\\CSV\\Bounds_{filenamePostfix}.csv"))
            {
                fileStream.WriteLine($"minlat,minlon,maxlat,maxlon");

                fileStream.WriteLine($"{Bounds.MinLat},{Bounds.MinLon},{Bounds.MaxLat},{Bounds.MaxLon}");
            }
            using (var fileStream = File.CreateText($"{outFolder}\\CSV\\Nodes_{filenamePostfix}.csv"))
            {
                fileStream.WriteLine($"id,lat,lon,visible,version,changeset,timestamp,user,uid,tag");
                foreach (var n in Nodes)
                {
                    fileStream.WriteLine(
                        $"{n.ID},{n.Coordinate.Latitude},{n.Coordinate.Longitude},{n.Visible},{n.Version},{n.Changeset},{n.Timestamp},{n.User},{n.Uid},{Newtonsoft.Json.JsonConvert.SerializeObject(n.Tag).Replace(",", "#comma#")}"
                        );

                    i++;
                    if (verbose)
                        Console.WriteLine($" {i}/{totalcount} writing node {n.ID} {n.Coordinate.Latitude} {n.Coordinate.Longitude} {n.Timestamp} {n.User}");
                }
            }
            using (var fileStream = File.CreateText($"{outFolder}\\CSV\\Relations_{filenamePostfix}.csv"))
            {
                fileStream.WriteLine($"id,visible,version,changeset,timestamp,user,uid,members,tag");
                foreach (var r in Relations)
                {
                    fileStream.WriteLine(
                        $"{r.ID},{r.Visible},{r.Version},{r.Changeset},{r.Timestamp},{r.User},{r.Uid},{Newtonsoft.Json.JsonConvert.SerializeObject(r.Members).Replace(",", "#comma#")},{Newtonsoft.Json.JsonConvert.SerializeObject(r.Tag).Replace(",", "#comma#")}");

                    i++;
                    if (verbose)
                        Console.WriteLine($" {i}/{totalcount} writing relation {r.ID} {r.Timestamp} {r.User}");
                }
            }
            using (var fileStream = File.CreateText($"{outFolder}\\CSV\\Ways_{filenamePostfix}.csv"))
            {
                fileStream.WriteLine($"id,visible,version,changeset,timestamp,user,uid,refs,tag");
                foreach (var w in Ways)
                {
                    fileStream.WriteLine(
                        $"{w.ID},{w.Visible},{w.Version},{w.Changeset},{w.Timestamp},{w.User},{w.Uid},{Newtonsoft.Json.JsonConvert.SerializeObject(w.NodeRefs).Replace(",", "#comma#")},{Newtonsoft.Json.JsonConvert.SerializeObject(w.Tag).Replace(",", "#comma#")}");

                    i++;
                    if (verbose)
                        Console.WriteLine($" {i}/{totalcount} writing way {w.ID} {w.Timestamp} {w.User}");
                }
            }
        }

        public void WriteToFile(string outFolder = "OSMSmartGrid", string extension = "json", string filenamePostfix = "", bool verbose = false, bool pauseWhenDone = false)
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

                default:
                    Console.WriteLine("no output extensions specified");
                    Console.ReadLine();
                    break;
            }

            if (!pauseWhenDone) return;
            Console.WriteLine($" done writing to {extension}");
            Console.ReadLine();
        }
    }
}