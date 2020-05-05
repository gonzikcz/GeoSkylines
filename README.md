# GeoSkylines general info
Cities: Skylines game mod for import/export of geodata. 

Install game mod (if not on Steam workshop):
- use the provided GeoSkylines.dll or compile the code using e.g. Visual Studio
- go to c:\Users\<username>\AppData\Local\Colossal Order\Cities_Skylines\Addons\Mods\
- create a folder GeoSkylines
- copy the GeoSkylines.dll file to newly created folder GeoSkylines
- in game, go to Content Manager > Mods > turn on GeoSkylines

GeoSkylines methods are called using the threading hooks via hotkeys, combination of Ctrl + R, L, W, Q, T, V, S, Z, P, G, H, J, K. To avoid conflicts with hotkeys of other mods, it is recommended to turn off other mods before using GeoSkylines. After using import or export methods of GeoSkylines then you can turn the mod off. 

Three stages of creating playable model in Cities: Skylines based on geodata:
1. Prepare geodata for import
2. Create base model using GeoSkylines methods
3. Create playable model (manual post-processing)

# Prepare vector geodata for import
I chose to use a simple CSV format with geometry data recorded as WKT. Thus, any geo-dataset can be used. For testing I used OSM predominantly. For the CSV file preparation I used OSMSharp library (other programs such as QGIS or FME would suffice). See the code examples of the data preparation using OSMSharp. 
During the data preparation phase I followed these initial steps:
- Selection of an area to model
- Defining a 17.28km x 17.28km (size of the gaming area) bounding box in the chosen location. The easiest way is to create a CSV where you write the coordinates of the bounding box. Use metric projection (e.g. variation of UTM), that way calculating the bounding box is an easy math task: you choose any coordinate and then add 17280 meters to x and y axes. See example in examples\Olomouc. You can then upload this into e.g. QGIS. 
- Recalculate the bounding box to WGS. (Metric projections are easy to calculate but for import and export, WGS coordinates are easier to use.). 
- Calculate the mid-point of the bounding box (i.e. centroid) > this will be used for conversions between geographic coordinates (in WGS) and game coordinates
- download geodata using the defined bouding box. In my case I got the OSM data from OverPass API. 
- Filtering out most of the attributes. In most cases I just need the geometry, type of object (e.g. road type). See examples. 
- Resulting CSV files should be named: roads_rwo.csv, waterway_rwo.csv, water_rwo.csv, buildings_rwo.csv, amenity_rwo.csv

# Prepare raster image for tree coverage 
There is a GeoSkylines method for creating tree coverage from raster image. To prepare the raster image follow these steps:
- For the selected area, obtain tree layer such as Urban Atlas Street Tree Layer or CORINE Landcover or layers from local authorities. 
- Clip the layer by the defined bounding box (and do another required processing e.g. filtering out unwanted data)
- In QGIS (or other GIS software), fill polygons with color, no line, turn off all other layers so you have white background and then export as image: extent defined by the bounding box, resolution should be 1081 x 1081 pixels. (sometimes it's bit off, maybe there's some better way)
- name it trees.png

# Prepare tree vector layer
Alternatively, tree coverage can be created from vector data as well. To prepare the CSV file, follow these steps:
- For the selected area, obtain tree layer such as Urban Atlas Street Tree Layer or CORINE Landcover or layers from local authorities.
- Clip the layer by the defined bounding box (and do another required processing e.g. filtering out unwanted data)
- In QGIS (or other GIS software), create a layer of regular points in the defined bounding box. You have to test the optimal distance between regular points, 10 metres is recommended. Add random offset to avoid gridded look of the regular points. 
- Clip the layer of regular points by the polygons of the tree layer
- Export clipped regular points layer as CSV with geometry in WKT format. Named it trees_rwo.csv. 

# Import methods of GeoSkylines mod
GeoSkylinesImport.ImportRoads():
- Run by hotkey combo: Ctrl + R
- Requires: roads_rwo.csv, rwo_cs_road_match.csv, import_export.conf
- Description: loops over all road segments in roads_rwo.csv, matches road types according to rwo_cs_road_match.csv, creates game nodes and then game roads, names the roads according to geodata orginals, creates a bridge if original data says bridge = yes, creates one way roads. 
- Note: Although the message "Roads import completed" will display almost immediately, it can take a long time (e.g. up to half an hour) to generate the road network. During this time the game looks like it's not doing anything. 

GeoSkylinesImport.ImportRails():
- Run by hotkey combo: Ctrl + L
- Requires: rails_rwo.csv, rwo_cs_rail_match.csv, import_export.conf
- Description: loops over all rail segments in rails_rwo.csv, matches rail types according to rwo_cs_rail_match.csv, creates game nodes and then game rails. 

