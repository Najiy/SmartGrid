using SmartGrid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
//using System.Device.Location;
using System.Linq;
using System.Reflection;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using Newtonsoft.Json;

/*
 * This class will serve as the base map structure that other map structures can cast to
 */

namespace SmartGrid
{
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
        public Dictionary<string, string> Descriptors { get; set; }
    }

    public struct RoadLink
    {
        public List<RoadVector> Vectors { get; set; }   // Use this list of vectors instead of list definition of nodes
        //, this way we can add stuff like colours to road vectors - like google map traffic
        // public List<string> Nodes { get; set; }         // <-- refactor the logic to this part in favour of the above ^^^
        public Dictionary<string, string> Descriptors { get; set; }
    }

    internal class SmartGrid
    {
        // the key will be ids and values the point themeselves, using dictionary so access to node is O(1)
        public Dictionary<string, RoadNode> RoadNodes { get; set; }
        public Dictionary<string, RoadLink> RoadLinks { get; set; }

        public SmartGrid()
        {
            RoadNodes = new Dictionary<string, RoadNode>();
            RoadLinks = new Dictionary<string, RoadLink>();
        }

        public static SmartGrid FromOSM(OSMRoads.OSMRoads self, bool KeepRelations = true)
        {
            SmartGrid r = new SmartGrid();
            foreach (var currentNode in self.Nodes)
            {
                RoadNode node = new RoadNode();

                GeoCoordinate coordinate = new GeoCoordinate()
                {
                    Latitude = currentNode.Coordinate.Latitude,
                    Longitude = currentNode.Coordinate.Longitude
                };
                node.Coordinate = coordinate;
                PropertyInfo[] attributes = currentNode.GetType().GetProperties();
                foreach (var attribute in attributes)
                {
                    if (attribute.Name.ToLower() != "lat" && attribute.Name.ToLower() != "lon" &&
                        attribute.Name.ToLower() != "id")
                    {
                        node.Descriptors.Add(attribute.Name, attribute.GetValue(currentNode, null).ToString());
                    }
                }
                r.RoadNodes.Add(currentNode.ID.ToString(), node);
            }

            //no understanding of how relations are used in the OSM files. I think this is right but not sure.
            if (!KeepRelations) return r;

            foreach (var currentLink in self.Relations)
            {
                RoadLink link = new RoadLink();
                List<RoadVector> vectors = new List<RoadVector>();
                List<string> nodes = (from member in currentLink.Members
                                      where member.Type.ToLower() == "node"
                                      select member.Ref.ToString()).ToList();
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    var currNode = nodes[index];
                    var nextNode = nodes[index + 1];
                    vectors.Add(new RoadVector()
                    {
                        NodeFrom = currNode,
                        NodeTo = nextNode
                    });
                }
                link.Vectors = vectors;
                PropertyInfo[] attributes = currentLink.GetType().GetProperties();
                foreach (var attribute in attributes)
                {
                    if (attribute.Name.ToLower() != "members")
                    {
                        link.Descriptors.Add(attribute.Name, attribute.GetValue(currentLink, null).ToString());
                    }
                }
                var key = "OSM:" + currentLink.ID;
                r.RoadLinks.Add(key, link);
            }



            return r;
        }

        public static SmartGrid FomOSOpenRoads(OSOpenRoads.OSOpenRoads self)
        {
            SmartGrid r = new SmartGrid();
            foreach (var currentNode in self.RoadNodes)
            {
                //need to debug, @Najiy, when you implement these methods, please provide feedback
                RoadNode node = new RoadNode();

                GeoCoordinate coordinate = new GeoCoordinate()
                {
                    Latitude = currentNode.Value.Coordinate.Latitude,
                    Longitude = currentNode.Value.Coordinate.Longitude
                };
                node.Coordinate = coordinate;
                PropertyInfo[] attributes = currentNode.GetType().GetProperties();
                foreach (PropertyInfo i in attributes)
                {
                    node.Descriptors = new Dictionary<string, string>();
                    if (i.Name.ToLower() != "longitude" && i.Name.ToLower() != "latitude" && i.Name.ToLower() != "id")
                    {
                        node.Descriptors.Add(i.Name, i.GetValue(currentNode, null).ToString());
                    }
                }
                var key = "OSOR:" + currentNode.Key;
                try
                {
                    r.RoadNodes.Add(key, node);
                }
                catch
                {
                    var a = r.RoadNodes.Where(x => x.Key == key);

                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(a, Formatting.Indented));
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(node, Formatting.Indented));
                }
            }
            foreach (var currentLink in self.RoadLinks)
            {
                RoadLink link = new RoadLink();
                List<RoadVector> vectors = new List<RoadVector>();
                for (var index = 0; index < currentLink.CentrelineGeometry.Count - 1; index++)
                {
                    GeoCoordinate from = currentLink.CentrelineGeometry[index];
                    GeoCoordinate to = currentLink.CentrelineGeometry[index + 1];
                    var fromNode = self.RoadNodes.Where(x =>
                        x.Value.Coordinate.Latitude.Equals(from.Latitude) && x.Value.Coordinate.Longitude.Equals(from.Longitude));
                    var toNode = self.RoadNodes.Where(x =>
                        x.Value.Coordinate.Latitude.Equals(to.Latitude) && x.Value.Coordinate.Longitude.Equals(to.Longitude));
                    vectors.Add(new RoadVector()
                    {
                        NodeFrom = fromNode.FirstOrDefault().Key,
                        NodeTo = toNode.FirstOrDefault().Key

                    });
                }
                link.Vectors = vectors;
                PropertyInfo[] attributes = currentLink.GetType().GetProperties();
                foreach (var i in attributes)
                {
                    link.Descriptors = new Dictionary<string, string>();

                    if (i.Name.ToLower() != "id" && i.Name.ToLower() != "centerlinegeometry")
                    {
                        try
                        {
                            link.Descriptors.Add(i.Name, i.GetValue(currentLink, null).ToString());
                        }
                        catch
                        {
                            link.Descriptors.Add(i.Name,"null");
                        }
                    }
                }
                var key = "OSOR:" + currentLink.Id;
                r.RoadLinks.Add(key, link);
            }


            return r;
        }



        /*
         * this section will add functionalities to SmartGrid - all the cool maths and stuff
         * if can think of any cool stuff to make the SmartGrid smarter, add it here..
         * ask Alan if he needs any road geometry stuff for traffic analysis
         */

        public double GetAngle(string roadNode1, string roadNodeOrigin, string roadNode3)
        {

            // @Shyam compute the angle based on the given road nodes
            //Cosine law 

            var ab = GetDistance(roadNode1, roadNodeOrigin);
            var bc = GetDistance(roadNodeOrigin, roadNode3);
            var ac = GetDistance(roadNode1, roadNode3);
            var theta = Math.Acos((ab * ab + bc * bc - ac * ac) / (2 * ab * bc));
            return theta;

        }
        //Possible rounding errors
        public double GetDistance(string roadNode1, string roadNode2)
        {


            const int r = 6371000; //Earth's Radius in meters
            const decimal radiansOverDegrees =(decimal) (Math.PI / 180.0);
            var coordinate1 = RoadNodes[roadNode1];
            var coordinate2 = RoadNodes[roadNode2];
            var lat1 = coordinate1.Coordinate.Latitude * radiansOverDegrees;
            var latDifference = (coordinate1.Coordinate.Latitude - coordinate2.Coordinate.Latitude) * radiansOverDegrees;
            var lat2 = coordinate2.Coordinate.Latitude * radiansOverDegrees;
            var longDifference = (coordinate1.Coordinate.Longitude - coordinate2.Coordinate.Longitude) * radiansOverDegrees;

            //Haversine Formula
            var a = Math.Sin((double) latDifference / 2) * Math.Sin((double) latDifference / 2) +
                    Math.Cos((double) lat1) * Math.Cos((double) lat2) * Math.Sin((double) longDifference / 2) * Math.Sin((double) longDifference / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;



        }

        public void GeneratePNG(decimal minlat, decimal minlon, decimal maxlat, decimal maxlon)
        {
            // return roadlinks in range specified by parameters
            var linksInRange = RoadLinks.Where(x => x.Value.Vectors.TrueForAll(y => (minlat < RoadNodes[y.NodeFrom].Coordinate.Latitude
                                                 && maxlat > RoadNodes[y.NodeFrom].Coordinate.Latitude &&
                                                 minlat < RoadNodes[y.NodeFrom].Coordinate.Longitude &&
                                                 maxlat > RoadNodes[y.NodeFrom].Coordinate.Longitude) &&
                                                 (minlat < RoadNodes[y.NodeTo].Coordinate.Latitude
                                                    && maxlat > RoadNodes[y.NodeTo].Coordinate.Latitude &&
                                                minlat < RoadNodes[y.NodeTo].Coordinate.Longitude &&
                                                maxlat > RoadNodes[y.NodeTo].Coordinate.Longitude)));
            // @Shyam generate PNG using roadvectors defined by the bounds - do this then you're a Legend..
            Grid g = new Grid();

            Axis lat = new Axis
            {
                Minimum = (double)minlat,
                Maximum = (double)maxlat,


            };
            Axis lon = new Axis()
            {
                Minimum = (double) minlon,
                Maximum = (double) maxlon
            };
            lat.Interval = (double)(maxlat - minlat) / 100;
            lon.Interval = (double)(maxlon - minlon) / 100;
            ChartArea a = new ChartArea()
            {
                AxisX = lat,
                AxisY = lon
            };
            var ch = new Chart();
            ch.ChartAreas.Add(a);

            string python = @"D:\Program Files (x86)\python\python.exe";
            string pythonApp = @"C:\Users\shyam\PycharmProjects\codetest\plot_graph.py";
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);

            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcessStartInfo.Arguments = pythonApp;
            Process trustTheProcess = new Process();
            trustTheProcess.StartInfo = myProcessStartInfo;
            trustTheProcess.Start();

            //            {
            //               ChartAreas.Add(a)
            //                //  var s = new Series();
            //                //  foreach (var pnt in linksInRange) s.Points.Add(RoadNodes[pnt.Key].Coordinate.);
            //                //   ch.Series.Add(s);
            //
            ////                ch.SaveImage(@"C:\a", ChartImageFormat.Png);
            //
            //            };

        }

    }
}