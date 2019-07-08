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

namespace GeoSkylines
{
    public class GeoSkylinesThreading : ThreadingExtensionBase
    {
        private bool _processed = false;
        ushort tmpSegId = 0;

        private Randomizer rand;
        private Dictionary<short, List<SimpleNode>> nodeMap = new Dictionary<short, List<SimpleNode>>();
        private List<Road> segments = new List<Road>();
        private List<Building> buildings = new List<Building>();

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

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.O))
            {
                if (_processed) return;

                _processed = true;



            }
            else
            {
                _processed = false;
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.I))
            {


            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.G))
            {
                if (_processed) return;

                _processed = true;

                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                string msg = "";

                NetManager nm = NetManager.instance;
                NetInfo ni;
                TerrainManager tm = TerrainManager.instance;
                SimulationManager sm = SimulationManager.instance;

                if (!File.Exists("Files/import_export.conf"))
                {
                    panel.SetMessage("Jan's mod", "no conf file provided!", false);
                    return;
                }

                StreamReader confSr = File.OpenText("Files/import_export.conf");
                Dictionary<string, string> conf = new Dictionary<string, string>();
                while (!confSr.EndOfStream)
                {
                    string[] keyVal = confSr.ReadLine().Split(':');
                    if (keyVal.Length == 2)
                        conf.Add(keyVal[0], keyVal[1]);
                }

                double centerLat = double.Parse(conf["CenterLatitude"]);
                double centerLon = double.Parse(conf["CenterLongitude"]);

                WGS84_UTM convertor = new WGS84_UTM(null);
                UTMResult centerUTM = convertor.convertLatLngToUtm(centerLat, centerLon);

                List<string> txtLines = new List<string>
                {
                    "Id, Tree Type, Latitude, Longitude"
                };


                TreeManager tree_manager = TreeManager.instance;
                TreeInstance[] trees = tree_manager.m_trees.m_buffer;
                int cnt = 0;
                foreach (var tree in trees)
                {
                    var rwoX = tree.Position.x + centerUTM.Easting;
                    var rwoY = tree.Position.z + centerUTM.Northing;
                    LatLng rwoLL = convertor.convertUtmToLatLng(rwoX, rwoY, centerUTM.ZoneNumber, centerUTM.ZoneLetter);
                    if (cnt == 0)
                    {
                        msg += rwoX.ToString();
                        msg += "\n";
                        msg += rwoY.ToString();
                        msg += "\n";
                        msg += rwoLL.Lat.ToString();
                        msg += "\n";
                        msg += rwoLL.Lng.ToString();
                        msg += "\n";
                    }

                    var treeType = tree.Info.name;
                    cnt++;

                    txtLines.Add(string.Format("{0},{1},{2},{3}", cnt, treeType, rwoLL.Lat, rwoLL.Lng));
                }

                StreamWriter outputFile = new StreamWriter("Files/trees_rwo.csv", false, new UTF8Encoding(true));
                foreach (var lineTxt in txtLines)
                {
                    outputFile.WriteLine(lineTxt);
                }
                outputFile.Close();

                panel.SetMessage("Jan's mod", msg, false);
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.H))
            {
                if (_processed) return;

                _processed = true;

                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                string msg = "";

                NetManager nm = NetManager.instance;
                NetInfo ni;
                TerrainManager tm = TerrainManager.instance;
                SimulationManager sm = SimulationManager.instance;

                // set max coords to avoid building objects out of the game area 
                // out of the box game area is 5x5 tiles where 1 tile is 1920 meters (absolute game area is 9x9)
                ushort coordMax = 4800;

                if (!File.Exists("Files/import_export.conf"))
                {
                    panel.SetMessage("Jan's mod", "no conf file provided!", false);
                    return;
                }

                StreamReader confSr = File.OpenText("Files/import_export.conf");
                Dictionary<string, string> conf = new Dictionary<string, string>();
                while (!confSr.EndOfStream)
                {
                    string[] keyVal = confSr.ReadLine().Split(':');
                    if (keyVal.Length == 2)
                        conf.Add(keyVal[0], keyVal[1]);
                }

                double centerLat = double.Parse(conf["CenterLatitude"]);
                double centerLon = double.Parse(conf["CenterLongitude"]);

                WGS84_UTM convertor = new WGS84_UTM(null);
                UTMResult centerUTM = convertor.convertLatLngToUtm(centerLat, centerLon);

                if (!File.Exists("Files/roads_cs.csv"))
                {
                    panel.SetMessage("Jan's mod", "file doesn't exist!", false);
                    return;
                }

                Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                StreamReader sr = File.OpenText("Files/roads_cs.csv");

                string[] fields;
                sr.ReadLine();
                ushort cntNoName = 1;
                while (!sr.EndOfStream)
                {
                    fields = CSVParser.Split(sr.ReadLine());

                    var coords = fields[5];
                    if (!coords.Contains("LINESTRING"))
                        continue;

                    var streetName = fields[1];
                    if (streetName.Length == 0)
                    {
                        streetName = "No Name " + cntNoName.ToString();
                        cntNoName++;
                    }
                    //segments.Add(streetName, new List<float[]>());
                    List<float[]> segCoords = new List<float[]>();

                    var roadId = ulong.Parse(fields[0]);
                    var roadType = fields[2];
                    var oneWay = fields[3];
                    if (oneWay.Length == 0)
                        oneWay = "no";
                    int lanes;
                    if (fields[4].Length == 0)
                        lanes = 1;
                    else
                        lanes = int.Parse(fields[4]);

                    coords = coords.Replace("LINESTRING (", "");
                    coords = coords.Replace(")", "");
                    if (coords.Contains("\""))
                        coords = coords.Replace("\"", "");
                    string[] coords_v = coords.Split(',');

                    foreach (var nodeCoords in coords_v)
                    {
                        string[] separatingChars = { " " };
                        string[] nodeCoords_v = nodeCoords.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                        var lat = double.Parse(nodeCoords_v[1].Trim());
                        var lon = double.Parse(nodeCoords_v[0].Trim());
                        UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);
                        float xCoord = (float)(utmCoords.Easting - centerUTM.Easting);
                        float zCoord = (float)(utmCoords.Northing - centerUTM.Northing);
                        if (Math.Abs(xCoord) < coordMax && Math.Abs(zCoord) < coordMax)
                        {
                            segCoords.Add(new float[] { xCoord, zCoord });
                        }
                    }

                    Road csRoad = new Road(roadId, streetName, roadType, oneWay, lanes, segCoords);
                    segments.Add(csRoad);
                }

                sr.Close();

                //foreach (KeyValuePair<string, List<float[]>> segment in segments)
                foreach (var segment in segments)
                {
                    string street_name = segment.roadName;
                    List<float[]> nodes = segment.roadCoords;
                    string street_type = segment.roadType;
                    string oneWay = segment.oneWay;
                    int lanes = segment.lanes;

                    // for now allow only certain road types
                    string[] allowedRoadTypes = { "primary", "secondary", "tertiary", "residential", "service", "road" };
                    if (!allowedRoadTypes.Contains(street_type))
                        continue;

                    if (street_type == "service")
                    {
                        ni = PrefabCollection<NetInfo>.FindLoaded("Gravel Road");
                        //PrefabCollection<NetInfo>.GetPrefab(11);
                    }
                    else if (oneWay == "yes")
                    {
                        ni = PrefabCollection<NetInfo>.FindLoaded("Oneway Road");
                    }
                    else if (lanes == 2)
                    {
                        ni = PrefabCollection<NetInfo>.FindLoaded("Medium Road");
                    }
                    else
                    {
                        ni = PrefabCollection<NetInfo>.FindLoaded("Basic Road");
                    }

                    for (int i = 0; i < nodes.Count - 1; i++)
                    {
                        float[] startNodeCoords = nodes[i];
                        float[] endNodeCoords = nodes[i + 1];

                        if (!FindNode(out ushort startNetNodeId, startNodeCoords))
                        {
                            Vector3 startNodePos = new Vector3(startNodeCoords[0], 0, startNodeCoords[1]);
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
                            Vector3 endNodePos = new Vector3(endNodeCoords[0], 0, endNodeCoords[1]);
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

                panel.SetMessage("Jan's mod", msg, false);
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.J))
            {
                if (_processed) return;

                _processed = true;

                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                string msg = "";

                BuildingManager bm = BuildingManager.instance;
                BuildingInfo bi;
                TerrainManager tm = TerrainManager.instance;
                SimulationManager sm = SimulationManager.instance;

                // set max coords to avoid building objects out of the game area 
                // out of the box game area is 5x5 tiles where 1 tile is 1920 meters (absolute game area is 9x9)
                ushort coordMax = 4800;

                if (!File.Exists("Files/import_export.conf"))
                {
                    panel.SetMessage("Jan's mod", "no conf file provided!", false);
                    return;
                }

                StreamReader confSr = File.OpenText("Files/import_export.conf");
                Dictionary<string, string> conf = new Dictionary<string, string>();
                while (!confSr.EndOfStream)
                {
                    string[] keyVal = confSr.ReadLine().Split(':');
                    if (keyVal.Length == 2)
                        conf.Add(keyVal[0], keyVal[1]);
                }

                double centerLat = double.Parse(conf["CenterLatitude"]);
                double centerLon = double.Parse(conf["CenterLongitude"]);

                WGS84_UTM convertor = new WGS84_UTM(null);
                UTMResult centerUTM = convertor.convertLatLngToUtm(centerLat, centerLon);

                if (!File.Exists("Files/buildings_cs.csv"))
                {
                    panel.SetMessage("Jan's mod", "file doesn't exist!", false);
                    return;
                }

                Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                StreamReader sr = File.OpenText("Files/buildings_cs.csv");

                string[] fields;
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    fields = CSVParser.Split(sr.ReadLine());

                    var centroid = fields[4];
                    if (!centroid.Contains("POINT"))
                        continue;

                    var bldId = ulong.Parse(fields[0]);
                    var bldType = fields[1];
                    int bldLvl = 0;
                    if (fields[2].Length != 0)
                        bldLvl = int.Parse(fields[2]);

                    centroid = centroid.Replace("POINT (", "");
                    centroid = centroid.Replace(")", "");
                    if (centroid.Contains("\""))
                        centroid = centroid.Replace("\"", "");

                    string[] separatingChars = { " " };
                    string[] nodeCoords_v = centroid.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                    var lat = double.Parse(nodeCoords_v[1].Trim());
                    var lon = double.Parse(nodeCoords_v[0].Trim());
                    UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);
                    float xCoord = (float)(utmCoords.Easting - centerUTM.Easting);
                    float zCoord = (float)(utmCoords.Northing - centerUTM.Northing);
                    if (Math.Abs(xCoord) < coordMax && Math.Abs(zCoord) < coordMax)
                    {
                        float[] centroidCoords = new float[] { xCoord, zCoord };
                        Building csBld = new Building(bldId, bldType, bldLvl, centroidCoords);
                        buildings.Add(csBld);
                    }
                }

                sr.Close();

                //foreach (KeyValuePair<string, List<float[]>> segment in segments)
                foreach (var bld in buildings)
                {
                    Vector3 bldPos = new Vector3(bld.bldCentroid[0], 0, bld.bldCentroid[1]);
                    float yCoord = tm.SampleRawHeightSmoothWithWater(bldPos, false, 0f);
                    bldPos.y = yCoord;
                    string building_type = bld.bldType;
                    int bldLvl = bld.bldLvl;

                    // for now allow only certain road types
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

                    SimulationManager.instance.AddAction(AddBuilding(bi, bldPos, 0f));
                }

                panel.SetMessage("Jan's mod", msg, false);

            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.T))
            {
                if (_processed) return;

                _processed = true;


            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            //base.OnUpdate(realTimeDelta, simulationTimeDelta);
        }

        private IEnumerator AddTree(Vector3 treePos, Randomizer rr, TreeInfo treeI)
        {
            uint treeNum;
            TreeManager tree_manager = TreeManager.instance;
            try
            {
                tree_manager.CreateTree(out treeNum, ref rr, treeI, treePos, false);
            }
            catch (Exception ex)
            {
                //try-catch just to prevent crashing by ignoring invalid trees and letting valid trees get created
                //RaiseTreeMapperEvent (ex.Message);
            }
            yield return null;
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
                    tmpSegId = segmentId;
                    Debug.Log("New segment ID: " + segmentId.ToString() + " and name: " + net_manager.GetSegmentName(segmentId));
                    net_manager.SetSegmentNameImpl(segmentId, street_name);
                    Debug.Log("New segment ID: " + segmentId.ToString() + " and name: " + net_manager.GetSegmentName(segmentId));
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

                if (bm.CreateBuilding(out ushort bldId, ref SimulationManager.instance.m_randomizer, bi, bldPos, angle, bi.GetLength(), Singleton<SimulationManager>.instance.m_currentBuildIndex))
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
    }
}