GeoSkylinesImport.ImportWaterBody():
- Run by hotkey combo: Ctrl + W
- Requires: water_rwo.csv, import_export.conf
- Description: loops over all records of standing water defined by a polygon in water_rwo.csv, creates a bounding box around polygon, then every 5 metres withing the bounding box calls Ray casting algorithm to find out whether point is within polygon or not. If yes, then lower terrain by defined value (variable ImportWaterDepth, see more details below). 

GeoSkylinesImport.ImportWaterWay():
- Run by hotkey combo: Ctrl + Q
- Requires: waterway_rwo.csv, import_export.conf
- Description: loops over all segments of water way in waterway_rwo.csv, lowers terrain by defined value (variable ImportWaterWayDepths, see more details below) every 5 metres between the vertices of each segments. 

GeoSkylinesImport.ImportTreesRaster():
- Run by hotkey combo: Ctrl + T
- Requires: trees.png (1081 x 1081 resolution), import_export.conf
- Description: loops over every pixel and for every non-white pixel it creates a tree. If variable ImportTreesRasterMultiply is defined, method adjust the number of trees created (see more details below). Method adds randomness into the position of the created trees. 

GeoSkylinesImport.ImportTreesVector():
- Run by hotkey combo: Ctrl + V
- Requires: trees_rwo.csv, import_export.conf
- Description: loops over all trees in trees_rwo.csv and creates a tree.

GeoSkylinesImport.ImportZones():
- Run by hotkey combo: Ctrl + Z
- Requires: buildings_rwo.csv, rwo_cs_zone_match.csv, import_export.conf
- Description: sets zones to existing zone blocks (must be called after creating roads, this will create zone blocks as well). First it loops over every building in buildings_rwo.csv, finds zone blocks near the position of the building, matches the building type to a game zone (e.g. building type = house to zone = ResidentialLow) according to rwo_cs_zone_match.csv and then assigns selected zone to the zone blocks. 

GeoSkylinesImport.ImportServices():
- Run by hotkey combo: Ctrl + S
- Requires: amenity_rwo.csv, rwo_cs_service_match.csv, import_export.conf
- Description: loops over every amenity (service) in amenity_rwo.csv, matches amenity type to a game service building according to rwo_cs_service_match.csv and creates a service building. 
- Note: the service buildings created by this method doesn't seem to work properly but still it might be handy to know where the services are. It can be buldozed and then re-created manually. 

GeoSkylinesImport.ImportBuildings():
- Requires: buildings_rwo.csv, import_export.conf
- Description: loops over every building in buildings_rwo.csv, tries to calculate the right building rotation angle and creates the building. 
- Note: this method is not used due to many complications. Difficult to calculate the right rotation angle, buildings are offten to close to the roads, and mainly: creating buildings directly goes against the game logic where only zones are set. Although this can be overcome by mods, it was still quite unusable. 

# Export methods of GeoSkylines mod
GeoSkylinesExport.ExportSegments(): 
- Run by hotkey combo: Ctrl + G
- Requires: import_export.conf
- Description: loops over all roads created in the game and exports them as GIS data (CSV format, geometry in WKT, any meaningful information about the road as attributes). 

GeoSkylinesExport.ExportBuildings():
- Run by hotkey combo: Ctrl + H
- Requires: import_export.conf
- Description: loops over all buildings created in the game and exports them as GIS data (CSV format, geometry in WKT, any meaningful information about the building as attributes).

GeoSkylinesExport.ExportZones():
- Run by hotkey combo: Ctrl + J
- Requires: import_export.conf
- Description: loops over all zones created in the game and exports them as GIS data (CSV format, geometry in WKT, any meaningful information about the zone as attributes).

GeoSkylinesExport.ExportTrees():
- Run by hotkey combo: Ctrl + K
- Requires: import_export.conf
- Description: loops over all trees created in the game and exports them as GIS data (CSV format, geometry in WKT, any meaningful information about the tree as attributes).

# Helper methods of GeoSkylines mod
GeoSkylinesExport.DisplayLLOnMouseClick():
- Runb by hotkey combo: Ctrl + left mouse click
- Requires: import_export.conf
- Description: Displays in a message box screen, game and Lat Lon coordinates of the place of the click. 

GeoSkylinesExport.OutputPrefabInfo():
- Run by hotkey combo: Ctrl + P
- Requires: nothing
- Description: outputs all road types (NetInfo), building types (BuidlingInfo) and tree types (TreeInfo) loaded currently in the game into "c:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\output_log.txt". 
- Note: this is valuable for creating the match CSV files and setting some variables in import_export.conf

