# GeoSkylines
Cities: Skylines game mod for import/export of geodata. 

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
