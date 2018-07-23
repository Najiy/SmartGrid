using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace OsmSmartGrid
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
        public double Lat { get; set; }
        public double Lon { get; set; }
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

    public class OsmSmartGrid
    {
        public Bounds Bounds = new Bounds();
        public List<Node> Nodes = new List<Node>();
        public List<Way> Ways = new List<Way>();
        public List<Relation> Relations = new List<Relation>();

        public void LoadOSM(XmlDocument osm, bool verbose = false, bool validate = true)
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
                            Lat = double.Parse(osmItem.Attributes.GetNamedItem("lat").Value),
                            Lon = double.Parse(osmItem.Attributes.GetNamedItem("lon").Value),
                            Tag = new Dictionary<string, string>()
                        };

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
                                        if (validate)
                                        {
                                            Console.WriteLine("undefined way child node");
                                            Console.ReadLine();
                                        }
                                        break;
                                }
                            }

                        if (verbose)
                            Console.WriteLine(
                                $"Nodes <<   {n.ID,-12} {n.Lat,-12} {n.Lon,-12} {n.Timestamp,-20}  {n.User}");

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
                                        if (validate)
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
                                        if (validate)
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

        public void LoadOSM(string osmPath)
        {
            XmlDocument osm = new XmlDocument();
            osm.Load(osmPath);
            LoadOSM(osm);
        }

        public void WriteJsons(string outFolder)
        {
            Directory.CreateDirectory($"{outFolder}\\JSON");
            File.WriteAllText($"{outFolder}\\JSON\\Bounds.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(Bounds, Newtonsoft.Json.Formatting.Indented));
            File.WriteAllText($"{outFolder}\\JSON\\Nodes.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(Nodes, Newtonsoft.Json.Formatting.Indented));
            File.WriteAllText($"{outFolder}\\JSON\\Ways.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(Ways, Newtonsoft.Json.Formatting.Indented));
            File.WriteAllText($"{outFolder}\\JSON\\Relations.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(Relations, Newtonsoft.Json.Formatting.Indented));
        }

        public void WriteCsvs(string outFolder)
        {
            Directory.CreateDirectory($"{outFolder}\\CSV");
            using (var fileStream = File.CreateText($"{outFolder}\\CSV\\Bounds.csv"))
            {
                fileStream.WriteLine($"minlat,minlon,maxlat,maxlon");

                fileStream.WriteLine($"{Bounds.MinLat},{Bounds.MinLon},{Bounds.MaxLat},{Bounds.MaxLon}");
            }
            using (var fileStream = File.CreateText($"{outFolder}\\CSV\\Nodes.csv"))
            {
                fileStream.WriteLine($"id,visible,version,changeset,timestamp,user,uid,lat,lon,tag");
                foreach (var n in Nodes)
                {
                    fileStream.WriteLine(
                        $"{n.ID},{n.Visible},{n.Version},{n.Changeset},{n.Timestamp},{n.User},{n.Uid},{n.Lat},{n.Lon},{Newtonsoft.Json.JsonConvert.SerializeObject(n.Tag).Replace(",", "#comma#")}");
                }
            }
            using (var fileStream = File.CreateText($"{outFolder}\\CSV\\Relations.csv"))
            {
                fileStream.WriteLine($"id,visible,version,changeset,timestamp,user,uid,members,tag");
                foreach (var r in Relations)
                {
                    fileStream.WriteLine(
                        $"{r.ID},{r.Visible},{r.Version},{r.Changeset},{r.Timestamp},{r.User},{r.Uid},{Newtonsoft.Json.JsonConvert.SerializeObject(r.Members).Replace(",", "#comma#")},{Newtonsoft.Json.JsonConvert.SerializeObject(r.Tag).Replace(",", "#comma#")}");
                }
            }
            using (var fileStream = File.CreateText($"{outFolder}\\CSV\\Ways.csv"))
            {
                fileStream.WriteLine($"id,visible,version,changeset,timestamp,user,uid,refs,tag");
                foreach (var w in Ways)
                {
                    fileStream.WriteLine(
                        $"{w.ID},{w.Visible},{w.Version},{w.Changeset},{w.Timestamp},{w.User},{w.Uid},{Newtonsoft.Json.JsonConvert.SerializeObject(w.NodeRefs).Replace(",", "#comma#")},{Newtonsoft.Json.JsonConvert.SerializeObject(w.Tag).Replace(",", "#comma#")}");
                }
            }
        }

        public void WriteToFile(string outFolder = "SmartGrid", string extension = "json")
        {
            Directory.CreateDirectory(outFolder);

            switch (extension)
            {
                case "json":
                    WriteJsons(outFolder);
                    break;
                case "csv":
                    WriteCsvs(outFolder);
                    break;
                case "all":
                    WriteJsons(outFolder);
                    WriteCsvs(outFolder);
                    break;
                default:
                    Console.WriteLine("no output extensions specified");
                    Console.ReadLine();
                    break;
            }
        }
    }
}