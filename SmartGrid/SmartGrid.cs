using System;
using System.Collections.Generic;
using System.Text;

/*
 * This class will serve as the base map structure that other map structures can cast to
 */

namespace SmartGrid
{
    public struct RoadNode
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Dictionary<string, string> Descriptors { get; set; }
    }

    public struct RoadLink
    {
        public string FromNode { get; set; }
        public string ToNode { get; set; }
        public Dictionary<string, string> Descriptors { get; set; }
    }

    class SmartGrid
    {
        // the key will be ids and values the point themeselves, using dictionary so access to node is O(1)
        public Dictionary<string, RoadNode> Points { get; set; }
        public List<RoadLink> Ways { get; set; }

        public SmartGrid() {
            Points = new Dictionary<string, RoadNode>();
            Ways = new List<RoadLink>();
        }

        public static explicit operator SmartGrid(OSMRoads.OSMRoads self) {
            SmartGrid r = new SmartGrid();

            // @Shyam: Convert the OSMRoads reference as self to SmartGrid r

            return r;
        }

        public static explicit operator SmartGrid(OSOpenRoads.OSOpenRoads self)
        {
            SmartGrid r = new SmartGrid();

            // @Shyam: Convert the OSOpenRoads reference as self to SmartGrid r

            return r;
        }
    }
}
