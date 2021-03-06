﻿using SmartGrid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using System.Xml;
using getAddress;

/*
 * This class will serve as the base map structure that other map structures can cast to
 */

namespace SmartGrid
{
    
    public struct Bounds
    {
        public decimal MaxLon { get; set; }
        public decimal MinLon { get; set; }
        public decimal MaxLat { get; set; }
        public decimal MinLat { get; set; }
    }
    // using custom GeoCoordinate structure - keeping most relevant data for json parsing
    public struct GeoCoordinate
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public struct RoadNode
    {
        public GeoCoordinate Coordinate { get; set; }
        public Dictionary<string, string> Descriptors { get; set; }
    }

    public struct RoadVector
    {
        public string NodeFrom { get; set; }
        public string NodeTo { get; set; }
        
    }

    public struct RoadLink
    {
        public List<RoadVector> Vectors { get; set; }   // Use this list of vectors instead of list definition of nodes

        //, this way we can add stuff like colours to road vectors - like google map traffic
        
        public Dictionary<string, string> Descriptors { get; set; }
    }

    public class SmartGrid
    {
        // the key will be ids and values the point themeselves, using dictionary so access to node is O(1)
        public Dictionary<string, RoadNode> RoadNodes { get; set; }

        public Dictionary<string, RoadLink> RoadLinks { get; set; }

