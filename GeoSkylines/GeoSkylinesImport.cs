using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using System.Collections;
using ColossalFramework.Math;
using ColossalFramework;
using System.Text.RegularExpressions;
//using System.Drawing;

namespace GeoSkylines
{
    public class GeoSkylinesImport
    {
        private Randomizer rand;
        private System.Random sysRand = new System.Random();
        private Dictionary<short, List<SimpleNode>> nodeMap = new Dictionary<short, List<SimpleNode>>();
        private Dictionary<string, List<uint>> zoneBlockMap = new Dictionary<string, List<uint>>();        

        private BuildingManager bm = BuildingManager.instance;
        private NetManager nm = NetManager.instance;        
        private TerrainManager tm = TerrainManager.instance;
        private SimulationManager sm = SimulationManager.instance;
        private TreeInfo ti;
        private ZoneManager zm = ZoneManager.instance;

        private string impRoadsFileName = "roads_rwo.csv";        
        private string roadMappingFileName = "rwo_cs_road_match.csv";
        private string impRailsFileName = "rails_rwo.csv";
        private string railMappingFileName = "rwo_cs_rail_match.csv";
        private string impBuildingsFileName = "buildings_rwo.csv";
        private string buildingMatchFileName = "rwo_cs_building_match.csv";
        private string impTreesRasterFileName = "trees.png";
        private string impTreesVectorFileName = "trees_rwo.csv";
        private string impWaterWayFileName = "waterway_rwo.csv";
        private string impWaterFileName = "water_rwo.csv";
        private string impServicesFileName = "amenity_rwo.csv";
        private string serviceMatchFileName = "rwo_cs_service_match.csv";

        // set max coords to avoid building objects out of the game area 
        // out of the box game area is 5x5 tiles where 1 tile is 1920 meters (absolute game area is 9x9)
        private ushort coordMax = 8640;

        private WGS84_UTM convertor = new WGS84_UTM(null);
        private UTMResult centerUTM;
        private double centerLat;
        private double centerLon;
        private ushort latitudePos;
        private ushort longitudePos;
        private string[] impRoadsColumns;
        private string impRoadsGeometryColumn;
        private ushort impRoadsCoordMax;
        private string[] impRailsColumns;
        private string impRailsGeometryColumn;
        private ushort impRailsCoordMax;        
        private string[] impBuildingsColumns;
        private string impBuildingsGeometryColumn;
        private ushort impBuildingsCoordMax;
        private ushort impTreesRasterOffTolerance;
        private int density = 1;
        private string[] impTreesVectorColumns;
        private string impTreesVectorGeometryColumn;
        private string[] impTreesTreeTypes;
        private int treeTypesLength;
        private TreeInfo[] treeTypes;
        private ushort impTreesRasterOffsetX;
        private ushort impTreesRasterOffsetY;
        private int impTreesRasterMultiply;
        private ushort impTreesCoordMax;
        private string[] impWaterWayColumns;
        private string impWaterWayGeometryColumn;
        private string[] impWaterWayTypes;
        private string[] impWaterWayDepths;
        private string[] impWaterWayWidths;
        private string[] impWaterColumns;
        private string impWaterGeometryColumn;
        private ushort impWaterDepth;
        private string[] impServicesColumns;
        private string impServicesGeometryColumn;

        private ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
        private bool confloaded;

        private Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        private List<int> heightIdxs = new List<int>();

        public GeoSkylinesImport()
        {
            LoadConfiguration();

            centerUTM = convertor.convertLatLngToUtm(centerLat, centerLon);

            longitudePos = 0;
            if (latitudePos == 0)
                longitudePos = 1;
        }

        public void LoadConfiguration()
        {
            if (!File.Exists("Files/import_export.conf"))
            {
                panel.SetMessage("GeoSkylines", "No configuration file provided!", false);
                confloaded = false;
            }

            StreamReader confSr = File.OpenText("Files/import_export.conf");
            Dictionary<string, string> conf = new Dictionary<string, string>();
            while (!confSr.EndOfStream)
            {
                string[] keyVal = confSr.ReadLine().Split(':');
                if (keyVal.Length == 2)
                    conf.Add(keyVal[0], keyVal[1]);
            }
            confSr.Close();

            foreach (KeyValuePair<string, string> a_conf in conf)
            {
                var val = a_conf.Value;
                var key = a_conf.Key;

                if (key == "CenterLatitude")
                    double.TryParse(val, out centerLat);
                else if (key == "CenterLongitude")
                    double.TryParse(val, out centerLon);
                else if (key == "LatitudePosition")
                    ushort.TryParse(val, out latitudePos);
                else if (key == "ImportRoadsColumns")
                    impRoadsColumns = val.Split(',').Select(p => p.Trim()).ToArray();
                else if (key == "ImportRoadsGeometryColumn")
                    impRoadsGeometryColumn = val.Trim() ?? "Geometry";
                else if (key == "ImportRailsColumns")
                    impRailsColumns = val.Split(',').Select(p => p.Trim()).ToArray();
                else if (key == "ImportRailsGeometryColumn")
                    impRailsGeometryColumn = val.Trim() ?? "Geometry";
                else if (key == "ImportBuildingsColumns")
                    impBuildingsColumns = val.Split(',').Select(p => p.Trim()).ToArray();
                else if (key == "ImportBuildingsGeometryColumn")
                    impBuildingsGeometryColumn = val.Trim() ?? "Geometry";
                else if (key == "ImportServicesColumns")
                    impServicesColumns = val.Split(',').Select(p => p.Trim()).ToArray();
                else if (key == "ImportServicesGeometryColumn")
                    impServicesGeometryColumn = val.Trim() ?? "Geometry";
                else if (key == "ImportTreesRasterOffTolerance")
                    ushort.TryParse(val, out impTreesRasterOffTolerance);
                else if (key == "ImportTreesVectorColumns")
                    impTreesVectorColumns = val.Split(',').Select(p => p.Trim()).ToArray();
                else if (key == "ImportTreesVectorGeometryColumn")
                    impTreesVectorGeometryColumn = val.Trim() ?? "Geometry";
                else if (key == "ImportTreesTreeTypes")
                {
                    impTreesTreeTypes = val.Split(',').Select(p => p.Trim()).ToArray();
                    treeTypesLength = Math.Max(3, impTreesTreeTypes.Length);
                    treeTypes = new TreeInfo[treeTypesLength];

                    int treeInfoId;
                    for (int i = 0; i < treeTypesLength; i++)
                    {
                        treeTypes[i] = PrefabCollection<TreeInfo>.GetLoaded((uint)i);
                        int.TryParse(impTreesTreeTypes[i], out treeInfoId);
                        if (treeInfoId != 0)
                            treeTypes[i] = PrefabCollection<TreeInfo>.GetLoaded((uint)treeInfoId);
                    }
                }
                else if (key == "ImportTreesRasterOffsetX")
                    ushort.TryParse(val, out impTreesRasterOffsetX);
                else if (key == "ImportTreesRasterOffsetY")
                    ushort.TryParse(val, out impTreesRasterOffsetY);
                else if (key == "ImportTreesRasterMultiply")
                    int.TryParse(val, out impTreesRasterMultiply);
                else if (key == "ImportWaterWayColumns")
                    impWaterWayColumns = val.Split(',').Select(p => p.Trim()).ToArray();
                else if (key == "ImportWaterWayGeometryColumn")
                    impWaterWayGeometryColumn = val.Trim() ?? "Geometry";
                else if (key == "ImportWaterWayTypes")
                    impWaterWayTypes = val.Split(',').Select(p => p.Trim()).ToArray();
                else if (key == "ImportWaterWayDepths")
                    impWaterWayDepths = val.Split(',');
                else if (key == "ImportWaterWayWidths")
                    impWaterWayWidths = val.Split(',');
                else if (key == "ImportWaterGeometryColumn")
                    impWaterGeometryColumn = val.Trim() ?? "Geometry";
                else if (key == "ImportWaterColumns")
                    impWaterColumns = val.Split(',').Select(p => p.Trim()).ToArray();
                else if (key == "ImportWaterDepth")
                    ushort.TryParse(val, out impWaterDepth);
                else if (key == "ImportRoadsCoordMax")
                {
                    ushort.TryParse(val, out impRoadsCoordMax);
                    impRoadsCoordMax = Math.Max(Math.Min(impRoadsCoordMax, coordMax), (ushort)960);
                }
                else if (key == "ImportRailsCoordMax")
                {
                    ushort.TryParse(val, out impRailsCoordMax);
                    impRailsCoordMax = Math.Max(Math.Min(impRailsCoordMax, coordMax), (ushort)960);
                }
                else if (key == "ImportBuildingsCoordMax")
                {
                    ushort.TryParse(val, out impBuildingsCoordMax);
                    impBuildingsCoordMax = Math.Max(Math.Min(impBuildingsCoordMax, coordMax), (ushort)960);
                }
                else if (key == "ImportTreesCoordMax")
                {
                    ushort.TryParse(val, out impTreesCoordMax);
                    impTreesCoordMax = Math.Max(Math.Min(impTreesCoordMax, coordMax), (ushort)960);
                }
            }              

            confloaded = true;
            
        }

