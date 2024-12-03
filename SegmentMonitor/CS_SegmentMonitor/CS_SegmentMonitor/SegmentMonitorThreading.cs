using ICities;
using UnityEngine;

namespace CS_SegmentMonitor
{
    public class SegmentMonitorThreading : ThreadingExtensionBase
    {
        private bool _processed = false;
        private NetManager nm = NetManager.instance;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // ID, Name, Type, Traffic
            if (Input.GetKey(KeyCode.RightAlt) && Input.GetKey(KeyCode.M))
            {
                if (_processed) return;

                _processed = true;
                
                string segLineTxt = "";

                NetSegment[] segments = nm.m_segments.m_buffer;
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
                    
                    segLineTxt += i.ToString();
                    segLineTxt += ",";
                    segLineTxt += nm.GetSegmentName((ushort)i);
                    segLineTxt += ",";
                    segLineTxt += infoTxt;
                    segLineTxt += ",";
                    segLineTxt += a_seg.m_trafficDensity.ToString();
                    segLineTxt += ",";
                    segLineTxt += "\n";                    
                }

                Debug.Log(segLineTxt);
            }
        }
    }
}