        public SmartGrid()
        {
            RoadNodes = new Dictionary<string, RoadNode>();
            RoadLinks = new Dictionary<string, RoadLink>();
        }
        //Parse OSM data from XML structure 
        public static SmartGrid FromOSM(XmlDocument osm, GeoCoordinate maxCoordinate,GeoCoordinate minCoordinate, SmartGrid r)
        {
            
            Bounds bounds = new Bounds()
            {
                MaxLat = maxCoordinate.Latitude,
                MaxLon = maxCoordinate.Longitude,
                MinLat = minCoordinate.Latitude,
                MinLon = minCoordinate.Longitude
            };
            //navigate XML structure for relevant information
            for (int i = 0; i < osm.FirstChild.NextSibling.ChildNodes.Count; i++)
            {
                var osmItem = osm.FirstChild.NextSibling.ChildNodes.Item(i);

                switch (osmItem.Name.ToLower())
                {
                    case "node":
                        GeoCoordinate coordinate = new GeoCoordinate()
                        {
                            Latitude = decimal.Parse(osmItem.Attributes.GetNamedItem("lat").Value),
                            Longitude = decimal.Parse(osmItem.Attributes.GetNamedItem("lon").Value)
                        };
                        if (IsWithin(coordinate, bounds))
                        {
                            var n = new RoadNode()
                            {
                                Descriptors = new Dictionary<string, string>()
                            };
                            try
                            {
                                n.Descriptors.Add("Visible", osmItem.Attributes.GetNamedItem("visible").Value);
                            }
                            catch { }
                            n.Descriptors.Add("Version", osmItem.Attributes.GetNamedItem("version").Value);
                            n.Descriptors.Add("Changeset", osmItem.Attributes.GetNamedItem("changeset").Value);
                            n.Descriptors.Add("Timestamp", osmItem.Attributes.GetNamedItem("timestamp").Value);
                            n.Descriptors.Add("User", osmItem.Attributes.GetNamedItem("user").Value);
                            n.Descriptors.Add("Uid", osmItem.Attributes.GetNamedItem("uid").Value);

                            var ID = "OSM:" + osmItem.Attributes.GetNamedItem("id").Value;

                            n.Coordinate = coordinate;

                            if (osmItem.HasChildNodes)
                            {
                                for (int j = 0; j < osmItem.ChildNodes.Count; j++)
                                {
                                    var n_j = osmItem.ChildNodes[j];

                                    if (n_j.Name == "tag")
                                        n.Descriptors.Add(n_j.Attributes.GetNamedItem("k").Value,
                                            n_j.Attributes.GetNamedItem("v").Value);
                                }
                            }
                            r.RoadNodes.Add(ID, n);
                        }
                        break;

                    case "way":
                        var roadlink = new RoadLink()
                        {
                            Descriptors = new Dictionary<string, string>(),
                        };

                        var Id = "OSM:" + osmItem.Attributes.GetNamedItem("id").Value;
//                        roadlink.Descriptors.Add("Visible", osmItem.Attributes.GetNamedItem("visible").Value);
                        roadlink.Descriptors.Add("Version", osmItem.Attributes.GetNamedItem("version").Value);
                        roadlink.Descriptors.Add("Changeset", osmItem.Attributes.GetNamedItem("changeset").Value);
                        roadlink.Descriptors.Add("Timestamp", osmItem.Attributes.GetNamedItem("timestamp").Value);
                        roadlink.Descriptors.Add("User", osmItem.Attributes.GetNamedItem("user").Value);
                        roadlink.Descriptors.Add("Uid", osmItem.Attributes.GetNamedItem("uid").Value);

                        List<string> NodeRefs = new List<string>();
                        if (osmItem.HasChildNodes)
                        {
                            for (int j = 0; j < osmItem.ChildNodes.Count; j++)
                            {
                                var w_j = osmItem.ChildNodes[j];

                                switch (w_j.Name)
                                {
                                    case "nd":
                                        NodeRefs.Add("OSM:" + w_j.Attributes.GetNamedItem("ref").Value);
                                        break;

                                    case "tag":
                                        roadlink.Descriptors.Add(w_j.Attributes.GetNamedItem("k").Value,
                                            w_j.Attributes.GetNamedItem("v").Value);
                                        break;
                                }
                            }
                        }

                        var vectors = new List<RoadVector>();
                        for (int q = 0; q < NodeRefs.Count - 1; q++)
                        {
                            var vector = new RoadVector()
                            {
                                NodeFrom = NodeRefs[q],
                                NodeTo = NodeRefs[q + 1]
                            };
                            vectors.Add(vector);
                        }
                        var link = new RoadLink() { Vectors = vectors };
                        r.RoadLinks.Add(Id, link);
                        break;
                }
               
            }
         
            return r;
        }
        //Parse OpenRoad data from XML structure
        public static SmartGrid FromOSOpenRoads(XmlDocument or, GeoCoordinate maxCoordinate, GeoCoordinate minCoordinate, SmartGrid r)
        {
            

            XmlNode node = or.DocumentElement.FirstChild;
            Bounds bounds = new Bounds()
            {
                MaxLat = maxCoordinate.Latitude,
                MaxLon = maxCoordinate.Longitude,
                MinLat = minCoordinate.Latitude,
                MinLon = minCoordinate.Longitude
            };
            //navigate XML structure for relevant information

            for (var a = 0; a < or.DocumentElement.ChildNodes.Count; a++)
            {
                for (var i = 0; i < node.ChildNodes.Count; i++)
                {
                    var currentNode = node.ChildNodes.Item(i);
                    string Id;
                    switch (currentNode.Name)
                    {
                        case "road:RoadNode":
                            var roadNode = new RoadNode() { Descriptors = new Dictionary<string, string>() };
                            Id = "OSOR:" + currentNode.Attributes.GetNamedItem("gml:id").Value;
                            var coord = currentNode.ChildNodes.Where(x => x.Name == "net:geometry");
                            var location = coord.FirstOrDefault().InnerText.Split(" ");
                            var CCoordinate = LatLonConversions.ConvertOSToLatLon(Convert.ToDouble(location[0]),
                                                                                Convert.ToDouble(location[1]));
                            GeoCoordinate Ncoordinate = new GeoCoordinate()
                            {
                                Latitude = (decimal)CCoordinate.Latitude,
                                Longitude = (decimal)CCoordinate.Longitude
                            };
                            if (IsWithin(Ncoordinate, bounds))
                            {
                                roadNode.Coordinate = Ncoordinate;
                                var blsv = currentNode.ChildNodes.Where(x => x.Name == "net:beginLifespanVersion");
                                roadNode.Descriptors.Add("BeginLifeSpanVersion",
                                    blsv.FirstOrDefault()?.Attributes.GetNamedItem("xsi:nil").Value);
                                var inNetwork = currentNode.ChildNodes.Where(x => x.Name == "net:inNetwork");
                                roadNode.Descriptors.Add("inNetwork", inNetwork.FirstOrDefault()?.Attributes.GetNamedItem("xsi:nil").Value);
                                var forn = currentNode.ChildNodes.Where(x => x.Name == "tn-ro:formOfRoadNode");
                                roadNode.Descriptors.Add("FormOfRoadNode",
                                    forn.FirstOrDefault()?.Attributes.GetNamedItem("codeSpace").Value);
                                roadNode.Descriptors.Add("RoadType", forn.FirstOrDefault()?.InnerText);
                                r.RoadNodes.Add(Id, roadNode);
                            }

                            break;

                        case "road:RoadLink":
                            {
                                var roadLink = new RoadLink { Descriptors = new Dictionary<string, string>() };
                                Id = "OSOR:" + currentNode.Attributes.GetNamedItem("gml:id").Value;
                                var clg = currentNode.ChildNodes.Where(x => x.Name == "net:centrelineGeometry");
                                var vals = clg.FirstOrDefault()?.InnerText.Split(" ");
                                var CentrelineGeometry = new List<GeoCoordinate>();

                                for (int q = 0; q < vals.Length; q += 2)
                                {
                                    var convertedCoordinate = LatLonConversions.ConvertOSToLatLon(Double.Parse(vals[q]), double.Parse(vals[q + 1]));
                                    GeoCoordinate coordinate = new GeoCoordinate()
                                    {
                                        Latitude = (decimal)convertedCoordinate.Latitude,
                                        Longitude = (decimal)convertedCoordinate.Longitude
                                    };
                                    if (IsWithin(coordinate, bounds))
                                    {
                                        CentrelineGeometry.Add(coordinate);
                                    }
                                }

                                CreateNodes(CentrelineGeometry, r);

                                roadLink.Vectors = AddLinks(CentrelineGeometry, r);

                                var blv = currentNode.ChildNodes.Where(x => x.Name == "net:beginLifespanVersion");

                                roadLink.Descriptors.Add("BeginLifeSpanVersion", blv.FirstOrDefault()?.Attributes.GetNamedItem("xsi:nil").Value);

                                var inn = currentNode.ChildNodes.Where(x => x.Name == "net:inNetwork");

                                roadLink.Descriptors.Add("InNetwork", inn.FirstOrDefault()?.Attributes.GetNamedItem("xsi:nil").Value);
                                var fic = currentNode.ChildNodes.Where(x => x.Name == "net:fictitious");
                                roadLink.Descriptors.Add("Fictitious", fic.FirstOrDefault()?.InnerText);

                                var sn = currentNode.ChildNodes.Where(x => x.Name == "net:startNode");
                                roadLink.Descriptors.Add("StartNode", sn.FirstOrDefault()?.Attributes.GetNamedItem("xlink:href").Value);
                                var en = currentNode.ChildNodes.Where(x => x.Name == "net:endNode");
                                roadLink.Descriptors.Add("EndNode", en.FirstOrDefault()?.Attributes.GetNamedItem("xlink:href").Value);
                                var rc = currentNode.ChildNodes.Where(x => x.Name == "road:roadClassification");
                                roadLink.Descriptors.Add("RoadClassification", rc.FirstOrDefault()?.InnerText);
                                var rf = currentNode.ChildNodes.Where(x => x.Name == "road:roadFunction");
                                roadLink.Descriptors.Add("RoadFunction", rf.FirstOrDefault()?.InnerText);
                                var fow = currentNode.ChildNodes.Where(x => x.Name == "road:formOfWay");
                                roadLink.Descriptors.Add("FormOfWay", fow.FirstOrDefault()?.InnerText);
                                var len = currentNode.ChildNodes.Where(x => x.Name == "road:length");
                                roadLink.Descriptors.Add("Length", len.FirstOrDefault()?.InnerText);
                                roadLink.Descriptors.Add("UnitsOfMeasure", len.FirstOrDefault()?.Attributes.GetNamedItem("uom").Value);
                                var loop = currentNode.ChildNodes.Where(x => x.Name == "road:loop");
                                roadLink.Descriptors.Add("Loop", loop.FirstOrDefault()?.InnerText);
                                var pr = currentNode.ChildNodes.Where(x => x.Name == "road:primaryRoute");
                                roadLink.Descriptors.Add("PrimaryRoute", pr.FirstOrDefault()?.InnerText);
                                var tr = currentNode.ChildNodes.Where(x => x.Name == "road:trunkRoad");
                                roadLink.Descriptors.Add("TrunkRoad", tr.FirstOrDefault()?.InnerText);
                                var rn = currentNode.ChildNodes.Where(x => x.Name.Contains("road:name"));
                                roadLink.Descriptors.Add("RoadName", rn.FirstOrDefault()?.InnerText);
                                var nametoid = currentNode.ChildNodes.Where(x => x.Name == "road:roadNameTOID");
                                roadLink.Descriptors.Add("RoadNameTOID", nametoid.FirstOrDefault()?.Attributes.GetNamedItem("xlink:href").Value);
                                var numbertoid = currentNode.ChildNodes.Where(x => x.Name == "road:roadNumberTOID");
                                roadLink.Descriptors.Add("RoadNumberTOID",
                                    numbertoid.FirstOrDefault()?.Attributes.GetNamedItem("xlink:href").Value);

                                
                                try
                                {
                                    if (roadLink.Vectors.Count > 0)
                                        r.RoadLinks.Add(Id, roadLink);
                                }
                                catch { }
                                break;
                            }
                    }
                }
                node = node.NextSibling;
            }

            return r;
        }
        //check coordinate is within bounds
        public static bool IsWithin(GeoCoordinate coord, Bounds bound)
        {
            return (coord.Latitude < bound.MaxLat && coord.Latitude > bound.MinLat && coord.Longitude < bound.MaxLon &&
                   coord.Longitude > bound.MinLon);
        }

      
        private static List<RoadVector> AddLinks(List<GeoCoordinate> centrelineGeometry, SmartGrid r)
        {
           
            List<RoadVector> vectors = new List<RoadVector>();
            for (var index = 1; index < centrelineGeometry.Count - 2; index++)
            {
                GeoCoordinate from = centrelineGeometry[index];
                GeoCoordinate to = centrelineGeometry[index + 1];
                var fromNode = r.RoadNodes.Where(x =>
                    x.Value.Coordinate.Latitude.Equals(from.Latitude) && x.Value.Coordinate.Longitude.Equals(from.Longitude));
                var toNode = r.RoadNodes.Where(x =>
                    x.Value.Coordinate.Latitude.Equals(to.Latitude) && x.Value.Coordinate.Longitude.Equals(to.Longitude));
                var roadVector = new RoadVector()
                {
                    NodeFrom = fromNode.FirstOrDefault().Key,
                    NodeTo = toNode.FirstOrDefault().Key
                };
                vectors.Add(roadVector);
            }
         

           
            return vectors;
        }