        public bool FindNode(out ushort netNodeId, float[] nodeCoords)
        {
            short xRound = (short)Math.Round(nodeCoords[0]);

            if (nodeMap.ContainsKey(xRound))
            {
                foreach (SimpleNode node in nodeMap[xRound])
                {
                    if (node.nodeCoords[0] == nodeCoords[0])
                    {
                        if (node.nodeCoords[1] == nodeCoords[1])
                        {
                            netNodeId = node.nodeId;
                            return true;
                        }
                    }
                }
            }

            netNodeId = 0;
            return false;
        }

        public void ImportRoads()
        {
            if (!confloaded)
                return;

            Dictionary<string, string> roadMapping = new Dictionary<string, string>();
            if (!File.Exists("Files/" + roadMappingFileName))
            {
                panel.SetMessage("GeoSkylines", roadMappingFileName + " file doesn't exist!", false);
                return;
            }                           
            else
            {
                StreamReader r_map_sr = File.OpenText("Files/" + roadMappingFileName);
                while (!r_map_sr.EndOfStream)
                {
                    var r_map_vec = CSVParser.Split(r_map_sr.ReadLine());
                    roadMapping[r_map_vec[0]] = r_map_vec[1];
                }
            }
            
            List<GeoSkylinesRoad> segments = new List<GeoSkylinesRoad>();
            if (!File.Exists("Files/"+ impRoadsFileName))
            {
                panel.SetMessage("GeoSkylines", impRoadsFileName+" file doesn't exist!", false);
                return;
            }

            StreamReader sr = File.OpenText("Files/"+ impRoadsFileName);            

            string[] fields;
            sr.ReadLine();
            ushort cntNoName = 1;
            ushort bridges = 0;
            while (!sr.EndOfStream)
            {
                fields = CSVParser.Split(sr.ReadLine());

                string coords = "";
                string oneWay = "";
                int lanes = 1;
                string streetName = "";
                ulong roadId = 0;
                string roadType = "";
                bool bridge = false;
                for (int i = 0; i < impRoadsColumns.Length; i++)
                {
                    var columnName = impRoadsColumns[i];
                    if (columnName == impRoadsGeometryColumn)
                        ProvideCoordsString(out coords, fields[i]);

                    if (columnName.ToLower().Contains("one way"))
                    {
                        oneWay = fields[i];
                        if (oneWay.Length == 0)
                            oneWay = "no";
                    }

                    if (columnName.ToLower().Contains("lane"))                    
                        if (fields[i].Length > 0)                                                    
                            lanes = int.Parse(fields[i]);                    

                    if (columnName.ToLower().Contains("name"))
                    {
                        streetName = fields[i];
                        if (streetName.Length == 0)
                        {
                            streetName = "No Name " + cntNoName.ToString();
                            cntNoName++;
                        }
                    }

                    if (columnName.ToLower() == "id")
                        roadId = ulong.Parse(fields[i]);

                    if (columnName.ToLower().Contains("type"))
                        roadType = fields[i];

                    if (columnName.ToLower().Contains("bridge"))
                        if (fields[i] != "")
                        {
                            bridge = true;
                            bridges++;
                        }
                            
                }
                
                if (coords == "")
                    continue; 
                                
                List<float[]> segCoords = new List<float[]>();                        
                string[] coords_v = coords.Split(',');

                foreach (var nodeCoords in coords_v)
                {
                    string[] separatingChars = { " " };
                    string[] nodeCoords_v = nodeCoords.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                    var lat = double.Parse(nodeCoords_v[latitudePos].Trim());
                    var lon = double.Parse(nodeCoords_v[longitudePos].Trim());
                    UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);
                    float xCoord = (float)(utmCoords.Easting - centerUTM.Easting);
                    float zCoord = (float)(utmCoords.Northing - centerUTM.Northing);
                    if (Math.Abs(xCoord) < impRoadsCoordMax && Math.Abs(zCoord) < impRoadsCoordMax)
                    {
                        segCoords.Add(new float[] { xCoord, zCoord });
                    }
                }

                GeoSkylinesRoad csRoad = new GeoSkylinesRoad(roadId, streetName, roadType, oneWay, lanes, bridge, segCoords);
                segments.Add(csRoad);
            }

            sr.Close();

            NetInfo ni;
           
            foreach (var segment in segments)
            {
                string street_name = segment.roadName;
                List<float[]> nodes = segment.roadCoords;
                string street_type = segment.roadType;
                string oneWay = segment.oneWay;
                int lanes = segment.lanes;

                ni = PrefabCollection<NetInfo>.FindLoaded("Basic Road");

                string prefab_name = "";
                if (roadMapping.ContainsKey(street_type))
                    prefab_name = roadMapping[street_type];
                if (prefab_name == "")
                    continue;

                if (PrefabCollection<NetInfo>.LoadedExists(prefab_name))
                    ni = PrefabCollection<NetInfo>.FindLoaded(prefab_name);

                if (segment.bridge)
                {
                    string prefab_name_bridge = prefab_name + " Bridge";
                    if (PrefabCollection<NetInfo>.LoadedExists(prefab_name_bridge))
                        ni = PrefabCollection<NetInfo>.FindLoaded(prefab_name_bridge);
                }
                
                if (oneWay == "yes" && ni.name == "Basic Road")
                    ni = PrefabCollection<NetInfo>.FindLoaded("Oneway Road");

                for (int i = 0; i < nodes.Count - 1; i++)
                {
                    float[] startNodeCoords = nodes[i];
                    float[] endNodeCoords = nodes[i + 1];

                    if (!FindNode(out ushort startNetNodeId, startNodeCoords))
                    {
                        var startNodePos = new Vector3(startNodeCoords[0], 0, startNodeCoords[1]);
                        float yStart = tm.SampleRawHeightSmoothWithWater(startNodePos, false, 0f);
                        startNodePos.y = yStart;

                        if (nm.CreateNode(out startNetNodeId, ref rand, ni, startNodePos, sm.m_currentBuildIndex))
                        {
                            sm.m_currentBuildIndex += 1u;
                        }

                        short xRound = (short)Math.Round(startNodeCoords[0]);
                        if (!nodeMap.ContainsKey(xRound))
                        {
                            nodeMap.Add(xRound, new List<SimpleNode>());
                        }
                        SimpleNode simpleNode = new SimpleNode(startNetNodeId, startNodePos);
                        nodeMap[xRound].Add(simpleNode);
                    }

                    if (!FindNode(out ushort endNetNodeId, endNodeCoords))
                    {
                        var endNodePos = new Vector3(endNodeCoords[0], 0, endNodeCoords[1]);
                        float yStart = tm.SampleRawHeightSmoothWithWater(endNodePos, false, 0f);
                        endNodePos.y = yStart;

                        if (nm.CreateNode(out endNetNodeId, ref rand, ni, endNodePos, sm.m_currentBuildIndex))
                        {
                            sm.m_currentBuildIndex += 1u;
                        }

                        short xRound = (short)Math.Round(endNodeCoords[0]);
                        if (!nodeMap.ContainsKey(xRound))
                        {
                            nodeMap.Add(xRound, new List<SimpleNode>());
                        }
                        SimpleNode simpleNode = new SimpleNode(endNetNodeId, endNodePos);
                        nodeMap[xRound].Add(simpleNode);
                    }

                    Vector3 endPos = nm.m_nodes.m_buffer[endNetNodeId].m_position;
                    Vector3 startPos = nm.m_nodes.m_buffer[startNetNodeId].m_position;
                    Vector3 startDirection = VectorUtils.NormalizeXZ(endPos - startPos);

                    SimulationManager.instance.AddAction(AddRoad(rand, ni, startNetNodeId, endNetNodeId, startDirection, street_name));
                }
            }

