using System;
using System.Collections.Generic;
using System.IO;

namespace SmartGrid
{
    class JsonObject
    {
        public SmartGrid SmartGrid { get; set; }
        public OsmTile OsmTile { get; set; }
    }
    public static class WriteJson
    {

        public static void CreateJsons(SmartGrid sg)
        {
            var path = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            path = path.Substring(6);
            OsmTileConversion conversion = new OsmTileConversion();
            foreach (var node in sg.RoadNodes.Values)
            {
                var osmTile = conversion.CoordToOsmTile(node.Coordinate, 16);
                var jsonFilePath = (path + $"\\SmartGrid\\JSON\\{osmTile.XCoord}\\{osmTile.YCoord}.json");
                //TODO: ISSUE HERE tile values correct but bounds arent sensical
                var maxCoo =
                    conversion.OsmTileToCoord(new KeyValuePair<int, int>(osmTile.XCoord, osmTile.YCoord), 16);
                var minCoo =
                    conversion.OsmTileToCoord(new KeyValuePair<int, int>(osmTile.XCoord + 1, osmTile.YCoord + 1), 16);
                 var maxCoord = new GeoCoordinate()
                 {
                     Latitude = Math.Max(maxCoo.Latitude,minCoo.Latitude),
                     Longitude = Math.Max(maxCoo.Longitude,minCoo.Longitude)
                 };
                var minCoord = new GeoCoordinate()
                {
                    Latitude = Math.Min(maxCoo.Latitude, minCoo.Latitude),
                    Longitude = Math.Min(maxCoo.Longitude, minCoo.Longitude)
                };
                if (File.Exists(jsonFilePath)) continue;
                var queriedSmartGrid = new SmartGrid()
                {
                    RoadNodes = new Dictionary<string, RoadNode>(),
                    RoadLinks = new Dictionary<string, RoadLink>()

                };
                foreach (var nd in sg.RoadNodes)
                {
                    if (IsWithin(nd.Value.Coordinate, maxCoord, minCoord))
                    {
                        queriedSmartGrid.RoadNodes.Add(nd.Key, nd.Value);

                    }
                }
                var linksInTile = new Dictionary<string, RoadLink>();
                foreach (var links in sg.RoadLinks)
                {

                    var linkInTile = new RoadLink()
                    {
                        Vectors = new List<RoadVector>(),
                        Descriptors = new Dictionary<string, string>()
                    };
                    foreach (var link in links.Value.Vectors)
                    {
                        try
                        {
                            if (!IsWithin(sg.RoadNodes[link.NodeFrom].Coordinate, maxCoord, minCoord) ||
                                !IsWithin(sg.RoadNodes[link.NodeTo].Coordinate, maxCoord, minCoord)) continue;
                            linkInTile.Vectors.Add(link);
                            linkInTile.Descriptors = link.Descriptors;
                        }
                        catch { }
                    }
                    if (linkInTile.Vectors.Count > 0)
                        linksInTile.Add(links.Key, linkInTile);
                }
                queriedSmartGrid.RoadLinks = linksInTile;

                var jsonObject = new JsonObject()
                {
                    OsmTile = osmTile,
                    SmartGrid = queriedSmartGrid
                };
                if (queriedSmartGrid.RoadNodes.Count <= 0) continue;
                System.IO.FileInfo file = new System.IO.FileInfo(jsonFilePath);
                file.Directory.Create();
                File.WriteAllText(jsonFilePath,
                    Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented));
            }
        }

        public static bool IsWithin(GeoCoordinate coord, GeoCoordinate max, GeoCoordinate min)
        {

            return (coord.Latitude >= min.Latitude && coord.Latitude <= max.Latitude &&
                    coord.Longitude >= min.Longitude &&
                    coord.Longitude <= max.Longitude);
        }

    }
}