# Configuration of import and export methods
CSV files for import, CSV files for matching types of objects, trees.png file and import_export.conf file have to be stored in folder: c:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Files\. This folder is also used to store CSV files to output game objects as GIS data using the export methods (e.g. roads_cs.csv). 

File import_export.conf lists parameters for configurying the import and export methods. Here's the complete list of parameters. 

MapName: 
- Description: not used in the code, just a label for distinguishing the file from others
- Example: Svit

CenterLatitude:
- Description: Latitude of the defined mid-point. Coordinates of the mid-point are used for conversion between real-world WGS coordinates and the game coordinates. It should be pretty accurate in order to get accurate conversion of coordinates. 
- Example: 49.063148018262

CenterLongitude:
- Description: Longitude of the defined mid-point. Coordinates of the mid-point are used for conversion between real-world WGS coordinates and the game coordinates. It should be pretty accurate in order to get accurate conversion of coordinates.
- Example: 20.1857094521613

LatitudePosition: 
- Description: Some systems define WGS coordinates in lat, lon others in lon, lat. Using this parameter you can simply tell the code which is which. Range is 0 to 1. If LatitudePosition is set to 0 then latitude is the first coordinate and longitude the second and vice versa. 
- Example: 1

ImportRoadsColumns:
- Description: List of columns of the prepared roads_rwo.csv file. Columns: Road Name, Road Type, One Way, Bridge and the geometry column are required by the code. You can have more columns but these will be just ignored. 
- Example: Id,Road Name,Road Type,One Way,Lanes,Bridge,Geometry

ImportRoadsGeometryColumn: 
- Description: Specifies the name of the geometry column. The code will look for it and use this for the coordinates conversion and game object creation. 
- Example: Geometry

