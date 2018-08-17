using System;
using System.Collections.Generic;
using System.Text;

namespace SmartGrid
{
    public class OsmTile
    {
        public int XCoord { get; set; }
        public int YCoord { get; set; }
    }
   static  class OsmTileConversion
    {
        public static OsmTile CoordToOsmTile(GeoCoordinate node, int zoom)
        {
            
            var n =(decimal) Math.Pow(2,zoom);
            var xTile =(int) (n * ((node.Longitude + 180) / 360));
            var lat_rad = DegToRad(node.Latitude);
            var tanLat = Math.Tan((double) lat_rad);
            var secLat = Secant((double) lat_rad);
            var yTile =(int) (n * (decimal) (1 - (Math.Log(tanLat + secLat)) /Math.PI) /2);
            var tileLocation = new OsmTile()
            {
                XCoord = xTile,
                YCoord = yTile
            };
            return tileLocation;
        }

        public static GeoCoordinate OsmTileToCoord(KeyValuePair<int, int> osmTile, int zoom)
        {
            var n = (decimal) Math.Pow(2, zoom);
            var longitude = osmTile.Key / n * 360 - 180;
            var latitudeRad = (decimal) Math.Atan(Math.Sinh(Math.PI * (1 - 2 * osmTile.Value / (double) n)));
            decimal latitude =  latitudeRad * (decimal) 180.0 /(decimal) Math.PI;
            return new GeoCoordinate()
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }

        private static double Secant(double x)
        {
            return (1 / Math.Cos(x));
        }
        public static decimal DegToRad(decimal degrees)
        {
            return degrees *(decimal) Math.PI / 180;
        }
    }
}