        //used for intermediate nodes that don't appear in .gml files
        public static void CreateNodes(List<GeoCoordinate> coordinates, SmartGrid r)
        {
            for (int i = 1; i < coordinates.Count - 1; i++)
            {
                var coordinate = coordinates[i];
             
                var id = "OSOR:" + coordinate.Latitude + coordinate.Longitude;
                var node = new RoadNode()
                {
                    Coordinate = coordinate,

                    //Default settings
                };
                try
                {
                    r.RoadNodes.Add(id, node);
                }
                catch { }
            }
        }

        /*
         * this section will add functionalities to SmartGrid - all the cool maths and stuff
         * if can think of any cool stuff to make the SmartGrid smarter, add it here..
         * ask Alan if he needs any road geometry stuff for traffic analysis
         */

        public double GetAngle(string roadNode1, string roadNodeOrigin, string roadNode3)
        {
           
            //Cosine law

            var ab = GetDistance(roadNode1, roadNodeOrigin);
            var bc = GetDistance(roadNodeOrigin, roadNode3);
            var ac = GetDistance(roadNode1, roadNode3);
            var theta = Math.Acos((ab * ab + bc * bc - ac * ac) / (2 * ab * bc));
            return theta;
        }

       
        public double GetDistance(string roadNode1, string roadNode2)
        {
            const int r = 6371000; //Earth's Radius in meters
            const decimal radiansOverDegrees = (decimal)(Math.PI / 180.0);
            var coordinate1 = RoadNodes[roadNode1];
            var coordinate2 = RoadNodes[roadNode2];
            var lat1 = coordinate1.Coordinate.Latitude * radiansOverDegrees;
            var latDifference = (coordinate1.Coordinate.Latitude - coordinate2.Coordinate.Latitude) * radiansOverDegrees;
            var lat2 = coordinate2.Coordinate.Latitude * radiansOverDegrees;
            var longDifference = (coordinate1.Coordinate.Longitude - coordinate2.Coordinate.Longitude) * radiansOverDegrees;

            //Haversine Formula
            var a = Math.Sin((double)latDifference / 2) * Math.Sin((double)latDifference / 2) +
                    Math.Cos((double)lat1) * Math.Cos((double)lat2) * Math.Sin((double)longDifference / 2) 
                    * Math.Sin((double)longDifference / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }

        public static void GeneratePNG(OsmTile tileBounds, string filePath)
        {
            
            //change where necessary    
            string python = @"D:\Program Files (x86)\python\python.exe";
            string pythonApp = @"D:\BiRT\SmartGrid\SmartGrid\plot_graph.py";
            var originalCode = File.ReadAllLines(pythonApp);
            var code = File.ReadAllLines(pythonApp).ToList();
            Process p = new Process
            {
                StartInfo =
                {
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    FileName = "python"
                }
            };


            //feed latlon bounds and json filepath to python script
            var max = OsmTileConversion.OsmTileToCoord(new KeyValuePair<int, int>(tileBounds.XCoord, tileBounds.YCoord - 1), 16);
            var min = OsmTileConversion.OsmTileToCoord(new KeyValuePair<int, int>(tileBounds.XCoord - 1, tileBounds.YCoord), 16);
            code.Insert(5, "max_x = " + max.Latitude);
            code.Insert(5, "max_y = " + max.Longitude);
            code.Insert(5, "min_x = " + min.Latitude);
            code.Insert(5, "min_y = " + min.Longitude);
            p.StartInfo.Arguments = pythonApp + " " + filePath;
            File.WriteAllLines(pythonApp,code);

            p.Start();
            using (var reader = p.StandardOutput)
            {
                string result = reader.ReadToEnd();
                Console.Write(result);
            }
            File.WriteAllLines(pythonApp, originalCode);
        }

        public static RootObject reverseGeoCode(GeoCoordinate coordinate)
        {
            return global::getAddress.getAddress.findAddress((double)coordinate.Latitude,
                (double)coordinate.Longitude);
        }
    }
}
