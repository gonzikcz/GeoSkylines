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

    public class GeoSkylinesRoad
    {
        public ulong roadId;
        public string roadName;
        public string roadType;
        public string oneWay;
        public int lanes;
        public List<float[]> roadCoords;
        public bool bridge;

        public GeoSkylinesRoad(ulong roadId, string roadName, string roadType, string oneWay, int lanes, bool bridge, List<float[]> roadCoords)
        {
            this.roadId = roadId;
            this.roadName = roadName;
            this.roadType = roadType;
            this.oneWay = oneWay;
            this.lanes = lanes;
            this.bridge = bridge;
            this.roadCoords = roadCoords;
        }
    }
    public class GeoSkylinesBuilding
    {
        public ulong bldId;
        public string bldType;
        public int bldLvl;
        public float[] bldCentroid;
        public float angle;
        public float width;
        public float height;

        public GeoSkylinesBuilding(ulong bldId, string bldType, int bldLvl, float[] bldCentroid, float angle, float width, float height)
        {
            this.bldId = bldId;
            this.bldType = bldType;
            this.bldLvl = bldLvl;
            this.bldCentroid = bldCentroid;
            this.angle = angle;
            this.width = width;
            this.height = height;
        }
    }

    public class GeoSkylinesService
    {
        public ulong serviceId;
        public string serviceType;
        public float[] serviceCentroid;

        public GeoSkylinesService(ulong serviceId, string serviceType, float[] serviceCentroid)
        {
            this.serviceId = serviceId;
            this.serviceType = serviceType;
            this.serviceCentroid = serviceCentroid;
        }
    }

    public class GeoSkylinesLoading : ILoadingExtension
    {
        //called when level loading begins
        public void OnCreated(ILoading loading)
        {

        }

        public void OnLevelLoaded(LoadMode mode)
        {
            //ConfirmPanel panel = UIView.library.ShowModal<ConfirmPanel>("ConfirmPanel");
            //panel.SetMessage("Build road network?", "Confirm to build road network. ");            
            //LoadingExtension.go.transform.localScale = new Vector3(8640f, 1f, 8640f);
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
