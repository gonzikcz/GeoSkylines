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

namespace GeoSkylines
{
    public class GeoSkylinesExport
    {
        private Randomizer rand;
        private Dictionary<short, List<GeoSkylinesNode>> nodeMap = new Dictionary<short, List<GeoSkylinesNode>>();

        private WGS84_UTM convertor = new WGS84_UTM(null);
        private UTMResult centerUTM;
        private string ZoneLetter;
        private double centerLat;
        private double centerLon;
        private float exportXMin = -8640;
        private float exportXMax = 8640;
        private float exportYMin = -8640;
        private float exportYMax = 8640;

        private ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
        private bool confloaded;

        private NetManager net_manager = NetManager.instance;
        BuildingManager bld_manager = BuildingManager.instance;

        public GeoSkylinesExport()
        {
            LoadConfiguration();
            
            centerUTM = convertor.convertLatLngToUtm(centerLat, centerLon);

            ZoneLetter = "S";
            if (centerLat > 0)
                ZoneLetter = "N";
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

            foreach (KeyValuePair<string, string> a_conf in conf)
            {
                var val = a_conf.Value;
                var key = a_conf.Key;

                if (key == "CenterLatitude")
                    double.TryParse(val, out centerLat);
                else if (key == "CenterLongitude")
                    double.TryParse(val, out centerLon);
                else if (key == "ExportCoordsBox")
                {
                    var coords = val.Split(',').Select(p => p.Trim()).ToArray();
                    if (coords.Length == 4)
                    {
                        float exportXMin_tmp;
                        float exportYMin_tmp;
                        float exportXMax_tmp;
                        float exportYMax_tmp;
                        float.TryParse(coords[0], out exportXMin_tmp);
                        if (exportXMin_tmp != 0)
                            exportXMin = exportXMin_tmp;

                        float.TryParse(coords[1], out exportYMin_tmp);
                        if (exportYMin_tmp != 0)
                            exportYMin = exportYMin_tmp;

                        float.TryParse(coords[2], out exportXMax_tmp);
                        if (exportXMax_tmp != 0)
                            exportXMax = exportXMax_tmp;

                        float.TryParse(coords[3], out exportYMax_tmp);
                        if (exportYMax_tmp != 0)
                            exportYMax = exportYMax_tmp;
                    }
                }

            }

            confloaded = true;
        }

        //public bool FindNode(out ushort netNodeId, Vector2 node)
        //{
        //    short xRound = (short)Math.Round(node.x);

        //    if (nodeMap.ContainsKey(xRound))
        //    {
        //        foreach (InputNode sn in nodeMap[xRound])
        //        {
        //            // try to connect with near nodes
        //            var xDiff = Mathf.Abs(sn.position.x - node.x);
        //            var zDiff = Mathf.Abs(sn.position.y - node.y);
        //            if (xDiff < 0.5)
        //            {
        //                if (zDiff < 0.5)
        //                {
        //                    netNodeId = sn.nodeId;
        //                    return true;
        //                }
        //            }
        //            //if (node.nodeCoords[0] == nodeCoords[0])
        //            //    if (node.nodeCoords[1] == nodeCoords[1])
        //            //    {
        //            //        netNodeId = node.nodeId;
        //            //        return true;
        //            //    }
        //        }
        //    }

        //    netNodeId = 0;
        //    return false;
        //}

        public void ExportBuildings()
        {
            if (!confloaded)
                return;

            string columns = "Id,Geometry,Boundary";
            var a_type = typeof(Building);
            var properties = a_type.GetProperties();
            foreach (var prop in properties)
            {
                columns += string.Format(",{0}", prop.Name);
            }
            columns += ",Service,SubService";            

            List<string> txtLines = new List<string>
                {
                    columns
                };
            
            Building[] bldgs = bld_manager.m_buildings.m_buffer;            
            foreach (var a_bldg in bldgs)
            {
                if (a_bldg.m_position.y == 0)
                    continue;

                if (a_bldg.Info is null)
                    continue;

                if (!WithinExportCoords(a_bldg.m_position))
                    continue;

                txtLines.Add(ExportBuilding(a_bldg, ref properties));
            }
            
            StreamWriter outputFile = new StreamWriter("Files/buildings_cs.csv", false, new UTF8Encoding(true));
            foreach (var lineTxt in txtLines)
            {
                outputFile.WriteLine(lineTxt);
            }
            outputFile.Close();

            panel.SetMessage("GeoSkylines", "Buildings export completed. ", false);
        }

