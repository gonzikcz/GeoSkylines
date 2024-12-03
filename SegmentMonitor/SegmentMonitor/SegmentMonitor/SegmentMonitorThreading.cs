using ICities;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SegmentMonitor
{
    public class SegmentMonitorThreading : ThreadingExtensionBase
    {
        private bool _first = true;
        private NetManager nm = NetManager.instance;
        private SimulationManager sm = SimulationManager.instance;
        private System.DateTime _gameTime;
        public StreamWriter outputFile;
        public StreamWriter outputFileRoads;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (_first)
            {
                _gameTime = sm.m_currentGameTime;
                outputFile = new StreamWriter("C:/temp/SegmentMonitor.csv", false, new UTF8Encoding(true));
                outputFile.Write("\"ID\", \"RoadName\", \"RoadType\", \"Traffic%\", \"DateTime\", \"Date\", \"Time\"\n");
                outputFileRoads = new StreamWriter("C:/temp/RoadMonitor.csv", false, new UTF8Encoding(true));
                outputFileRoads.Write("\"RoadName\", \"NumOfSegs\",\"LengthOfSegs\",\"AvgDens\", \"DateTime\", \"Date\", \"Time\"\n");
                _first = false;
            }

            var newGameTime = sm.m_currentGameTime;
            var timeDiff = newGameTime - _gameTime;
            if (timeDiff.TotalMinutes > 15)
            {
                Dictionary<string, List<int>> roadsTraffic = new Dictionary<string, List<int>>();
                string segLineTxt = "";

                var gameTimeTxt = newGameTime.ToString();
                var gameTimeTxt_split = gameTimeTxt.Split(' ');

                NetSegment[] segments = nm.m_segments.m_buffer;

                // first do segments - limit output to density > 40
                for (int i = 0; i < segments.Length; i++)
                {
                    var a_seg = segments[i];

                    if (a_seg.m_startNode == 0 || a_seg.m_endNode == 0)
                        continue;

                    string infoTxt = a_seg.Info.ToString();
                    if (infoTxt.Contains("Water Pipe"))
                        continue;

                    if (infoTxt.Contains("Train Line") || infoTxt.Contains("Train Track"))
                        continue;

                    if (a_seg.m_averageLength < 10)
                        continue;

                    byte segDens = a_seg.m_trafficDensity;
                    if (segDens < 35)
                        continue;

                    string segName = nm.GetSegmentName((ushort)i);

                    segLineTxt += "\"" + i.ToString() + "\"";
                    segLineTxt += ",";
                    segLineTxt += "\"" + segName + "\"";
                    segLineTxt += ",";
                    segLineTxt += "\"" + infoTxt + "\"";
                    segLineTxt += ",";
                    segLineTxt += "\"" + segDens.ToString() + "\"";
                    segLineTxt += ",";
                    segLineTxt += "\"" + gameTimeTxt + "\"";
                    segLineTxt += ",";
                    segLineTxt += "\"" + gameTimeTxt_split[0] + "\"";
                    segLineTxt += ",";
                    segLineTxt += "\"" + gameTimeTxt_split[1] + " " + gameTimeTxt_split[2] + "\"";
                    segLineTxt += "\n";
                }

                outputFile.Write(segLineTxt);

                // now do roads - I need to collect all the segments, then calculate avg dens and then limit to > 40
                for (int i = 0; i < segments.Length; i++)
                {
                    var a_seg = segments[i];

                    if (a_seg.m_startNode == 0 || a_seg.m_endNode == 0)
                        continue;

                    string infoTxt = a_seg.Info.ToString();
                    if (infoTxt.Contains("Water Pipe"))
                        continue;

                    if (infoTxt.Contains("Train Line") || infoTxt.Contains("Train Track"))
                        continue;

                    string segName = nm.GetSegmentName((ushort)i);

                    if (!roadsTraffic.ContainsKey(segName))
                        roadsTraffic.Add(segName, new List<int>());

                    roadsTraffic[segName].Add(i);
                }

                string roadTxt = "";

                foreach (KeyValuePair<string, List<int>> a_road in roadsTraffic)
                {
                    float avgDens = 0;
                    float lenSegs = 0;
                    foreach (var segId in a_road.Value)
                    {
                        var roadSeg = segments[segId];
                        avgDens += roadSeg.m_trafficDensity;
                        lenSegs += roadSeg.m_averageLength;
                    }

                    if (lenSegs < 120)
                        continue;
                    
                    avgDens = avgDens / a_road.Value.Count;
                    if (avgDens < 25)
                        continue;

                    roadTxt += "\"" + a_road.Key + "\"";
                    roadTxt += ",";
                    roadTxt += "\"" + a_road.Value.Count.ToString() + "\"";
                    roadTxt += ",";
                    roadTxt += "\"" + lenSegs + "\"";
                    roadTxt += ",";
                    roadTxt += "\"" + avgDens + "\"";
                    roadTxt += ",";
                    roadTxt += "\"" + gameTimeTxt + "\"";
                    roadTxt += ",";
                    roadTxt += "\"" + gameTimeTxt_split[0] + "\"";
                    roadTxt += ",";
                    roadTxt += "\"" + gameTimeTxt_split[1] + " " + gameTimeTxt_split[2] + "\"";
                    roadTxt += "\n";
                }

                outputFileRoads.Write(roadTxt);


                _gameTime = newGameTime;
            }
        }



        // >>> this is to run it only once for testing purposes <<< 
        //
        //public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        //{
        //    if (Input.GetKey(KeyCode.RightAlt) && Input.GetKey(KeyCode.M))
        //    {

        //        string segLineTxt = "\"ID\", \"Name\", \"Type\", \"Traffic\"\n";
        //        Dictionary<string, List<int>> roadsTraffic = new Dictionary<string, List<int>>();

        //        NetSegment[] segments = nm.m_segments.m_buffer;
        //        for (int i = 0; i < segments.Length; i++)
        //        {
        //            var a_seg = segments[i];

        //            if (a_seg.m_startNode == 0 || a_seg.m_endNode == 0)
        //                continue;

        //            string infoTxt = a_seg.Info.ToString();
        //            if (infoTxt.Contains("Water Pipe"))
        //                continue;

        //            if (infoTxt.Contains("Train Line") || infoTxt.Contains("Train Track"))
        //                continue;

        //            byte segDens = a_seg.m_trafficDensity;
        //            if (segDens < 40)
        //                continue;

        //            string segName = nm.GetSegmentName((ushort)i);

        //            if (!roadsTraffic.ContainsKey(segName))
        //                roadsTraffic.Add(segName, new List<int>());

        //            roadsTraffic[segName].Add(i);

        //            segLineTxt += "\"" + i.ToString() + "\"";
        //            segLineTxt += ",";
        //            segLineTxt += "\"" + segName + "\"";
        //            segLineTxt += ",";
        //            segLineTxt += "\"" + infoTxt + "\"";
        //            segLineTxt += ",";
        //            segLineTxt += "\"" + segDens.ToString() + "\"";
        //            segLineTxt += "\n";
        //        }

        //        StreamWriter outputFile = new StreamWriter("C:/temp/SegmentMonitor.csv", false, new UTF8Encoding(true));
        //        outputFile.Write(segLineTxt);
        //        outputFile.Close();

        //        StreamWriter outputFileRoads = new StreamWriter("C:/temp/RoadMonitor.csv", false, new UTF8Encoding(true));
        //        string roadTxt = "";
        //        roadTxt += "\"Name\", \"NumOfSegs\",\"AvgDens\"\n";

        //        foreach (KeyValuePair<string, List<int>> a_road in roadsTraffic)
        //        {
        //            float avgDens = 0;
        //            foreach (var segId in a_road.Value)
        //            {
        //                var roadSeg = segments[segId];
        //                avgDens += roadSeg.m_trafficDensity;
        //            }
        //            avgDens = avgDens / a_road.Value.Count;

        //            roadTxt += "\"" + a_road.Key + "\"";
        //            roadTxt += ",";
        //            roadTxt += "\"" + a_road.Value.Count.ToString() + "\"";
        //            roadTxt += ",";
        //            roadTxt += "\"" + avgDens + "\"";
        //            roadTxt += "\n";
        //        }

        //        outputFileRoads.Write(roadTxt);
        //        outputFileRoads.Close();

        //        //string timeTxt = "";
        //        //timeTxt += "sm.m_dayTimeFrame: " + sm.m_dayTimeFrame.ToString();
        //        //timeTxt += "\n";
        //        //timeTxt += "sm.FrameToTime(: " + sm.FrameToTime(sm.m_dayTimeFrame).ToString();
        //        //timeTxt += "\n";
        //        //timeTxt += "sm.m_currentGameTime: " + sm.m_currentGameTime.ToString();
        //        //timeTxt += "\n";
        //        //timeTxt += "sm.m_currentDayTimeHour: " + sm.m_currentDayTimeHour.ToString();
        //        //timeTxt += "\n";

        //        //var newGameTime = sm.m_currentGameTime;
        //        //timeTxt += "_gameTime: " + _gameTime.ToString();
        //        //timeTxt += "\n";               
        //        //var timeDiff = newGameTime - _gameTime;
        //        //timeTxt += "timeDiff: " + timeDiff.ToString() + " | " + timeDiff.Minutes.ToString() + " | " + timeDiff.TotalMilliseconds;
        //        //timeTxt += "\n";
        //        //var timeDiff2 = _gameTime - newGameTime;
        //        //timeTxt += "timeDiff: " + timeDiff2.ToString() + " | " + timeDiff2.Minutes.ToString() + " | " + timeDiff2.TotalMilliseconds;
        //        //timeTxt += "\n";

        //        //if (timeDiff.Minutes > 15)
        //        //    timeTxt += "More than 15 minutes \n";

        //        //if (timeDiff.TotalMilliseconds > 900000)
        //        //    timeTxt += "More than 15 minutes (in milisec) \n";

        //        //Debug.Log(timeTxt);
        //    }
        //}
    }
}