ImportRoadsCoordMax:
- Description: Specifies the max coord (in both directions - positive and negative) for creating game objects. Game area is 17280 x 17280, thus axes x a z range from -8640 to 8640. This represents 9x9 tile. If no value is defined, then the absolute 8640 will be used. 
- Example: 4800 (this represents the area of 5x5 tiles, game objects won't be created past this)

ImportRailsColumns:
- Description: List of columns of the prepared rails_rwo.csv file. Columns: Rail Type and the geometry column are required by the code. You can have more columns but these will be just ignored. 
- Example: Id,Rail Type,Bridge,Geometry

ImportRailsGeometryColumn: 
- Description: Specifies the name of the geometry column. The code will look for it and use this for the coordinates conversion and game object creation. 
- Example: Geometry

ImportRailsCoordMax:
- Description: Specifies the max coord (in both directions - positive and negative) for creating game objects. Game area is 17280 x 17280, thus axes x a z range from -8640 to 8640. This represents 9x9 tiles. If no value is defined, then the absolute 8640 will be used. 
- Example: 4800 (this represents the area of 5x5 tiles, game objects won't be created past this)

ImportBuildingsColumns:
- Description: List of columns of the prepared buildings_rwo.csv file. The code works with point geometry. If you have area then create a centroid of it and use that instead. 
- Example: Id, Buidling Type, Levels, Centroid, Geometry

ImportBuildingsGeometryColumn:
- Description: Specifies the name of the geometry column. The code will look for it and use this for the coordinates conversion and game object creation.
- Example: Centroid

ImportBuildingsCoordMax: 
- Description: Specifies the max coord (in both directions - positive and negative) for creating game objects. Game area is 17280 x 17280, thus axes x a z range from -8640 to 8640. This represents 9x9 tiles. If no value is defined, then the absolute 8640 will be used. 
- Example: 4800 (this represents the area of 5x5 tiles, game objects won't be created past this)

ImportServicesColumns: 
- Description: List of columns of the prepared amenity_rwo.csv file. The code works with point geometry. If you have area then create a centroid of it and use that instead. Column Amenity is required by the code. 
- Example: Id,Amenity,Centroid,Geometry

ImportServicesGeometryColumn: 
- Description: Specifies the name of the geometry column. The code will look for it and use this for the coordinates conversion and game object creation.
- Example: Centroid

ImportTreesRasterOffTolerance:
- Description: Sometimes the created map image is not exactly 1081 x 1081 but instead few pixels off (but still close enough). In this case you can specify the number of pixels the map image is off. The code will work only with map images that are within range (1081 - ImportTreesRasterOffTolerance) to (1081 + ImportTreesRasterOffTolerance). 
- Example: 1

ImportTreesRasterOffsetX & ImportTreesRasterOffsetY: 
- Description: If the tree map image (and the game trees generated based on the map image) doesn't align with other layers (roads, water basins) then it's possible to use ImportTreesRasterOffsetX & ImportTreesRasterOffsetY to move it around and align it with other layers. 
- Example: 100 (metres)

ImportTreesRasterMultiply: 
- Description: To make tree coverage denser or less dense (e.g. to avoid reaching the game limit 250 000 of trees created), you can use parameter ImportTreesRasterMultiply. The number specifies a step at which tree creation will be skipped (if number is negative) or an additional tree will be created (if number is positive). 
- Example A: -2 (every second tree creation will be skipped, i.e. the total number of trees will be divided by 2)
- Example B: 1 (at every tree creation, new additional tree will be created, i.e. multiplying the number of tree by 2). 

ImportTreesVectorColumns: 
- Description: List of columns of the prepared trees_rwo.csv file. Only geometry column is required by the code. 
- Example: WKT,id

ImportTreesVectorGeometryColumn: 
- Description: Specifies the name of the geometry column. The code will look for it and use this for the coordinates conversion and game object creation.
- Example: WKT

ImportTreesTreeTypes: 
- Description: For adding diversity in the tree creation process. List of ID of a TreeInfo (prefab) instances. It has to be IDs as names getting the TreeInfo from name didn't work in the code. For each tree creation, one TreeInfo is randomly selected from the provided list. 
- Example: 0,1,2,6,14,13 (you can use the helper method GeoSkylinesExport.OutputPrefabInfo() to list IDs of all TreeInfo instances) 

ImportTreesCoordMax: 
- Description: Specifies the max coord (in both directions - positive and negative) for creating game objects. Game area is 17280 x 17280, thus axes x a z range from -8640 to 8640. This represents 9x9 tiles. If no value is defined, then the absolute 8640 will be used. 
- Example: 8640 (this represents the area of 9x9 tiles, game objects won't be created past this)
- Note: lower the max coord if you need to lower the total number of trees if reaching the tree limit is a problem. 
 
ImportWaterWayColumns:
- Description: List of columns of the prepared waterway_rwo.csv file. Column waterway and the geometry column is required by the code. 
- Example: WKT,name,waterway

ImportWaterWayGeometryColumn:
- Description: Specifies the name of the geometry column. The code will look for it and use this for the coordinates conversion and game object creation.
- Example: WKT

ImportWaterWayTypes: 
- Description: List of waterway types that the code will work with. Any other water way type will be ignored. 
- Example A: river
- Example B: river, stream, canal

ImportWaterWayDepths: 
- Description: List of depths for water way types defined by ImportWaterWayTypes. 
- Example: 15, 10, 10 (assuming "ImportWaterWayTypes=river, stream, canal" then river basins will be 15 metres deep, stream and canal basins will be 10 metres deep). 
- Note: the depths must be exgaragated for the game water dynamics to work properly

ImportWaterWayWidths: 
- Description: List of additional widths for water way types defined by ImportWaterWayTypes. One "width" represents 16x16 metres on 1081 x 1081 grid. 
- Example: 1, 0 (assuming "ImportWaterWayTypes=river, stream" then river basins will width=2, stream basins will have width=1)

ImportWaterColumns:
- Description: List of columns of the prepared water_rwo.csv file. Only the geometry column is required by the code. 
- Example: Id,Geometry

ImportWaterGeometryColumn: 
- Description: Specifies the name of the geometry column. The code will look for it and use this for the coordinates conversion and game object creation.
- Example: Geometry

ImportWaterDepth: 
- Description: Defines the depth of standing water basins. 
- Example: 15

ExportCoordsBox: 
- Description: Xmin, Zmin, Xmax, Zmax game coordinates - only game objects within this bounding box will be exported. If not defined then bounding box is set to max (i.e. all game objects are exported). 
- Example: -250, -250, 1000, 1000
- Note: avoid using 0 as 0 will be considered as failed attempt to set the coordinate (and thus set to max coord). Instead of 0 use -1 or 1. 

# Create a playable model in Cities: Skylines (post-processing)
Calling the GeoSkylines' import methods creates just a geographically accurate base model. However, it has to be manually processed to create a playable model. Sources of water have to be added to the water basins. Connection to a highway has to be created in order for new citizens to move in. The created game objects must be fixed in some cases (e.g. due to bad input GIS data). It is recommended that the post-processing is done by an experienced C: S player. 

# Acknowledgements
This mod was inspired by another C: S mods, mainly Cimptographer (Mapper). The curved segments in this mod are created with library burningmime.curves. Data from OSM were mainly prepared with library OSMSharp. Big thanks to all these creations and their authors! 