        public string ExportBuilding(Building a_bldg, ref System.Reflection.PropertyInfo[] properties)
        {
            LatLng centroidLL = GamePosition2LatLng(a_bldg.m_position);

            var centroidWkt = CreateWkt(new LatLng[] { centroidLL });

            // creating a boundary (courtesy of Cimtographer)
            int width = a_bldg.Width;
            int length = a_bldg.Length;

            Vector3 a = new Vector3(Mathf.Cos(a_bldg.m_angle), 0f, Mathf.Sin(a_bldg.m_angle)) * 8f;
            Vector3 a2 = new Vector3(a.z, 0f, -a.x);
            Vector3 startEndcorner = a_bldg.m_position - (float)width * 0.5f * a - (float)length * 0.5f * a2;
            Vector3[] corners = new Vector3[]
            {
                        startEndcorner,
                        a_bldg.m_position + (float)width * 0.5f * a - (float)length * 0.5f * a2,
                        a_bldg.m_position + (float)width * 0.5f * a + (float)length * 0.5f * a2,
                        a_bldg.m_position - (float)width * 0.5f * a + (float)length * 0.5f * a2,
                        startEndcorner
            };

            LatLng[] cornersLL = new LatLng[corners.Length];
            for (int i = 0; i < corners.Length; i++)
            {
                LatLng a_cornerLL = GamePosition2LatLng(corners[i]);
                cornersLL[i] = a_cornerLL;
            }

            var boundaryWkt = CreateWkt(cornersLL);

            string rowTxt = string.Format("{0},{1},{2}", a_bldg.m_buildIndex, centroidWkt, boundaryWkt);
            foreach (var prop in properties)
            {
                var prop_val = prop.GetValue(a_bldg, null);
                if (prop_val is null)
                    continue;
                if (prop_val.ToString().Contains(","))
                    prop_val = "\"" + prop_val.ToString() + "\"";
                rowTxt += string.Format(",{0}", prop_val);
            }
            rowTxt += string.Format(",{0},{1}", a_bldg.Info.m_class.m_service, a_bldg.Info.m_class.m_subService);
            return rowTxt;
        }

        public void ExportSegments()
        {
            if (!confloaded)
                return;

            string columns = "Id,Name,Geometry";

            var a_type = typeof(NetSegment);
            var properties = a_type.GetProperties();
            foreach (var prop in properties)
            {
                columns = columns + string.Format(",{0}", prop.Name);
            }

            List<string> txtLines = new List<string>
                {
                    columns
                };

            List<string> txtLinesRail = new List<string>
                {
                    columns
                };

            NetSegment[] segments = net_manager.m_segments.m_buffer;
            for (int i = 0; i < segments.Length; i++)
            {
                var a_seg = segments[i];

                if (a_seg.m_startNode == 0 || a_seg.m_endNode == 0)
                    continue;

                string infoTxt = a_seg.Info.ToString();
                if (infoTxt.Contains("Water Pipe"))
                    continue;

                if (!WithinExportCoords(a_seg.m_middlePosition))
                    continue;

                if (infoTxt.Contains("Train Line") || infoTxt.Contains("Train Track"))
                    txtLinesRail.Add(ExportSegment(a_seg, ref i, ref properties));
                else
                    txtLines.Add(ExportSegment(a_seg, ref i, ref properties));
            }

            StreamWriter outputFile = new StreamWriter("Files/roads_cs.csv", false, new UTF8Encoding(true));
            foreach (var lineTxt in txtLines)
            {
                outputFile.WriteLine(lineTxt);
            }
            outputFile.Close();

            if (txtLinesRail.Count > 1)
            {
                StreamWriter outputFileRail = new StreamWriter("Files/rails_cs.csv", false, new UTF8Encoding(true));
                foreach (var lineTxt in txtLinesRail)
                {
                    outputFileRail.WriteLine(lineTxt);
                }
                outputFileRail.Close();
            }

            panel.SetMessage("GeoSkylines", "Segments export completed. ", false);
        }

