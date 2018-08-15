using System;
using System.Collections.Generic;
using System.Text;

namespace SmartGrid
{
    class OsmTileConversion
    {
        public KeyValuePair<int, int> CoordToOsmTile(GeoCoordinate node, int zoom)
        {
            
            var n =(decimal) Math.Pow(2,zoom);
            var xTile =(int) Math.Round(n * ((node.Longitude + 180) / 360));
            var lat_rad = DegToRad(node.Latitude);
            var tanLat = Math.Tan((double) lat_rad);
            var secLat = Secant((double) lat_rad);
            var yTile =(int) Math.Round(n * (decimal) (1 - (Math.Log(tanLat + secLat)) /Math.PI) /2);
            var tileLocation = new KeyValuePair<int, int>(xTile, yTile);
            return tileLocation;
        }

        public GeoCoordinate OsmTileToCoord(KeyValuePair<int, int> osmTile, int zoom)
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

        private double Secant(double x)
        {
            return (1 / Math.Cos(x));
        }
        public decimal DegToRad(decimal degrees)
        {
            return degrees *(decimal) Math.PI / 180;
        }
    }
}