            UpdateSegments();

            panel.SetMessage("GeoSkylines", "Roads import complete.", false);
        }

        public void ImportRails()
        {
            if (!confloaded)
                return;

            Dictionary<string, string> railMapping = new Dictionary<string, string>();

            if (!File.Exists("Files/" + railMappingFileName))
            {
                panel.SetMessage("GeoSkylines", railMappingFileName + " file doesn't exist!", false);
                return;
            }
            else
            {
                StreamReader r_map_sr = File.OpenText("Files/" + railMappingFileName);
                while (!r_map_sr.EndOfStream)
                {
                    var r_map_vec = CSVParser.Split(r_map_sr.ReadLine());
                    railMapping[r_map_vec[0]] = r_map_vec[1];
                }
            }

            List<GeoSkylinesRoad> segments = new List<GeoSkylinesRoad>();
            if (!File.Exists("Files/" + impRailsFileName))
            {
                panel.SetMessage("GeoSkylines", impRailsFileName + " file doesn't exist!", false);
                return;
            }

            StreamReader sr = File.OpenText("Files/" + impRailsFileName);

            string[] fields;
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                fields = CSVParser.Split(sr.ReadLine());

                string coords = "";
                ulong railId = 0;
                string railType = "";
                bool bridge = false;
                for (int i = 0; i < impRailsColumns.Length; i++)
                {
                    var columnName = impRailsColumns[i];
                    var columnValue = fields[i].Replace("\"", "");
                    if (columnName == impRailsGeometryColumn)
                        ProvideCoordsString(out coords, columnValue);
                    
                    if (columnName.ToLower().Contains("type"))
                        railType = columnValue;

                    if (columnName.ToLower() == "id")
                        railId = ulong.Parse(columnValue);

                    if (columnName.ToLower() == "bridge")
                        if (columnValue != "")
                            bridge = true;
                }

                if (coords == "")
                    continue;

                List<float[]> segCoords = new List<float[]>();
                string[] coords_v = coords.Split(',');

                foreach (var nodeCoords in coords_v)
                {
                    string[] separatingChars = { " " };
                    string[] nodeCoords_v = nodeCoords.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                    var lat = double.Parse(nodeCoords_v[latitudePos].Trim());
                    var lon = double.Parse(nodeCoords_v[longitudePos].Trim());
                    UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);
                    float xCoord = (float)(utmCoords.Easting - centerUTM.Easting);
                    float zCoord = (float)(utmCoords.Northing - centerUTM.Northing);
                    if (Math.Abs(xCoord) < impRailsCoordMax && Math.Abs(zCoord) < impRailsCoordMax)
                    {
                        segCoords.Add(new float[] { xCoord, zCoord });
                    }
                }