        public string ExportSegment(NetSegment a_seg, ref int segId, ref System.Reflection.PropertyInfo[] properties)
        {
            var startNodeId = a_seg.m_startNode;
            var endNodeId = a_seg.m_endNode;
            var startDirection = a_seg.m_startDirection;
            var endDirection = a_seg.m_endDirection;

            if ((a_seg.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
            {
                startNodeId = a_seg.m_endNode;
                endNodeId = a_seg.m_startNode;
                startDirection = a_seg.m_endDirection;
                endDirection = a_seg.m_startDirection;
            }

            var road_name = net_manager.GetSegmentName((ushort)segId);
            if (road_name == null || road_name == "")
                road_name = "no name " + segId.ToString();

            var startPos = net_manager.m_nodes.m_buffer[startNodeId].m_position;
            LatLng startRwoLL = GamePosition2LatLng(startPos);

            var endPos = net_manager.m_nodes.m_buffer[endNodeId].m_position;
            LatLng endRwoLL = GamePosition2LatLng(endPos);

            string wkt = "";
            if (Vector3.Angle(startDirection, -endDirection) > 3f)
            {
                Vector3 a = Vector3.zero;
                Vector3 b = Vector3.zero;
                NetSegment.CalculateMiddlePoints(startPos, startDirection, endPos, endDirection, false, false, out a, out b);
                var bezier = new Bezier3(startPos, a, b, endPos);

                var p1 = bezier.Position(0.25f);
                var p1RwoLL = GamePosition2LatLng(p1);
                var p2 = bezier.Position(0.5f);
                var p2RwoLL = GamePosition2LatLng(p2);
                var p3 = bezier.Position(0.5f);
                var p3RwoLL = GamePosition2LatLng(p3);

                LatLng[] listOfPositions = new LatLng[] { startRwoLL, p1RwoLL, p2RwoLL, p3RwoLL, endRwoLL };

                wkt = CreateWkt(listOfPositions);
            }
            else
            {
                wkt = CreateWkt(new LatLng[] { startRwoLL, endRwoLL });
            }

            string rowTxt = string.Format("{0},{1},{2}", segId, road_name, wkt);

            string field_info = "";
            foreach (var prop in properties)
            {
                var prop_val = prop.GetValue(a_seg, null);
                if (prop_val.ToString().Contains(","))
                    prop_val = "\"" + prop_val.ToString() + "\"";
                field_info += string.Format(",{0}", prop_val);
            }
            rowTxt += field_info;
            return rowTxt;
        }

        public void ExportZones()
        {
            if (!confloaded)
                return;

            string columns = "Id,Boundary,Zone";
            //var a_type = typeof(ZoneBlock);
            //var properties = a_type.GetProperties();
            //foreach (var prop in properties)
            //{
            //    columns += string.Format(",{0}", prop.Name);
            //}

            List<string> txtLines = new List<string>
                {
                    columns
                };

            var zm = ZoneManager.instance;
            string debugMsg = "";
            for (int i = 0; i < zm.m_blocks.m_buffer.Length; i++)
            {
                var zoneBlock = zm.m_blocks.m_buffer[i];
                var pos = zoneBlock.m_position;
                if (pos == Vector3.zero)
                    continue;

                if (!WithinExportCoords(pos))
                    continue;

                Dictionary<ItemClass.Zone, ushort> zones_count = new Dictionary<ItemClass.Zone, ushort>();
                //int num = (int)((zoneBlock.m_flags & 65280u) >> 8);
                for (int z = 0; z < zoneBlock.RowCount; z++)
                    for (int x = 0; x < 4; x++)
                    {
                        var zone = zoneBlock.GetZone(x, z);
                        if (!zones_count.ContainsKey(zone))
                            zones_count.Add(zone, 0);
                        zones_count[zone] += 1;
                    }

                ItemClass.Zone zoneMax = ItemClass.Zone.Unzoned;
                ushort zoneMax_cnt = 0;
                foreach (KeyValuePair<ItemClass.Zone, ushort> a_zoneCount in zones_count)
                {
                    if (a_zoneCount.Value > zoneMax_cnt)
                    {
                        zoneMax_cnt = a_zoneCount.Value;
                        zoneMax = a_zoneCount.Key;
                    }
                }

                int width = 4;
                int length = zoneBlock.RowCount;

                Vector3 a = new Vector3(Mathf.Cos(zoneBlock.m_angle), 0f, Mathf.Sin(zoneBlock.m_angle)) * 8f;
                Vector3 a2 = new Vector3(a.z, 0f, -a.x);
                Vector3 startEndcorner = pos - (float)width * 0.5f * a - (float)length * 0.5f * a2;
                Vector3[] corners = new Vector3[]
                {
                        startEndcorner,
                        pos + (float)width * 0.5f * a - (float)length * 0.5f * a2,
                        pos + (float)width * 0.5f * a + (float)length * 0.5f * a2,
                        pos - (float)width * 0.5f * a + (float)length * 0.5f * a2,
                        startEndcorner
                };

                LatLng[] cornersLL = new LatLng[corners.Length];
                for (int j = 0; j < corners.Length; j++)
                {
                    LatLng a_cornerLL = GamePosition2LatLng(corners[j]);
                    cornersLL[j] = a_cornerLL;
                }

                var boundaryWkt = CreateWkt(cornersLL);

                string rowTxt = string.Format("{0},{1},{2}", i, boundaryWkt, zoneMax);

                txtLines.Add(rowTxt);
            }

            StreamWriter outputFile = new StreamWriter("Files/zones_cs.csv", false, new UTF8Encoding(true));
            foreach (var lineTxt in txtLines)
            {
                outputFile.WriteLine(lineTxt);
            }
            outputFile.Close();

            panel.SetMessage("GeoSkylines", "Zones export completed. ", false);

            //Debug.Log(debugMsg);
        }

        // for debug only
        public void ExportZoneBlocks2()
        {
            var zm = ZoneManager.instance;
            string debugMsg = "";
            for (int i = 0; i < zm.m_blocks.m_buffer.Length; i++)
            {
                var zoneBlock = zm.m_blocks.m_buffer[i];
                if (zoneBlock.m_position == Vector3.zero)
                    continue;

                debugMsg += "zoneBlockId: " + i;
                debugMsg += "\n";
                debugMsg += "Distance: " + zoneBlock.Distance + ", RowCount: " + zoneBlock.RowCount;
                debugMsg += "\n";
                debugMsg += "zoneBlock.m_zone1: " + zoneBlock.m_zone1 + " zoneBlock.m_zone2: " + zoneBlock.m_zone2;
                debugMsg += "\n";
                int num = (int)((zoneBlock.m_flags & 65280u) >> 8);
                debugMsg += "weird num: " + num;
                debugMsg += "\n";
                for (int z = 0; z < num; z++)
                    for (int x = 0; x < 4; x++)
                    {
                        var zone = zoneBlock.GetZone(x, z);
                        //SetZone(x, z, z.m_zone)
                        debugMsg += "x: " + x + ", z: " + z + ", zone: " + zone.ToString();
                        debugMsg += "\n";
                    }                
                debugMsg += "\n";
                debugMsg += "\n";


            }
            Debug.Log(debugMsg);
        }

        // for debug only
        public void ExportZoneBlocks3()
        {
            var nm = NetManager.instance;
            //nm.
            for (ushort i = 0; i < nm.m_segments.m_buffer.Length; i++)
            {
                var seg = nm.m_segments.m_buffer[i];
                //var roadAI = seg.Info.GetComponent<RoadAI>();
                //roadAI.Creat
                
                if (seg.m_startNode == 0 || seg.m_endNode == 0)
                    continue;

                var startDirection = seg.m_startDirection;
                var endDirection = seg.m_endDirection;
                if ((seg.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
                {                    
                    startDirection = seg.m_endDirection;
                    endDirection = seg.m_startDirection;
                }
                var startPos = nm.m_nodes.m_buffer[seg.m_startNode].m_position;
                var endPos = nm.m_nodes.m_buffer[seg.m_endNode].m_position;

                //var angle = Vector3.Angle(startDirection, -endDirection);
                var anglePos = Vector3.Angle(startPos, endPos);
                var angleRad = anglePos * Mathf.Deg2Rad;
                var anglePosRev = Vector3.Angle(endPos, startPos);
                var angleRadRev = anglePosRev * Mathf.Deg2Rad;
                var angleDirs = Vector3.Angle(startDirection, endDirection);
                //Vector3.Angle()

                string debugMsg = "";
                debugMsg += "startDirection: " + startDirection;
                debugMsg += "\n";
                debugMsg += "endDirection: " + endDirection;
                debugMsg += "\n";
                debugMsg += "startPos: " + startPos.ToString();
                debugMsg += "\n";
                debugMsg += "endPos: " + endPos.ToString();
                debugMsg += "\n";
                debugMsg += "anglePos: " + anglePos;
                debugMsg += "\n";
                debugMsg += "angleRad: " + angleRad;
                debugMsg += "\n";
                debugMsg += "anglePosRev: " + anglePosRev;
                debugMsg += "\n";
                debugMsg += "angleRadRev: " + angleRadRev;
                debugMsg += "\n";
                debugMsg += "angleDirs: " + angleDirs;
                debugMsg += "\n";
                debugMsg += "seg.m_cornerAngleEnd: " + seg.m_cornerAngleEnd;
                debugMsg += "\n";
                debugMsg += "seg.m_cornerAngleStart: " + seg.m_cornerAngleStart;
                debugMsg += "\n";
                Debug.Log(debugMsg);
            }

            var zm = ZoneManager.instance;
            foreach (var zoneBlock in zm.m_blocks.m_buffer)
            {
                if (zoneBlock.m_position == Vector3.zero)
                    continue;
                //zoneBlock.SetZone();
                //ItemClass.Zone.
                string debugMsg = "";
                debugMsg += "zoneBlock.m_position: " + zoneBlock.m_position.ToString();
                debugMsg += "\n";
                debugMsg += "zoneBlock.RowCount: " + zoneBlock.RowCount.ToString();
                debugMsg += "\n";
                debugMsg += "zoneBlock.Distance: " + zoneBlock.Distance.ToString();
                debugMsg += "\n";
                debugMsg += "zoneBlock.m_angle: " + zoneBlock.m_angle.ToString();
                debugMsg += "\n";
                debugMsg += "zoneBlock.m_zone1: " + zoneBlock.m_zone1.ToString();
                debugMsg += "\n";
                debugMsg += "zoneBlock.m_zone2: " + zoneBlock.m_zone2.ToString();
                debugMsg += "\n";
                debugMsg += "zoneBlock.GetType(): " + zoneBlock.GetType().ToString();
                debugMsg += "\n";
                

                Debug.Log(debugMsg);
            }
        }

        // attempt to export whole roads (multisegment) but it doesn't work properly - not used
        public void ExportRoads()
        {
            Debug.Log("EXPORT ROADS");

            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            string msg = "";

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
            string ZoneLetter = "S";
            if (centerLat > 0)
                ZoneLetter = "N";

            string columns = "Id,Name,Geometry";

            var a_type = typeof(NetSegment);
            var properties = a_type.GetProperties();
            foreach (var prop in properties)
            {
                columns = columns + string.Format(",{0}", prop.Name);
            }

            List<string> txtLines = new List<string>
                {
                    columns
                };

            NetManager net_manager = NetManager.instance;
            NetSegment[] segments = net_manager.m_segments.m_buffer;
            Dictionary<string, List<NetSegment>> roadsCs = new Dictionary<string, List<NetSegment>>();
            for (int i = 0; i < segments.Length; i++)
            {
                var a_seg = segments[i];
                if (a_seg.Info.ToString().Contains("Highway"))
                    continue;
                var road_name = net_manager.GetSegmentName((ushort)i);
                if (road_name == null || road_name == "")
                    continue; //road_name = "no name " + i.ToString();
                if (!roadsCs.ContainsKey(road_name))
                {
                    roadsCs.Add(road_name, new List<NetSegment>());
                }
                roadsCs[road_name].Add(a_seg);
            }

            int cnt = 0;
            foreach (KeyValuePair<string, List<NetSegment>> road in roadsCs)
            {
                cnt++;
                var road_name = road.Key;
                //string debug_msg = "";
                //debug_msg += "road_name: " + road_name + " | ";
                bool set_field_info = false;
                string field_info = "";
                //Dictionary<ushort, ushort> start_end_nodes = new Dictionary<ushort, ushort>();
                Dictionary<string, HashSet<string>> segments_order = new Dictionary<string, HashSet<string>>();
                List<string> road_ends = new List<string>();
                foreach (var a_seg in road.Value)
                {
                    if (!set_field_info)
                    {
                        foreach (var prop in properties)
                        {
                            var prop_val = prop.GetValue(a_seg, null);
                            if (prop_val.ToString().Contains(","))
                                prop_val = "\"" + prop_val.ToString() + "\"";
                            field_info += string.Format(",{0}", prop_val);
                        }
                        set_field_info = true;
                    }

                    //debug_msg += "a_seg.m_buildIndex: " + a_seg.m_buildIndex.ToString() + " | ";
                    //debug_msg += "a_seg.m_startNode: " + a_seg.m_startNode.ToString() + ", a_seg.m_endNode: " + a_seg.m_endNode.ToString() + "; ";
                    //debug_msg += "\n";

                    var startPos = net_manager.m_nodes.m_buffer[a_seg.m_startNode].m_position;
                    var startRwoX = startPos.x + centerUTM.Easting;
                    var startRwoY = startPos.z + centerUTM.Northing;
                    LatLng startRwoLL = convertor.convertUtmToLatLng(startRwoX, startRwoY, centerUTM.ZoneNumber, ZoneLetter);

                    var endPos = net_manager.m_nodes.m_buffer[a_seg.m_endNode].m_position;
                    var endRwoX = endPos.x + centerUTM.Easting;
                    var endRwoY = endPos.z + centerUTM.Northing;
                    LatLng endRwoLL = convertor.convertUtmToLatLng(startRwoX, startRwoY, centerUTM.ZoneNumber, ZoneLetter);

                    string startLLid = string.Format("{0} {1}", startRwoLL.Lat, startRwoLL.Lng);
                    string endLLid = string.Format("{0} {1}", endRwoLL.Lat, endRwoLL.Lng);

                    if (!segments_order.ContainsKey(startLLid))
                    {
                        segments_order[startLLid] = new HashSet<string>();
                        segments_order[startLLid].Add(startLLid);
                        segments_order[startLLid].Add(endLLid);
                        road_ends.Add(startLLid);
                    }
                    else
                    {
                        segments_order[startLLid].Add(startLLid);
                        segments_order[startLLid].Add(endLLid);
                        road_ends.Remove(startLLid);
                    }

                    if (!segments_order.ContainsKey(endLLid))
                    {
                        segments_order[endLLid] = new HashSet<string>();
                        segments_order[endLLid].Add(startLLid);
                        segments_order[endLLid].Add(endLLid);
                        road_ends.Add(endLLid);
                    }
                    else
                    {
                        segments_order[endLLid].Add(startLLid);
                        segments_order[endLLid].Add(endLLid);
                        road_ends.Remove(endLLid);
                    }

                    //if (start_end_nodes.Keys.Contains(a_seg.m_startNode) || start_end_nodes.Values.Contains(a_seg.m_endNode))
                    //    start_end_nodes[a_seg.m_endNode] = a_seg.m_startNode;
                    //else
                    //    start_end_nodes[a_seg.m_startNode] = a_seg.m_endNode;
                }

                string tmp_msg = "road ends(" + road_ends.Count.ToString() + "): ";
                foreach (var road_end in road_ends)
                    tmp_msg += road_end + " | ";
                tmp_msg += "segments_order (" + segments_order.Count.ToString() + "): ";
                foreach (KeyValuePair<string, HashSet<string>> seg_o in segments_order)
                {
                    tmp_msg += seg_o.Key + ": ";
                    foreach (var llid in seg_o.Value)
                        tmp_msg += llid + " | ";
                    tmp_msg += "\n";
                }


                panel.SetMessage("GeoSkylines", tmp_msg, false);
                return;



                //IEnumerable<ushort> keys_not_values = start_end_nodes.Keys.AsQueryable().Except(start_end_nodes.Values);
                //debug_msg += "Keys not in values: ";
                //foreach (var key in keys_not_values)
                //    Debug.Log(key);
                //foreach (var key in start_end_nodes.Keys)
                //{
                //    if (!start_end_nodes.Values.Contains(key))
                //        debug_msg += key.ToString();
                //}
                //debug_msg += "\n";
                //debug_msg += "Values not in Keys: ";
                //IEnumerable<ushort> values_not_keys = start_end_nodes.Values.AsQueryable().Except(start_end_nodes.Keys);
                //foreach (var val in values_not_keys)
                //    Debug.Log(val);   
                //foreach (var val in start_end_nodes.Values)
                //{
                //    if (!start_end_nodes.Keys.Contains(val))
                //        debug_msg += val.ToString();
                //}
                //debug_msg += "\n";
                //ushort nodeId = res.First();



                //    ushort nodeId = start_end_nodes.Keys.First();
                //    foreach (var key in start_end_nodes.Keys)
                //    {
                //        if (!start_end_nodes.Values.Contains(key))
                //        {
                //            nodeId = key;
                //            break;
                //        }
                //    }
                //    List<LatLng> listOfPositions = new List<LatLng>();
                //    bool end = false;
                //    while (!end)
                //    {
                //        //Debug.Log("Is it going here?");
                //        var pos = net_manager.m_nodes.m_buffer[nodeId].m_position;
                //        var rwoX = pos.x + centerUTM.Easting;
                //        var rwoY = pos.z + centerUTM.Northing;
                //        LatLng rwoLL = convertor.convertUtmToLatLng(rwoX, rwoY, centerUTM.ZoneNumber, ZoneLetter);
                //        listOfPositions.Add(rwoLL);

                //        if (start_end_nodes.ContainsKey(nodeId))
                //            nodeId = start_end_nodes[nodeId];
                //        else
                //            end = true;
                //    }
                //    //Debug.Log("listOfPositions.Count: " + listOfPositions.Count.ToString());
                //    var roadLineWkt = createWkt(listOfPositions.ToArray());

                //    string rowTxt = string.Format("{0},{1},{2},{3}", cnt, road_name, roadLineWkt, field_info);
                //    txtLines.Add(rowTxt);
                //    //Debug.Log(debug_msg);
                //}

                //StreamWriter outputFile = new StreamWriter("Files/roads_cs.csv", false, new UTF8Encoding(true));
                //foreach (var lineTxt in txtLines)
                //{
                //    outputFile.WriteLine(lineTxt);
                //}
                //outputFile.Close();

                //panel.SetMessage("Jan's mod", msg, false);
            }
        }

        static public void DisplayLLOnMouseClick()
        {
            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            if (!File.Exists("Files/import_export.conf"))
            {
                panel.SetMessage("GeoSkylines", "no conf file provided!", false);
                return;
            }            
            string msg = "";

            Vector3 screenMousePos = Input.mousePosition;            
            Ray mouseRay = Camera.main.ScreenPointToRay(screenMousePos);
            var mousePos = GeoSkylinesTool.RaycastMouseLocation(mouseRay);            

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
            string ZoneLetter = "S";
            if (centerLat > 0)
                ZoneLetter = "N";

            var rwoX = mousePos.x + centerUTM.Easting;
            var rwoY = mousePos.y + centerUTM.Northing;
            LatLng rwoLL = convertor.convertUtmToLatLng(rwoX, rwoY, centerUTM.ZoneNumber, ZoneLetter);

            msg += "Screen coordinates (x, y): ";
            msg += screenMousePos.ToString();
            msg += "\n";
            msg += "Game coordinates (x, z): ";
            msg += mousePos.ToString();
            msg += "\n";
            msg += "World coordinates (lon, lat): ";
            msg += rwoLL.Lng.ToString() + "," + rwoLL.Lat.ToString();
            panel.SetMessage("Coordinates", msg, false);
        }

        public void ExportTrees()
        {
            if (!confloaded)
                return;

            string columns = "Id,Geometry";

            var a_type = typeof(TreeInstance);
            var properties = a_type.GetProperties();
            foreach (var prop in properties)
            {
                columns = columns + string.Format(",{0}", prop.Name);
            }

            List<string> txtLines = new List<string>
                {
                    columns
                };

            TreeManager tree_manager = TreeManager.instance;
            TreeInstance[] trees = tree_manager.m_trees.m_buffer;
            int cnt = 0;
            foreach (var a_tree in trees)
            {
                if (a_tree.Position.y == 0)
                    continue;

                if (!WithinExportCoords(a_tree.Position))
                    continue;

                cnt++;

                LatLng rwoLL = GamePosition2LatLng(a_tree.Position);

                LatLng[] listOfPostions = new LatLng[]
                {
                        rwoLL
                };
                var wkt = CreateWkt(listOfPostions);
                string rowTxt = string.Format("{0},{1}", cnt, wkt);
                foreach (var prop in properties)
                {
                    var prop_val = prop.GetValue(a_tree, null);
                    if (prop_val.ToString().Contains(","))
                        prop_val = "\"" + prop_val.ToString() + "\"";
                    rowTxt = rowTxt + string.Format(",{0}", prop_val);
                }

                txtLines.Add(rowTxt);
            }

            StreamWriter outputFile = new StreamWriter("Files/trees_cs.csv", false, new UTF8Encoding(true));
            foreach (var lineTxt in txtLines)
            {
                outputFile.WriteLine(lineTxt);
            }
            outputFile.Close();

            panel.SetMessage("GeoSkylines", "Trees export completed. ", false);
        }

        public bool WithinExportCoords(Vector3 position)
        {
            if (position.x > exportXMin && position.x < exportXMax && position.z > exportYMin && position.z < exportYMax)
                return true;
            else
                return false;
        }

        public LatLng GamePosition2LatLng(Vector3 position)
        {
            var rwoX = position.x + centerUTM.Easting;
            var rwoY = position.z + centerUTM.Northing;
            LatLng rwoLL = convertor.convertUtmToLatLng(rwoX, rwoY, centerUTM.ZoneNumber, ZoneLetter);
            return rwoLL;
        }

        static private string CreateWkt(LatLng[] listOfPositions)
        {
            //POINT(14.362884416061357 50.965573500452379)
            //LINESTRING (-93.370254516601562 37.888759613037109, -93.371223449707031 37.888286590576172, -93.371490478515625 37.888153076171875)
            //LINEARRING (14.361453056335449 50.966289520263672, 14.361678123474121 50.966449737548828, 14.361824989318848 50.96636962890625, 14.361600875854492 50.966209411621094, 14.361453056335449 50.966289520263672)
            string wkt = "";
            //Debug.Log("listOfPositions.Length: " + listOfPositions.Length.ToString());
            if (listOfPositions.Length == 1)
                wkt = string.Format("\"POINT ({0} {1})\"", listOfPositions[0].Lng, listOfPositions[0].Lat);
            else if (listOfPositions.Length > 1)
            {
                string coords = "";
                string sep = "";
                foreach (var pos in listOfPositions)
                {
                    coords += string.Format("{0}{1} {2}", sep, pos.Lng, pos.Lat);
                    sep = ", ";
                }

                var lastIndex = listOfPositions.Length - 1;
                if (listOfPositions[0].Lat == listOfPositions[lastIndex].Lat && listOfPositions[0].Lng == listOfPositions[lastIndex].Lng)
                    wkt = string.Format("\"POLYGON (({0}))\"", coords);
                else
                    wkt = string.Format("\"LINESTRING ({0})\"", coords);
            }

            return wkt;
        }

        public void OutputPrefabInfo()
        {
            string msg = "";
            
            NetSegment[] segments = net_manager.m_segments.m_buffer;
            int segCnt = 0;
            List<ushort> nodes = new List<ushort>();
            for (int i = 0; i < segments.Length; i++)
            {
                msg = "";
                var a_seg = segments[i];

                if (a_seg.m_startNode == 0 || a_seg.m_endNode == 0)
                    continue;
                segCnt++;

                if (!nodes.Contains(a_seg.m_startNode))
                    nodes.Add(a_seg.m_startNode);
                if (!nodes.Contains(a_seg.m_endNode))
                    nodes.Add(a_seg.m_endNode);

            }

            NetNode[] nodes2 = net_manager.m_nodes.m_buffer;
            int nodeCnt = 0;
            for (int i = 0; i < nodes2.Length; i++)
            {
                msg = "";
                var a_node = nodes2[i];

                if ((net_manager.m_nodes.m_buffer[i].m_flags & NetNode.Flags.Created) != NetNode.Flags.None)
                {
                    nodeCnt++;                    
                }                
            }

            msg += "Node Count: \n";
            msg += "m_nodeCount: " + net_manager.m_nodeCount + "; From Segs: " + nodes.Count + "; From iter: " + nodeCnt + "\n";
            msg += "Segment Count: \n"; 
            msg += "m_segmentCount: " + net_manager.m_segmentCount + "; From iter: " + segCnt + "\n";
            msg += "\n";

            msg += "TreeInfo: ";
            msg += "\n";
            msg += "index, Prefab name, Title, Prefab category";
            msg += "\n";
            var prefabCnt = PrefabCollection<TreeInfo>.LoadedCount();
            for (int i = 0; i < prefabCnt; i++)
            {
                var prefab = PrefabCollection<TreeInfo>.GetPrefab((uint)i);
                msg += string.Format("{0}, {1}, {2}, {3}", i, prefab.name, prefab.GetGeneratedTitle(), prefab.category);
                msg += "\n";
            }

            msg += "NetInfo: ";
            msg += "\n";
            msg += "index, Prefab name, Title, Prefab category";
            msg += "\n";
            prefabCnt = PrefabCollection<NetInfo>.LoadedCount();
            for (int i = 0; i < prefabCnt; i++)
            {
                var prefab = PrefabCollection<NetInfo>.GetPrefab((uint)i);
                msg += string.Format("{0}, {1}, {2}, {3}", i, prefab.name, prefab.GetGeneratedTitle(), prefab.category);
                msg += "\n";
            }

            msg += "BuildingInfo: ";
            msg += "\n";
            msg += "index, Prefab name, Title, Prefab category";
            msg += "\n";
            prefabCnt = PrefabCollection<BuildingInfo>.LoadedCount();
            for (int i = 0; i < prefabCnt; i++)
            {
                var prefab = PrefabCollection<BuildingInfo>.GetPrefab((uint)i);
                msg += string.Format("{0}, {1}, {2}, {3}", i, prefab.name, prefab.GetGeneratedTitle(), prefab.category);
                msg += "\n";
            }

            Debug.Log(msg);

            panel.SetMessage("GeoSkylines", "Prefab information written into a output_log.txt in folder c:/Program Files (x86)/Steam/steamapps/common/Cities_Skylines/Cities_Data (or similar) ", false);

        }
    }
}
