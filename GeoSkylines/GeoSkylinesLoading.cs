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
    public class GeoSkylinesNode
    {
        public int NetNodeId = -1;
        public Vector2 position;
        public bool junction = false;
        public List<ulong> roadIds = new List<ulong>();
        public List<string> roadNames = new List<string>();

        public GeoSkylinesNode(Vector2 pos, string roadName, ulong roadId)
        {
            position = pos;
            roadIds.Add(roadId);
            roadNames.Add(roadName);
        }

        public void SetJunction()
        {
            junction = true;
        }
    }

    public class GeoSkylinesRoad
    {
        private TerrainManager tm = TerrainManager.instance;
        public ulong roadId;
        public string roadName;
        public string roadType;
        public string oneWay;
        public int lanes;
        public List<Vector2> vertexes;
        public List<GeoSkylinesSegment> segments = new List<GeoSkylinesSegment>();
        public bool bridge;
        public NetInfo netInfo;

        public GeoSkylinesRoad(ulong roadId, string roadName, string roadType, string oneWay, int lanes, bool bridge, List<Vector2> vertexes, NetInfo ni)
        {
            this.roadId = roadId;            
            this.roadName = roadName;
            this.roadType = roadType;
            this.oneWay = oneWay;
            this.lanes = lanes;
            this.bridge = bridge;
            this.vertexes = vertexes;
            netInfo = ni;

            for (int i = 0; i < vertexes.Count - 1; i++)
            {
                var startNode = vertexes[i];
                var endNode = vertexes[i + 1];
                GeoSkylinesSegment segment = new GeoSkylinesSegment(this, i, startNode, Vector3.zero, Vector3.zero, endNode);
                segments.Add(segment);
            }
        }
    }

    public class GeoSkylinesSegment
    {
        public GeoSkylinesRoad road;
        public string segId;
        public Vector2 startNode;
        public Vector2 endNode;
        public Vector2[] vertexes;
        public Vector2 controlA;
        public Vector2 controlB;
        public float length;
        public Vector2[] buffer;

        public GeoSkylinesSegment(GeoSkylinesRoad road, int numSegRoad, Vector2 startNodePos, Vector2 controlA, Vector2 controlB, Vector2 endNodePos)
        {
            this.road = road;
            segId = road.roadId + "-" + numSegRoad;
            string msg = "";

            this.startNode = startNodePos;
            this.endNode = endNodePos;
            vertexes = new Vector2[] { startNode, endNode};

            int buffer = 10;
            this.controlA = controlA;
            this.controlB = controlB;
            length = VectorUtils.LengthXY(endNodePos - startNodePos);
            List<Vector2> bufferPoints = new List<Vector2>();

            //Dictionary<int, Vector2> startBufferPoints = new Dictionary<int, Vector2>();
            //startBufferPoints.Add(1, new Vector2(startNode.x - buffer, startNode.y + buffer));
            //startBufferPoints.Add(2, new Vector2(startNode.x + buffer, startNode.y + buffer));
            //startBufferPoints.Add(3, new Vector2(startNode.x + buffer, startNode.y - buffer));
            //startBufferPoints.Add(4, new Vector2(startNode.x - buffer, startNode.y - buffer));

            //Dictionary<int, Vector2> endBufferPoints = new Dictionary<int, Vector2>();
            //endBufferPoints.Add(1, new Vector2(endNode.x - buffer, endNode.y + buffer));
            //endBufferPoints.Add(2, new Vector2(endNode.x + buffer, endNode.y + buffer));
            //endBufferPoints.Add(3, new Vector2(endNode.x + buffer, endNode.y - buffer));
            //endBufferPoints.Add(4, new Vector2(endNode.x - buffer, endNode.y - buffer));

            //VertexLoop vertLoop = new VertexLoop(4);
            //float shortest = 9999;
            //Dictionary<float, List<int>> distPointIndexes = new Dictionary<float, List<int>>();
            //foreach (KeyValuePair<int, Vector2> tmpPoint in startBufferPoints)
            //{
            //    float tmpDist = VectorUtils.LengthXZ(endNode - tmpPoint.Value);
            //    if (!distPointIndexes.ContainsKey(tmpDist))
            //        distPointIndexes.Add(tmpDist, new List<int>());
            //    if (tmpDist <= shortest)
            //    {
            //        shortest = tmpDist;
            //        distPointIndexes[shortest].Add(tmpPoint.Key);
            //    }
            //}

            //int startInd = 0;
            //var pointIndexes = distPointIndexes[shortest];
            //if (pointIndexes.Count == 1)
            //{
            //    startInd = vertLoop.NextVertex(pointIndexes[0]);
            //}
            //else if (pointIndexes.Count == 2)
            //{
            //    int l = 9;
            //    int h = 0;
            //    foreach (var ind in pointIndexes)
            //    {
            //        l = Math.Min(l, ind);
            //        h = Math.Max(h, ind);
            //    }
            //    if (h - l > 2)
            //        h = l;
            //    startInd = vertLoop.NextVertex(h);
            //}

            //shortest = 9999;
            //distPointIndexes = new Dictionary<float, List<int>>();
            //foreach (KeyValuePair<int, Vector2> tmpPoint in endBufferPoints)
            //{
            //    float tmpDist = VectorUtils.LengthXZ(startNode - tmpPoint.Value);
            //    if (!distPointIndexes.ContainsKey(tmpDist))
            //        distPointIndexes.Add(tmpDist, new List<int>());
            //    if (tmpDist <= shortest)
            //    {
            //        shortest = tmpDist;
            //        distPointIndexes[shortest].Add(tmpPoint.Key);
            //    }
            //}

            //int connectInd = 0;
            //pointIndexes = distPointIndexes[shortest];
            //if (pointIndexes.Count == 1)
            //{
            //    connectInd = vertLoop.NextVertex(pointIndexes[0]);
            //}
            //else if (pointIndexes.Count == 2)
            //{
            //    int l = 9;
            //    int h = 0;
            //    foreach (var ind in pointIndexes)
            //    {
            //        l = Math.Min(l, ind);
            //        h = Math.Max(h, ind);
            //    }
            //    if (h - l > 2)
            //        h = l;
            //    connectInd = vertLoop.NextVertex(h);
            //}

            //for (int i = 0; i < 4 - pointIndexes.Count; i++)
            //{
            //    var tmpInd = startInd;
            //    for (int k = 0; k < i; k++)
            //        tmpInd = vertLoop.NextVertex(tmpInd);
            //    bufferPoints.Add(startBufferPoints[tmpInd]);
            //}

            //for (int i = 0; i < 4 - pointIndexes.Count; i++)
            //{
            //    var tmpInd = connectInd;
            //    for (int k = 0; k < i; k++)
            //        tmpInd = vertLoop.NextVertex(tmpInd);
            //    bufferPoints.Add(endBufferPoints[tmpInd]);
            //}

            //bufferPoints.Add(startBufferPoints[startInd]);
            this.buffer = bufferPoints.ToArray();
            foreach (var p in bufferPoints)
            {
                msg += p.ToString() + "\n";
            }
            //Debug.Log(msg);
        }
    }

    public class VertexLoop
    {
        public int num_of_vertexes;
        
        public VertexLoop(int num_of_vertexes)
        {
            this.num_of_vertexes = num_of_vertexes;
        }

        public int NextVertex(int vertex)
        {
            int nextVertex = vertex + 1;
            if (nextVertex > 4)
                nextVertex -= 4;
            return nextVertex;
        }
        
        public int PreviousVertex(int vertex)
        {
            int prevVertex = vertex - 1;
            if (prevVertex < 1)
                prevVertex += 4;
            return prevVertex;
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

    public class GeoSkylinesZone
    {
        public ulong zoneId;
        public string zoneType;
        public Vector2[] zoneVertices;
        public int[] zoneBoundingBox;

        public GeoSkylinesZone(ulong zoneId, string zoneType, Vector2[] zoneVertices)
        {
            this.zoneId = zoneId;
            this.zoneType = zoneType;
            this.zoneVertices = zoneVertices;
            //this.zoneBoundingBox = zoneBoundingBox;

            float xMin = 8641;
            float xMax = -8641;
            float yMin = 8641;
            float yMax = -8641;

            foreach (var pos in zoneVertices)
            {
                xMin = Mathf.Min(xMin, pos.x);
                xMax = Mathf.Max(xMax, pos.x);
                yMin = Mathf.Min(yMin, pos.y);
                yMax = Mathf.Max(yMax, pos.y);
            }

            zoneBoundingBox = new int[] { (int)xMin-1, (int)xMax+1, (int)yMin-1, (int)yMax+1 };
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

