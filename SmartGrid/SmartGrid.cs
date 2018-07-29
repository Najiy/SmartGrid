using SmartGrid;
using System;
using System.Collections.Generic;
//using System.Device.Location;
using System.Linq;
using System.Reflection;

/*
 * This class will serve as the base map structure that other map structures can cast to
 */

namespace SmartGrid
{
    // using custom GeoCoordinate structure - keeping most relevant data for json parsing
    public struct GeoCoordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public struct RoadNode
    {
        public GeoCoordinate Coordinate { get; set; }
        public Dictionary<string, string> Descriptors { get; set; }
    }

    public struct RoadVector {
        public string NodeFrom { get; set; }
        public string NodeTo { get; set; }
        public Dictionary<string, string> Descriptors { get; set; }
    }

    public struct RoadLink
    {
        //public List<RoadVector> Vectors { get; set; }   // Use this list of vectors instead of list definition of nodes, this way we can add stuff like colours to road vectors - like google map traffic
        public List<string> Nodes { get; set; }         // <-- refactor the logic to this part in favour of the above ^^^
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
            if (KeepRelations)
            {
                foreach (var currentLink in self.Relations)
                {
                    RoadLink link = new RoadLink();

                    foreach (var member in currentLink.Members)
                    {
                        if (member.Type.ToLower() == "node")
                        {
                            link.Nodes.Add(member.Ref.ToString());
                        }
                    }
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
            }

            // @Shyam: Convert the OSMRoads reference as self to SmartGrid r

            return r;
        }

        public static SmartGrid FomOSOpenRoads(OSOpenRoads.OSOpenRoads self)
        {
            SmartGrid r = new SmartGrid();
            foreach (var currentNode in self.RoadNodes)
            {
                //need to debug
                RoadNode node = new RoadNode();

                GeoCoordinate coordinate = new GeoCoordinate()
                {
                    Latitude = currentNode.Coordinate.Latitude,
                    Longitude = currentNode.Coordinate.Longitude
                };
                node.Coordinate = coordinate;
                PropertyInfo[] attributes = currentNode.GetType().GetProperties();
                foreach (PropertyInfo i in attributes)
                {
                    if (i.Name.ToLower() != "longitude" && i.Name.ToLower() != "latitude" && i.Name.ToLower() != "id")
                    {
                        node.Descriptors.Add(i.Name, i.GetValue(currentNode, null).ToString());
                    }
                }
                var key = "OSOR:" + currentNode.Id;
                r.RoadNodes.Add(key, node);
            }
            foreach (var currentLink in self.RoadLinks)
            {
                RoadLink link = new RoadLink();

                //currently using GeoCoordinates as some coordinates do not have a corresponding ID in the same file.
                foreach (GeoCoordinate t in currentLink.CentrelineGeometry)
                {
                    var tolerance = Math.Abs(t.Longitude * .00001);
                    var a = self.RoadNodes.Where(x =>
                        x.Coordinate.Latitude.Equals(t.Latitude) && x.Coordinate.Longitude.Equals(t.Longitude));
                    link.Nodes.Add(a.FirstOrDefault().Id);
                }
                PropertyInfo[] attributes = currentLink.GetType().GetProperties();
                foreach (var i in attributes)
                {
                    if (i.Name.ToLower() != "id" && i.Name.ToLower() != "centerlinegeometry")
                    {
                        link.Descriptors.Add(i.Name, i.GetValue(currentLink, null).ToString());
                    }
                }
                var key = "OSOR:" + currentLink.Id;
                r.RoadLinks.Add(key, link);
            }
            // @Shyam: Convert the OSOpenRoads reference as self to SmartGrid r

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

            throw new NotImplementedException();
        }

        public double GetDistance(string roadNode1, string roadNode2)
        {

            // @Shyam compute the euclidean distance between two nodes

            throw new NotImplementedException();
        }

        public void GeneratePNG(double minlat, double minlon, double maxlat, double maxlon)
        {

            // @Shyam generate PNG using roadvectors defined by the bounds - do this then you're a Legend..
        }
    }
}