                GeoSkylinesRoad csRoad = new GeoSkylinesRoad(railId, "", railType, "", 0, bridge, segCoords);
                segments.Add(csRoad);
            }

            sr.Close();

            NetInfo ni;

            foreach (var segment in segments)
            {               
                List<float[]> nodes = segment.roadCoords;
                string rail_type = segment.roadType;

                ni = PrefabCollection<NetInfo>.FindLoaded("Train Track");
                string prefab_name = "";
                if (railMapping.ContainsKey(rail_type))
                    prefab_name = railMapping[rail_type];
                if (prefab_name == "")
                    continue;

                if (segment.bridge)
                {
                    string prefab_name_bridge = prefab_name + " Bridge";
                    if (PrefabCollection<NetInfo>.LoadedExists(prefab_name_bridge))
                        ni = PrefabCollection<NetInfo>.FindLoaded(prefab_name_bridge);
                }
                else 
                    if (PrefabCollection<NetInfo>.LoadedExists(prefab_name))
                        ni = PrefabCollection<NetInfo>.FindLoaded(prefab_name);

                for (int i = 0; i < nodes.Count - 1; i++)
                {
                    float[] startNodeCoords = nodes[i];
                    float[] endNodeCoords = nodes[i + 1];

                    if (!FindNode(out ushort startNetNodeId, startNodeCoords))
                    {
                        var startNodePos = new Vector3(startNodeCoords[0], 0, startNodeCoords[1]);
                        float yStart = tm.SampleRawHeightSmoothWithWater(startNodePos, false, 0f);
                        startNodePos.y = yStart;

                        if (nm.CreateNode(out startNetNodeId, ref rand, ni, startNodePos, sm.m_currentBuildIndex))
                        {
                            sm.m_currentBuildIndex += 1u;
                        }

                        short xRound = (short)Math.Round(startNodeCoords[0]);
                        if (!nodeMap.ContainsKey(xRound))
                        {
                            nodeMap.Add(xRound, new List<SimpleNode>());
                        }
                        SimpleNode simpleNode = new SimpleNode(startNetNodeId, startNodePos);
                        nodeMap[xRound].Add(simpleNode);
                    }

                    if (!FindNode(out ushort endNetNodeId, endNodeCoords))
                    {
                        var endNodePos = new Vector3(endNodeCoords[0], 0, endNodeCoords[1]);
                        float yStart = tm.SampleRawHeightSmoothWithWater(endNodePos, false, 0f);
                        endNodePos.y = yStart;

                        if (nm.CreateNode(out endNetNodeId, ref rand, ni, endNodePos, sm.m_currentBuildIndex))
                        {
                            sm.m_currentBuildIndex += 1u;
                        }

                        short xRound = (short)Math.Round(endNodeCoords[0]);
                        if (!nodeMap.ContainsKey(xRound))
                        {
                            nodeMap.Add(xRound, new List<SimpleNode>());
                        }
                        SimpleNode simpleNode = new SimpleNode(endNetNodeId, endNodePos);
                        nodeMap[xRound].Add(simpleNode);
                    }

                    Vector3 endPos = nm.m_nodes.m_buffer[endNetNodeId].m_position;
                    Vector3 startPos = nm.m_nodes.m_buffer[startNetNodeId].m_position;
                    Vector3 startDirection = VectorUtils.NormalizeXZ(endPos - startPos);

                    SimulationManager.instance.AddAction(AddRail(rand, ni, startNetNodeId, endNetNodeId, startDirection));
                }
            }

            panel.SetMessage("GeoSkylines", "Rails import complete.", false);
        }

        public void UpdateSegments()
        {
            for (ushort i = 0; i < nm.m_segments.m_buffer.Length; i++)
            {
                var seg = nm.m_segments.m_buffer[i];
                if (seg.m_startNode == 0 || seg.m_endNode == 0)
                    continue;
                nm.UpdateSegment(i);
            }
        }

        // working but not used (dificult to place buildings properly, also it's against the game's rules - buidling should have zone first)
        public void ImportBuildings()
        {
            if (!confloaded)
                return;            
            
            List<GeoSkylinesBuilding> buildings = new List<GeoSkylinesBuilding>();            
            if (!File.Exists("Files/" + impBuildingsFileName))
            {
                panel.SetMessage("GeoSkylines", impBuildingsFileName+" file doesn't exist!", false);
                return;
            }
            
            StreamReader sr = File.OpenText("Files/"+ impBuildingsFileName);

            string[] fields;
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                fields = CSVParser.Split(sr.ReadLine());

                string coords = "";
                float angle = 0f;
                float width = 0f;
                float height = 0f;
                ulong bldId = 0;
                string bldType = "";
                int bldLvl = 0;
                for (int i = 0; i < impBuildingsColumns.Length; i++)
                {
                    var columnName = impBuildingsColumns[i];
                    var columnValue = fields[i].Replace("\"", "");
                    Debug.Log(columnName + ": " + columnValue);
                    if (columnName == impBuildingsGeometryColumn)
                        ProvideCoordsString(out coords, columnValue);

                    else if (columnName.ToLower().Contains("type"))
                        bldType = columnValue;

                    else if (columnName.ToLower().Contains("height"))
                        height = float.Parse(columnValue);

                    else if (columnName.ToLower().Contains("level") && fields[i].Length != 0)
                        bldLvl = int.Parse(columnValue);

                    else if (columnName.ToLower().Contains("angle"))                    
                        angle = float.Parse(columnValue);

                    else if (columnName.ToLower().Contains("width"))
                        width = float.Parse(columnValue);

                    else if (columnName.ToLower() == "id")
                        bldId = ulong.Parse(columnValue);
                }

                if (coords == "")
                    continue;

                string[] separatingChars = { " " };
                string[] nodeCoords_v = coords.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                var lat = double.Parse(nodeCoords_v[latitudePos].Trim());
                var lon = double.Parse(nodeCoords_v[longitudePos].Trim());
                UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);
                float xCoord = (float)(utmCoords.Easting - centerUTM.Easting);
                float zCoord = (float)(utmCoords.Northing - centerUTM.Northing);
                if (Math.Abs(xCoord) < impBuildingsCoordMax && Math.Abs(zCoord) < impBuildingsCoordMax)
                {
                    float[] centroidCoords = new float[] { xCoord, zCoord };
                    GeoSkylinesBuilding csBld = new GeoSkylinesBuilding(bldId, bldType, bldLvl, centroidCoords, angle, width, height);
                    buildings.Add(csBld);
                }
            }

            sr.Close();

            BuildingInfo bi;
            
            foreach (var bld in buildings)
            {
                Vector3 bldPos = new Vector3(bld.bldCentroid[0], 0, bld.bldCentroid[1]);
                float yCoord = tm.SampleRawHeightSmoothWithWater(bldPos, false, 0f);
                bldPos.y = yCoord;
                string building_type = bld.bldType;
                int bldLvl = bld.bldLvl;
                float angle = bld.angle;

                string[] allowedBuildingTypes = { "yes", "residential", "house" };
                if (!allowedBuildingTypes.Contains(building_type))
                    continue;

                if (building_type == "yes")
                {
                    bi = PrefabCollection<BuildingInfo>.FindLoaded("L1 2x2 Detached03");
                }
                else if (building_type == "residential")
                {
                    bi = PrefabCollection<BuildingInfo>.FindLoaded("L1 2x2 Detached06");
                }
                else if (building_type == "house")
                {
                    bi = PrefabCollection<BuildingInfo>.FindLoaded("L1 1x1 Detached");
                }
                else
                {
                    bi = PrefabCollection<BuildingInfo>.FindLoaded("L1 2x2 Detached06");
                }

                SimulationManager.instance.AddAction(AddBuilding(bi, bldPos, angle));
            }

            panel.SetMessage("GeoSkylines", "Buildings import completed. ", false);
        }

        public void ImportTreesRaster()
        {
            if (!confloaded)
                return;
            
            string rasterFilePath = string.Format("{0}{1}", "Files/", impTreesRasterFileName.Trim());
            if (!File.Exists(rasterFilePath))
            {
                panel.SetMessage("GeoSkylines", rasterFilePath + " file doesn't exist!", false);
                return;
            }

            Texture2D tex = new Texture2D(0,0);
            byte[] bytes = File.ReadAllBytes(rasterFilePath);
            tex.LoadImage(bytes);                       

            var height = tex.height;
            var width = tex.width;
            Debug.Log("height: " + height.ToString() + " width: " + width.ToString());
            if (height < 1081 - impTreesRasterOffTolerance || height > 1081 + impTreesRasterOffTolerance ||
                width < 1081 - impTreesRasterOffTolerance || width > 1081 + impTreesRasterOffTolerance)
            {
                string msg = "The size of provided raster is not within the range ";
                msg += (1081 - impTreesRasterOffTolerance).ToString();
                msg += " - ";
                msg += (1081 + impTreesRasterOffTolerance).ToString();
                panel.SetMessage("GeoSkylines", msg, false);
                return;
            }

            float scale = 17280 / height;            

            int trees_found = 0; 
            
            Color pixel_color;

            int step = 999999999;
            if (impTreesRasterMultiply != 0)
            {
                step = impTreesRasterMultiply;
            }

            string debug_msg = "";
            int treesCreated = 0;
            for (int x = 0; x < height; x += density)
            {
                for (int y = 0; y < width; y += density)
                {  
                    pixel_color = tex.GetPixel(x, y);                                                           

                    // anything other than white
                    if (pixel_color.g < 1) 
                    {
                        trees_found++;

                        if (treesCreated > 250000)
                        {
                            Debug.Log("Tree limit 250000 reached. ");
                            break;
                        }

                        if (trees_found % step == 0)
                        {
                            if (step < 0)
                                continue;
                            else
                            {
                                if (AddTreeFromPixels(x, y, scale))
                                    treesCreated++;
                            }
                        }

                        if (AddTreeFromPixels(x,y,scale))
                            treesCreated++;

                    }
                }
            }

            Debug.Log(debug_msg);
            Debug.Log("Trees found: " + trees_found.ToString());

            panel.SetMessage("GeoSkylines", "Trees (from raster data) import completed. ", false);
        }

        public bool AddTreeFromPixels(int x, int y, float scale)
        {
            var gameX = x * scale - 8640;
            var gameZ = y * scale - 8640;
            double randomizedX = gameX + (0.5 - sysRand.NextDouble()) * scale + impTreesRasterOffsetX;
            double randomizedZ = gameZ + (0.5 - sysRand.NextDouble()) * scale + impTreesRasterOffsetY;

            if (Math.Abs(randomizedX) < impTreesCoordMax && Math.Abs(randomizedZ) < impTreesCoordMax)
            {
                var treePos = new Vector3((float)randomizedX, 0f, (float)randomizedZ);

                ti = treeTypes[sysRand.Next(0, treeTypesLength)];
                SimulationManager.instance.AddAction(AddTree(treePos, SimulationManager.instance.m_randomizer, ti));

                return true;

            }
            else
                return false;            
        }

        public void ImportTreesVector()
        {
            if (!confloaded)
                return;
            
            if (!File.Exists("Files/"+ impTreesVectorFileName))
            {
                panel.SetMessage("GeoSkylines", impTreesVectorFileName+" file doesn't exist!", false);
                return;
            }

            StreamReader sr = File.OpenText("Files/"+ impTreesVectorFileName);

            string[] fields;
            sr.ReadLine();

            int cnt = 0;
            while (!sr.EndOfStream)
            {
                fields = CSVParser.Split(sr.ReadLine());
                
                string coords = "";
                string treeType = "";
                ulong treeId = 0;
                for (int i = 0; i < impTreesVectorColumns.Length; i++)
                {
                    var columnName = impTreesVectorColumns[i];
                    var columnValue = fields[i].Replace("\"", "");
                    //Debug.Log(columnName + ": " + columnValue);
                    if (columnName == impTreesVectorGeometryColumn)
                        ProvideCoordsString(out coords, columnValue);

                    else if (columnName.ToLower().Contains("type"))
                        treeType = columnValue;

                    else if (columnName.ToLower() == "id")
                        treeId = ulong.Parse(columnValue);
                }

                if (coords == "")
                    continue;

                string[] separatingChars = { " " };
                string[] nodeCoords_v = coords.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                var lat = double.Parse(nodeCoords_v[latitudePos].Trim());
                var lon = double.Parse(nodeCoords_v[longitudePos].Trim());
                UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);
                float xCoord = (float)(utmCoords.Easting - centerUTM.Easting);
                float zCoord = (float)(utmCoords.Northing - centerUTM.Northing);                
                if (Math.Abs(xCoord) < impTreesCoordMax && Math.Abs(zCoord) < impTreesCoordMax)
                {                    
                    if (cnt > 250000)
                    {
                        Debug.Log("Tree limit 250000 reached. "); 
                        break;
                    }

                    ti = treeTypes[sysRand.Next(0, treeTypesLength)];
                    SimulationManager.instance.AddAction(AddTree(new Vector3(xCoord,0f,zCoord), rand, ti));

                    cnt++;
                }
            }

            sr.Close();

            panel.SetMessage("GeoSkylines", "Trees (from vector data) import completed. ", false);

        }

        public void ImportWaterWay()
        {
            if (!confloaded)
                return;
            
            if (!File.Exists("Files/"+ impWaterWayFileName))
            {
                panel.SetMessage("GeoSkylines", impWaterWayFileName+" file doesn't exist!", false);
                return;
            }

            string msg = "";

            StreamReader sr = File.OpenText("Files/"+ impWaterWayFileName);

            string[] fields;
            sr.ReadLine();
            
            float tmpY;            
            while (!sr.EndOfStream)
            {
                fields = CSVParser.Split(sr.ReadLine());

                string coords = "";
                string waterWayType = "";
                for (int i = 0; i < impWaterWayColumns.Length; i++)
                {
                    var columnName = impWaterWayColumns[i];
                    var columnValue = fields[i].Replace("\"", "");
                    if (columnName == impWaterWayGeometryColumn)
                        ProvideCoordsString(out coords, columnValue);
                    else if (columnName == "waterway")
                        waterWayType = columnValue;
                }

                if (coords == "")
                    continue;

                if (!impWaterWayTypes.Contains(waterWayType))
                    continue;

                float oneMetreBytes = ushort.MaxValue / (ushort)TerrainManager.TERRAIN_HEIGHT;
                ushort depthMetre;
                ushort.TryParse(impWaterWayDepths[Array.IndexOf(impWaterWayTypes, waterWayType)], out depthMetre);
                ushort depthBytes = (ushort)(depthMetre * oneMetreBytes);

                ushort widthTexel;
                ushort.TryParse(impWaterWayWidths[Array.IndexOf(impWaterWayTypes, waterWayType)], out widthTexel);                

                List<Vector3> waterwayNodes = new List<Vector3>();

                string[] separatingChars = { " " };                
                string[] coords_v = coords.Split(',');
                foreach (var nodeCoords in coords_v)
                {                    
                    string[] nodeCoords_v = nodeCoords.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                    var lat = double.Parse(nodeCoords_v[latitudePos].Trim());
                    var lon = double.Parse(nodeCoords_v[longitudePos].Trim());
                    UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);
                    float xCoord = (float)(utmCoords.Easting - centerUTM.Easting);
                    float zCoord = (float)(utmCoords.Northing - centerUTM.Northing);
                    if (Math.Abs(xCoord) < coordMax && Math.Abs(zCoord) < coordMax)
                    {
                        var pos = new Vector3(xCoord, 0f, zCoord);
                        tmpY = tm.SampleRawHeightSmoothWithWater(pos, false, 0f);
                        pos.y = tmpY;
                        waterwayNodes.Add(pos);
                    }
                }

                for (int i = 0; i < waterwayNodes.Count - 1; i++)
                {
                    var startPos = waterwayNodes[i];
                    var endPos = waterwayNodes[i + 1];

                    List<Vector3> positions = new List<Vector3>
                    {
                        startPos,
                        endPos
                    };

                    var diffX = endPos.x - startPos.x;
                    var diffZ = endPos.z - startPos.z;
                    var dist = Math.Sqrt(diffX * diffX + diffZ * diffZ);
                    //float dist = VectorUtils.LengthXZ(endPos - startPos);                    

                    int step = 5;
                    if (dist > step)
                    {
                        var stepDist = dist / step;
                        var stepX = diffX / dist * step;
                        var stepZ = diffZ / dist * step;
                        var stepPosX = startPos.x;
                        var stepPosZ = startPos.z;
                        Vector3 tmpPos = Vector3.zero;
                        for (int j = 0; j < (int)stepDist; j++)
                        {
                            stepPosX = stepPosX + (float)stepX;
                            stepPosZ = stepPosZ + (float)stepZ;
                            tmpPos.x = stepPosX;
                            tmpPos.z = stepPosZ;
                            tmpY = tm.SampleRawHeightSmoothWithWater(tmpPos, false, 0f);
                            tmpPos.y = tmpY;
                            positions.Add(tmpPos);
                        }
                    }

                    foreach (var pos in positions)
                    {
                        int pixelX = Mathf.Max((int)((pos.x) / TerrainManager.RAW_CELL_SIZE + (float)TerrainManager.RAW_RESOLUTION * 0.5f), 0);
                        int pixelZ = Mathf.Max((int)((pos.z) / TerrainManager.RAW_CELL_SIZE + (float)TerrainManager.RAW_RESOLUTION * 0.5f), 0);

                        List<int> tmpIdxs = new List<int>();
                        tmpIdxs.Add(pixelZ * (TerrainManager.RAW_RESOLUTION + 1) + pixelX);
                        for (int addedWidth=1; addedWidth < widthTexel+1; addedWidth++)
                        {
                            tmpIdxs.Add(pixelZ * (TerrainManager.RAW_RESOLUTION + 1) + (pixelX + addedWidth));
                            tmpIdxs.Add((pixelZ + addedWidth) * (TerrainManager.RAW_RESOLUTION + 1) + (pixelX + addedWidth));
                            tmpIdxs.Add((pixelZ + addedWidth) * (TerrainManager.RAW_RESOLUTION + 1) + pixelX);
                            tmpIdxs.Add((pixelZ + addedWidth) * (TerrainManager.RAW_RESOLUTION + 1) + (pixelX - addedWidth));
                            tmpIdxs.Add(pixelZ * (TerrainManager.RAW_RESOLUTION + 1) + (pixelX - addedWidth));
                            tmpIdxs.Add((pixelZ - addedWidth) * (TerrainManager.RAW_RESOLUTION + 1) + (pixelX - addedWidth));
                            tmpIdxs.Add((pixelZ - addedWidth) * (TerrainManager.RAW_RESOLUTION + 1) + pixelX);
                            tmpIdxs.Add((pixelZ - addedWidth) * (TerrainManager.RAW_RESOLUTION + 1) + (pixelX + addedWidth));
                        }

                        foreach (var idx in tmpIdxs)
                        {
                            if (heightIdxs.Contains(idx))
                                continue;
                            heightIdxs.Add(idx);
                            var currentHeight = tm.RawHeights[idx];
                            ushort newHeight = (ushort)(currentHeight - depthBytes);
                            tm.RawHeights[idx] = newHeight;                            
                            TerrainModify.UpdateArea(pixelX - 2, pixelZ - 2, pixelX + 2, pixelZ + 2, true, true, false);
                        }                                                                            
                    }
                }
            }

            panel.SetMessage("GeoSkylines", "Diging water ways complete. ", false);
        }

        public void ImportWaterBody()
        { 
            if (!confloaded)
                return;
            
            if (!File.Exists("Files/"+ impWaterFileName))
            {
                panel.SetMessage("GeoSkylines", impWaterFileName+" file doesn't exist!", false);
                return;
            }

            string msg = "";

            StreamReader sr = File.OpenText("Files/"+ impWaterFileName);

            string[] fields;
            sr.ReadLine();

            while (!sr.EndOfStream)
            {
                fields = CSVParser.Split(sr.ReadLine());

                string coords = "";
                for (int i = 0; i < impWaterColumns.Length; i++)
                {
                    var columnName = impWaterColumns[i];
                    var columnValue = fields[i].Replace("\"", "");
                    if (columnName == impWaterGeometryColumn)
                        ProvideCoordsString(out coords, columnValue);
                }

                if (coords == "")
                    continue;

                float oneMetreBytes = ushort.MaxValue / (ushort)TerrainManager.TERRAIN_HEIGHT;
                ushort depthMetre = 15;
                if (impWaterDepth != 0)
                    depthMetre = impWaterDepth;
                ushort depthBytes = (ushort)(depthMetre * oneMetreBytes);

                List<Vector2> waterbodyNodes = new List<Vector2>();
                float xMin = 8641;
                float xMax = -8641;
                float yMin = 8641;
                float yMax = -8641;

                string[] separatingChars = { " " };
                string[] coords_v = coords.Split(',');
                foreach (var nodeCoords in coords_v)
                {
                    string[] nodeCoords_v = nodeCoords.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                    var lat = double.Parse(nodeCoords_v[latitudePos].Trim());
                    var lon = double.Parse(nodeCoords_v[longitudePos].Trim());
                    UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);
                    float xCoord = (float)(utmCoords.Easting - centerUTM.Easting);
                    float yCoord = (float)(utmCoords.Northing - centerUTM.Northing);
                    if (Math.Abs(xCoord) < coordMax && Math.Abs(yCoord) < coordMax)
                    {
                        var pos = new Vector2(xCoord, yCoord);                        
                        waterbodyNodes.Add(pos);
                        xMin = Mathf.Min(xMin, pos.x);
                        xMax = Mathf.Max(xMax, pos.x);
                        yMin = Mathf.Min(yMin, pos.y);
                        yMax = Mathf.Max(yMax, pos.y);
                    }
                }

                Vector2[] polygonVertices = waterbodyNodes.ToArray();

                int xMinInt = (int)xMin;
                int xMaxInt = (int)xMax + 1;
                int yMinInt = (int)yMin;
                int yMaxInt = (int)yMax + 1;
                
                for (int i = 0; i < (xMaxInt - xMinInt); i+=5)
                    for (int j = 0; j < (yMaxInt - yMinInt); j+=5)
                    {
                        Vector2 pos = new Vector2(xMin + i, yMin + j);
                        if (IsPointInPolygon(polygonVertices, pos))
                        {
                            int pixelX = Mathf.Max((int)((pos.x) / TerrainManager.RAW_CELL_SIZE + (float)TerrainManager.RAW_RESOLUTION * 0.5f), 0);
                            int pixelZ = Mathf.Max((int)((pos.y) / TerrainManager.RAW_CELL_SIZE + (float)TerrainManager.RAW_RESOLUTION * 0.5f), 0);

                            int idx = pixelZ * (TerrainManager.RAW_RESOLUTION + 1) + pixelX;

                            if (heightIdxs.Contains(idx))
                                continue;
                            heightIdxs.Add(idx);
                            var currentHeight = TerrainManager.instance.RawHeights[idx];
                            ushort newHeight = (ushort)(currentHeight - depthBytes);
                            TerrainManager.instance.RawHeights[idx] = newHeight;                            
                        }
                    }

                int xMinArea = Mathf.Max((int)((xMinInt) / TerrainManager.RAW_CELL_SIZE + (float)TerrainManager.RAW_RESOLUTION * 0.5f), 0);
                int xMaxArea = Mathf.Min((int)((xMaxInt) / TerrainManager.RAW_CELL_SIZE + (float)TerrainManager.RAW_RESOLUTION * 0.5f), TerrainManager.RAW_RESOLUTION);
                int yMinArea = Mathf.Max((int)((yMinInt) / TerrainManager.RAW_CELL_SIZE + (float)TerrainManager.RAW_RESOLUTION * 0.5f), 0);
                int yMaxArea = Mathf.Min((int)((yMaxInt) / TerrainManager.RAW_CELL_SIZE + (float)TerrainManager.RAW_RESOLUTION * 0.5f), TerrainManager.RAW_RESOLUTION);

                TerrainModify.UpdateArea(xMinArea-1, yMinArea-1, xMaxArea+1, yMaxArea+1, true, true, false);
            }

            panel.SetMessage("GeoSkylines", "Diging water reservoirs complete. ", false);
        }

        private bool IsPointInPolygon(Vector2[] polygon, Vector2 point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }

        // not working!
        public void ImportZones()
        {
            
            var zoneBlocks = zm.m_blocks.m_buffer;
            string msg = "";
            ItemClass.Zone zone = ItemClass.Zone.ResidentialLow;            
            //for (ushort i = 0; i < zoneBlocks.Length; i++)
            //{
            //    var zoneBlock = zoneBlocks[i];
            //    var pos = zoneBlock.m_position;

            //    if (pos == Vector3.zero)
            //        continue;

            //    var tmpZone = zoneBlock.GetZone(0, 0);
            //    if (tmpZone != ItemClass.Zone.Unzoned &&
            //        tmpZone != ItemClass.Zone.None &&
            //        tmpZone != ItemClass.Zone.Distant)
            //    {
            //        zone = zoneBlock.GetZone(0, 0);
            //        break;
            //    }                    
            //}

            //Debug.Log(zone);
            for (ushort i = 0; i < zoneBlocks.Length; i++)
            {
                var zoneBlock = zoneBlocks[i];                
                var pos = zoneBlock.m_position;

                if (pos == Vector3.zero)
                    continue;

                msg += "zoneBlock.m_valid (before SetZone): " + zoneBlock.m_valid;
                msg += "\n";                
                //msg += "zoneBlock.m_valid (after SetZone): " + zoneBlock.m_valid;
                //msg += "\n";
                //debugMsg += "zoneBlockId: " + i;
                //debugMsg += "\n";
                //int num = (int)((zoneBlock.m_flags & 65280u) >> 8);
                //debugMsg += "num: " + num;
                //debugMsg += "\n";
                //for (int z = 0; z < num; z++)
                //    for (int x = 0; x < 4; x++)
                //    {
                //        debugMsg += "x: " + x + ", z: " + z + " ...";
                //        if (zoneBlock.SetZone(x, z, zone))                        
                //            debugMsg += "Zone set";
                //        debugMsg += "\n";
                //    }

                //zoneBlock.RefreshZoning(i);
                //msg += "zoneBlock.m_valid (after RefreshingZone): " + zoneBlock.m_valid;
                //msg += "\n";
                //UpdateZone(zone);
                //msg += "zoneBlock.m_valid (after UpdateZone): " + zoneBlock.m_valid;
                //msg += "\n";

                //var pixelX = (int)(pos.x / TerrainManager.RAW_CELL_SIZE + 540);                
                //var pixelZ = (int)(pos.z / TerrainManager.RAW_CELL_SIZE + 540);
                //zoneBlock.ZonesUpdated(i, pixelX - 10, pixelZ - 10, pixelX + 10, pixelZ + 10);
                var valid = zoneBlock.m_valid;
                ulong num6 = 144680345676153346;
                for (int index = 0; index < 7; ++index)
                {
                    valid = (ulong)((long)valid & ~(long)num6 | (long)valid & (long)valid << 1 & (long)num6);
                    num6 <<= 1;
                }
                zoneBlock.m_valid = valid;
                msg += "zoneBlock.m_valid (after SetZone): " + zoneBlock.m_valid;
                msg += "\n";

                Debug.Log(msg);
            }
            //Debug.Log(debugMsg);
        }

        public void ImportServices()
        {
            if (!confloaded)
                return;            

            if (!File.Exists("Files/" + impServicesFileName))
            {
                panel.SetMessage("GeoSkylines", impServicesFileName + " file doesn't exist!", false);
                return;
            }

            Dictionary<string, uint> serviceMapping = new Dictionary<string, uint>();            
            if (!File.Exists("Files/" + serviceMatchFileName))
            {
                panel.SetMessage("GeoSkylines", serviceMatchFileName + " file doesn't exist!", false);
                return;
            }
            else
            {
                StreamReader s_map_sr = File.OpenText("Files/" + serviceMatchFileName);
                while (!s_map_sr.EndOfStream)
                {
                    var s_map_vec = CSVParser.Split(s_map_sr.ReadLine());
                    uint prefabIndex;
                    uint.TryParse(s_map_vec[2], out prefabIndex);
                    serviceMapping[s_map_vec[0]] = prefabIndex;
                }
            }

            List<GeoSkylinesService> services = new List<GeoSkylinesService>();
            StreamReader sr = File.OpenText("Files/" + impServicesFileName);

            string[] fields;
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                fields = CSVParser.Split(sr.ReadLine());

                string coords = "";
                ulong serviceId = 0;
                string serviceType = "";
                for (int i = 0; i < impServicesColumns.Length; i++)
                {
                    var columnName = impServicesColumns[i];
                    var columnValue = fields[i].Replace("\"", "");
                    //Debug.Log(columnName + ": " + columnValue);
                    if (columnName == impServicesGeometryColumn)
                        ProvideCoordsString(out coords, columnValue);

                    else if (columnName.ToLower().Contains("amenity"))
                        serviceType = columnValue;

                    else if (columnName.ToLower() == "id")
                        serviceId = ulong.Parse(columnValue);
                }

                if (coords == "")
                    continue;

                string[] separatingChars = { " " };
                string[] nodeCoords_v = coords.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                var lat = double.Parse(nodeCoords_v[latitudePos].Trim());
                var lon = double.Parse(nodeCoords_v[longitudePos].Trim());
                UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);
                float xCoord = (float)(utmCoords.Easting - centerUTM.Easting);
                float zCoord = (float)(utmCoords.Northing - centerUTM.Northing);
                if (Math.Abs(xCoord) < impBuildingsCoordMax && Math.Abs(zCoord) < impBuildingsCoordMax)
                {
                    float[] centroidCoords = new float[] { xCoord, zCoord };
                    GeoSkylinesService csService = new GeoSkylinesService(serviceId, serviceType, centroidCoords);
                    services.Add(csService);
                }
            }

            sr.Close();

            BuildingInfo bi;

            foreach (var a_service in services)
            {
                Vector3 servicePos = new Vector3(a_service.serviceCentroid[0], 0, a_service.serviceCentroid[1]);
                float yCoord = tm.SampleRawHeightSmoothWithWater(servicePos, false, 0f);
                servicePos.y = yCoord;
                string service_type = a_service.serviceType;

                bi = PrefabCollection<BuildingInfo>.FindLoaded("MerryGoRound");
                uint prefab_id = 0;
                if (serviceMapping.ContainsKey(service_type))
                    prefab_id = serviceMapping[service_type];
                if (prefab_id == 0)
                    continue;

                bi = PrefabCollection<BuildingInfo>.GetLoaded(prefab_id);

                //float angle = Mathf.Atan2(servicePos.x, -servicePos.z);
                float angle = 0f;

                SimulationManager.instance.AddAction(AddBuilding(bi, servicePos, angle));
            }

            panel.SetMessage("GeoSkylines", "Services import completed. ", false);
        }

        // this should have been a method to assign zones according to type of building but it setting zones doesn't work for unknown reason
        public void ImportZones2()
        {
            if (!confloaded)
                return;

            List<GeoSkylinesBuilding> buildings = new List<GeoSkylinesBuilding>();

            if (!File.Exists("Files/" + impBuildingsFileName))
            {
                panel.SetMessage("GeoSkylines", impBuildingsFileName + " file doesn't exist!", false);
                return;
            }

            Dictionary<string, string> zoneMapping = new Dictionary<string, string>();              
            if (!File.Exists("Files/" + buildingMatchFileName))
            {
                panel.SetMessage("GeoSkylines", buildingMatchFileName + " file doesn't exist!", false);
                return;
            }
            else
            {
                StreamReader z_map_sr = File.OpenText("Files/" + buildingMatchFileName);
                while (!z_map_sr.EndOfStream)
                {
                    var z_map_vec = CSVParser.Split(z_map_sr.ReadLine());
                    zoneMapping[z_map_vec[0]] = z_map_vec[1];
                }
            }

            var zoneBlocks = zm.m_blocks.m_buffer;
            for (ushort i = 0; i < zoneBlocks.Length; i++)
            {
                var zoneBlock = zoneBlocks[i];
                var pos = zoneBlock.m_position;
                if (pos != Vector3.zero)
                {
                    var zoneMapX = pos.x / 120;
                    var zoneMapZ = pos.z / 120;
                    string zoneMapXZ = zoneMapX + "," + zoneMapZ;
                    
                    if (!zoneBlockMap.ContainsKey(zoneMapXZ))
                    {
                        zoneBlockMap.Add(zoneMapXZ, new List<uint>());
                    }
                    zoneBlockMap[zoneMapXZ].Add(i);
                }
            }


            StreamReader sr = File.OpenText("Files/" + impBuildingsFileName);

            string[] fields;
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                fields = CSVParser.Split(sr.ReadLine());

                string coords = "";
                float angle = 0f;
                float width = 0f;
                float height = 0f;
                ulong bldId = 0;
                string bldType = "";
                int bldLvl = 0;
                for (int i = 0; i < impBuildingsColumns.Length; i++)
                {
                    var columnName = impBuildingsColumns[i];
                    var columnValue = fields[i].Replace("\"", "");
                    //Debug.Log(columnName + ": " + columnValue);
                    if (columnName == impBuildingsGeometryColumn)
                        ProvideCoordsString(out coords, columnValue);

                    else if (columnName.ToLower().Contains("type"))
                        bldType = columnValue;

                    else if (columnName.ToLower().Contains("height"))
                        height = float.Parse(columnValue);

                    else if (columnName.ToLower().Contains("level") && fields[i].Length != 0)
                        bldLvl = int.Parse(columnValue);

                    else if (columnName.ToLower().Contains("angle"))
                        angle = float.Parse(columnValue);

                    else if (columnName.ToLower().Contains("width"))
                        width = float.Parse(columnValue);

                    else if (columnName.ToLower() == "id")
                        bldId = ulong.Parse(columnValue);
                }

                if (coords == "")
                    continue;

                string[] separatingChars = { " " };
                string[] nodeCoords_v = coords.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                var lat = double.Parse(nodeCoords_v[latitudePos].Trim());
                var lon = double.Parse(nodeCoords_v[longitudePos].Trim());
                UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);
                float xCoord = (float)(utmCoords.Easting - centerUTM.Easting);
                float zCoord = (float)(utmCoords.Northing - centerUTM.Northing);
                if (Math.Abs(xCoord) < impBuildingsCoordMax && Math.Abs(zCoord) < impBuildingsCoordMax)
                {
                    float[] centroidCoords = new float[] { xCoord, zCoord };
                    GeoSkylinesBuilding csBld = new GeoSkylinesBuilding(bldId, bldType, bldLvl, centroidCoords, angle, width, height);
                    buildings.Add(csBld);
                }
            }

            sr.Close();

            BuildingInfo bi;

            foreach (var bld in buildings)
            {
                Vector3 bldPos = new Vector3(bld.bldCentroid[0], 0, bld.bldCentroid[1]);
                float yCoord = tm.SampleRawHeightSmoothWithWater(bldPos, false, 0f);
                bldPos.y = yCoord;
                string building_type = bld.bldType;
                int bldLvl = bld.bldLvl;
                float angle = bld.angle;
                //ItemClass.Zone.CommercialHigh; //5
                //ItemClass.Zone.CommercialLow; //4
                //ItemClass.Zone.Distant; //1
                //ItemClass.Zone.Industrial; //6
                //ItemClass.Zone.None; //15
                //ItemClass.Zone.Office; //7
                //ItemClass.Zone.ResidentialHigh; //3
                //ItemClass.Zone.ResidentialLow; //2
                //ItemClass.Zone.Unzoned; //0

                string zone_name = "";
                if (zoneMapping.ContainsKey(building_type))
                    zone_name = zoneMapping[building_type];
                if (zone_name == "")
                    continue;

                var zoneMapX = bldPos.x / 120;
                var zoneMapZ = bldPos.z / 120;
                string zoneMapXZ = zoneMapX + "," + zoneMapZ;
                if (!zoneBlockMap.ContainsKey(zoneMapXZ))
                {
                    Debug.Log("Not found in zoneBlockMap: " + zoneMapXZ);
                    continue;
                }

                foreach (var zoneBlockId in zoneBlockMap[zoneMapXZ])
                {
                    var zoneBlock = zm.m_blocks.m_buffer[zoneBlockId];
                    var blockPos = zoneBlock.m_position;
                    if ((blockPos.x < bldPos.x+5 && blockPos.x > bldPos.x-5) && 
                        (blockPos.z < bldPos.z+5 && blockPos.z > bldPos.z-5))
                    {                        
                        for (int x = 0; x < zoneBlock.Distance; x++)
                            for (int z = 0; z < zoneBlock.RowCount; z++)
                            {
                                ItemClass.Zone zone = ItemClass.Zone.None;
                                switch (zone_name)
                                {
                                    case "CommercialHigh":
                                        zone = ItemClass.Zone.CommercialHigh;
                                        break;
                                    case "CommercialLow":
                                        zone = ItemClass.Zone.CommercialLow;
                                        break;
                                    case "Industrial":
                                        zone = ItemClass.Zone.Industrial;
                                        break;
                                    case "Office":
                                        zone = ItemClass.Zone.Office;
                                        break;
                                    case "ResidentialHigh":
                                        zone = ItemClass.Zone.ResidentialHigh;
                                        break;
                                    case "ResidentialLow":
                                        zone = ItemClass.Zone.ResidentialLow;
                                        break;
                                }

                                zoneBlock.SetZone(x, z, zone);
                            }
                    }
                }

                //string[] allowedBuildingTypes = { "yes", "residential", "house" };
                //if (!allowedBuildingTypes.Contains(building_type))
                //    continue;                

                //SimulationManager.instance.AddAction(AddBuilding(bi, bldPos, angle));
            }

            panel.SetMessage("GeoSkylines", "Zones import completed. ", false);

        }

        private IEnumerator AddRoad(Randomizer rand, NetInfo ni, ushort startNode, ushort endNode, Vector3 startDirection, string street_name)
        {
            ushort segmentId;
            NetManager net_manager = NetManager.instance;            
            try
            {
                if (net_manager.CreateSegment(out segmentId, ref rand, ni, startNode, endNode, startDirection, -startDirection, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                {
                    Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;                    
                    net_manager.SetSegmentNameImpl(segmentId, street_name);                    
                }
            }
            catch (Exception ex)
            {
                //try-catch just to prevent crashing by ignoring invalid trees and letting valid trees get created
                //RaiseTreeMapperEvent (ex.Message);
            }
            yield return null;
        }

        private IEnumerator AddRail(Randomizer rand, NetInfo ni, ushort startNode, ushort endNode, Vector3 startDirection)
        {
            ushort segmentId;
            NetManager net_manager = NetManager.instance;
            try
            {
                if (net_manager.CreateSegment(out segmentId, ref rand, ni, startNode, endNode, startDirection, -startDirection, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false))
                {
                    Singleton<SimulationManager>.instance.m_currentBuildIndex += 2u;
                }
            }
            catch (Exception ex)
            {
                //try-catch just to prevent crashing by ignoring invalid trees and letting valid trees get created
                //RaiseTreeMapperEvent (ex.Message);
            }
            yield return null;
        }

        private IEnumerator AddBuilding(BuildingInfo bi, Vector3 bldPos, float angle)
        {
            BuildingManager bm = BuildingManager.instance;
            try
            {

                if (bm.CreateBuilding(out ushort bldId, ref rand, bi, bldPos, angle, bi.GetLength(), Singleton<SimulationManager>.instance.m_currentBuildIndex))
                {
                    ++Singleton<SimulationManager>.instance.m_currentBuildIndex;
                    //Debug.Log("New segment ID: " + segmentId.ToString() + " and name: " + net_manager.GetSegmentName(segmentId));
                }
            }
            catch (Exception ex)
            {
                //try-catch just to prevent crashing by ignoring invalid trees and letting valid trees get created
                //RaiseTreeMapperEvent (ex.Message);
            }
            yield return null;
        }

        private IEnumerator AddTree(Vector3 treePos, Randomizer rr, TreeInfo treeI)
        {
            uint treeNum;
            TreeManager tree_manager = TreeManager.instance;
            try
            {
                if (!tree_manager.CreateTree(out treeNum, ref rr, treeI, treePos, false))
                    Debug.Log("Didn't create tree");
            }
            catch (Exception ex)
            {
                //try-catch just to prevent crashing by ignoring invalid trees and letting valid trees get created
                //RaiseTreeMapperEvent (ex.Message);
            }
            yield return null;
        }

        static void ProvideCoordsString(out string coords, string wkt)
        {
            coords = wkt;
            coords = coords.Replace("POINT", "");
            coords = coords.Replace("LINESTRING", "");
            coords = coords.Replace("LINEARRING", "");
            coords = coords.Replace("POLYGON", "");
            coords = coords.Replace(" (", "");
            coords = coords.Replace("(", "");
            coords = coords.Replace(")", "");
            if (coords.Contains("\""))
                coords = coords.Replace("\"", "");
        }
    }
}
