# SmartGrid
Parsing map sources into one structure for python/java/C# visualisation
SmartGrid is a Project for parsing road data into a generic structure which can be used for comparison between
datasets. Currently handling OpenStreetMap and Ordnance Survey Open Road data. This will need to be expanded upon
to parse other data structures.

For example ...
Generate a SmartGrid object given a OSM tile:
	
	var maxCoord = OsmTileConversion.OsmTileToCoord(new KeyValuePair<int, int>(OsmTile.XCoord, OsmTile.YCoord - 1), 16);
    var minCoord =
               OsmTileConversion.OsmTileToCoord(new KeyValuePair<int, int>(OsmTile.XCoord - 1, OsmTile.YCoord), 16);
    XmlDocument or = new XmlDocument();
    or.Load(OpenRoad File path);
    XmlDocument osm = new XmlDocument();
    osm.Load(Osm file path);
    SmartGrid SmartGrid = SmartGrid.FromOSOpenRoads(or, maxCoord, minCoord, new SmartGrid());
    SmartGrid = SmartGrid.FromOSM(osm, maxCoord, minCoord, BSmartGrid);

Generate a SmartGrid object given Coordinate bounds:
	
	var maxCoord = new GeoCoordinate()
    {
        Latitude = xMax
       ,
        Longitude = yMax
    };
    var minCoord = new GeoCoordinate()
    {
        Latitude = xMin,
        Longitude = yMin
    };
    XmlDocument or = new XmlDocument();
    or.Load(OpenRoad file path);
    XmlDocument osm = new XmlDocument();
    osm.Load(OSM file path);
    //smartgrid will only take values within the bounds.
    SmartGrid BSmartGrid = SmartGrid.FromOSOpenRoads(or, maxCoord, minCoord, new SmartGrid());
    BSmartGrid = SmartGrid.FromOSM(osm, maxCoord, minCoord, BSmartGrid);
	
Create JSON files for all OSM tiles covered by SmartGrid object:
	
	WriteJson.CreateJsons(SmartGrid);
	 
Generate PNG for SmartGrid object:
	
	SmartGrid.GeneratePNG(OsmTile, JSON file path);
	
