using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.IO;
using System.Collections;
using System.Threading;

namespace GeoSkylines
{
    public class SimpleNode
    {
        public ushort nodeId;
        public float[] nodeCoords;

        public SimpleNode(ushort node, Vector3 pos)
        {
            nodeId = node;
            nodeCoords = new float[2] { pos.x, pos.z };
        }
    }

    public class Road
    {
        public ulong roadId;
        public string roadName;
        public string roadType;
        public string oneWay;
        public int lanes;
        public List<float[]> roadCoords;

        public Road(ulong roadId, string roadName, string roadType, string oneWay, int lanes, List<float[]> roadCoords)
        {
            this.roadId = roadId;
            this.roadName = roadName;
            this.roadType = roadType;
            this.oneWay = oneWay;
            this.lanes = lanes;
            this.roadCoords = roadCoords;
        }
    }
    public class Building
    {
        public ulong bldId;
        public string bldType;
        public int bldLvl;
        public float[] bldCentroid;

        public Building(ulong bldId, string bldType, int bldLvl, float[] bldCentroid)
        {
            this.bldId = bldId;
            this.bldType = bldType;
            this.bldLvl = bldLvl;
            this.bldCentroid = bldCentroid;
        }
    }

    public class GeoSkylinesLoading : ILoadingExtension
    {
        private Randomizer rand;
        private Dictionary<short, List<SimpleNode>> nodeMap = new Dictionary<short, List<SimpleNode>>();  
        private Dictionary<string, List<float[]>> segments = new Dictionary<string, List<float[]>>();

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

        //called when level loading begins
        public void OnCreated(ILoading loading)
        {

        }

        public void OnLevelLoaded(LoadMode mode)
        {

        }

        public void OnLevelUnloading()
        {

        }

        //called when unloading finished
        public void OnReleased()
        {

        }
    }
}
