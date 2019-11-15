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
