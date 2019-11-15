# GeoSkylines
Cities: Skylines game mod for import/export of geodata. 

Three stages of creating playable model in Cities: Skylines based on geodata:
1. Prepare geodata for import
2. Create base model using GeoSkylines methods
3. Create playable model (manual post-processing)

# Prepare vector geodata for import
I chose to use a simple CSV format with geometry data recorded as WKT. Thus, any geo-dataset can be used. For testing I used OSM predominantly. For the CSV file preparation I used OSMSharp library (other programs such as QGIS or FME would suffice). See the code examples of the data preparation using OSMSharp. 
During the data preparation phase I followed these initial steps:
- Selection of an area to model
- Defining a 17.28km x 17.28km (size of the gaming area) bounding box in the chosen location. The easiest way is to create a CSV where you write the coordinates of the bounding box. Use metric projection (e.g. variation of UTM), that way calculating the bounding box is an easy math task: you choose any coordinate and then add 17280 metres to x and y axes. See example in examples\Olomouc. You can then upload this into e.g. QGIS. 
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
- Description: loops over all roads created in the game and exports them as GIS data (CSV format, geometry in WKT, any meaningfull information about the road as attributes). 

GeoSkylinesExport.ExportBuildings():
- Run by hotkey combo: Ctrl + H
- Requires: import_export.conf
- Description: loops over all buildings created in the game and exports them as GIS data (CSV format, geometry in WKT, any meaningfull information about the building as attributes).

GeoSkylinesExport.ExportZones():
- Run by hotkey combo: Ctrl + J
- Requires: import_export.conf
- Description: loops over all zones created in the game and exports them as GIS data (CSV format, geometry in WKT, any meaningfull information about the zone as attributes).

GeoSkylinesExport.ExportTrees():
- Run by hotkey combo: Ctrl + K
- Requires: import_export.conf
- Description: loops over all trees created in the game and exports them as GIS data (CSV format, geometry in WKT, any meaningfull information about the tree as attributes).

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

File import_export.conf ...


The development of GeoSkylines was focused on providing following features:
- Additional import/export of buildings and zones
- Game roads are named according to the source road data
- Minimalistic design of the mod so it is less prone to break after frequent game updates
- Not tied directly to OSM, other datasets can be used 
- More freedom in formatting the source geodata before importing them into the game
- Standard coordinate conversion methods

Install game mod:
- use the provided GeoSkylines.dll or compile the code using e.g. Visual Studio
- go to c:\Users\<username>\AppData\Local\Colossal Order\Cities_Skylines\Addons\Mods\
- create a folder GeoSkylines
- copy the GeoSkylines.dll file to newly created folder GeoSkylines
- in game, go to Content Manager > Mods > turn on GeoSkylines

Import roads:
- obtain road data (e.g. from OSM) for selected area
- convert road data into a flat file (CSV) named 'roads_cs.csv' with these columns: Id, Road Name, Road Type, One Way, Lanes, Geometry (but the only mandatory information is geometry, default values can be used for other columns)
- Geometry must be in well-known text (WKT) format
- copy roads_cs.csv file to c:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Files (folder may differ according to the installation folder of Steam)
- identify the mid-point of the selected area (e.g. using https://www.movable-type.co.uk/scripts/latlong.html) 
- in fodler c:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Files\ create a file named 'import_export.conf' that holds this information: MapName: value, CenterLatitude: value, CenterLongitude: value (see attached example)
- in game (with the mod activated), hit the keys "Ctrl + H"
- wait until roads are created (might take minutes if importing large area)

Export geodata:
- you have to define the mid-point of the exported area (how are the data in game related to places on Earth; this information should be known from previous import of either elevation or roads)
- in folder c:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Files\ create a file named 'import_export.conf' that holds this information: MapName: value, CenterLatitude: value, CenterLongitude: value (see attached example)
- in game (with the mod activated), hit the keys "Ctrl + G"
- game data will be exported to flat files and stored in c:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Files\ 
