﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Threading.Tasks;
using OsmSharp.Streams;
using System.IO;
using System.Net.Http;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Geo;
//using Sample.GeometryStream.Staging;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Numerics;
using burningmime.curves;

namespace OSMtest
{
    class Program
    {
        // Wheatland
        //const string basePath = @"h:\Documents\from_school_pc\school\thesis\1praxe\Cities_skyline\Svit\";
        //const string fileName = "map.osm";
        //const double centerLon = 20.1928136554851;
        //const double centerLat = 49.0555601470384;

        //Olomouc
        //const string basePath = @"h:\Documents\from_school_pc\school\thesis\1praxe\Cities_skyline\Olomouc_17km\";
        //const string fileName = "map";
        //const double centerLon = 17.2478128875718;
        //const double centerLat = 49.5887050728419;

        //Brussels
        const string basePath = @"c:\data\CS\Brusel\";
        const string fileName = "map";
        const double centerLon = 4.372754;
        const double centerLat = 50.849591;

        //Mikulasovice
        //const string basePath = @"h:\Documents\from_school_pc\school\thesis\1praxe\Cities_skyline\Mikulasovice\";
        //const string fileName = "map.osm";
        //const double centerLon = 14.3605685;
        //const double centerLat = 50.9662515;

        //Svit
        //const string basePath = @"h:\Documents\from_school_pc\school\thesis\1praxe\Cities_skyline\Svit_17km\";
        //const string fileName = "map.osm";
        //const double centerLon = 20.1857094521613;
        //const double centerLat = 49.063148018262;

        static void Main(string[] args)
        {

            //RoadOSM();

            //BldOSM();

            //WaterOSM();

            //WaterWayOSM();    // this produces only riverbanks, use QGIS to produce waterway_rwo.csv instead        

            //AmenityOSM();

            //RailOSM();

            // ****** just test methods below ***

            //BridgeOSM();

            //WriteFile();
            //ReadFile();
            //ReadFileRegex();
            //LoadConf();
            //Taranaki();

            //GameArea();

            //Buildings();

            //ExceptDicts();
            //TestParse();
            //TestBitmap();
            //TestRandom();

            //PointF[] polygon = new PointF[]
            //{
            //    new PointF(-10,-10),
            //    new PointF(10,-10),
            //    new PointF(0,10),
            //    new PointF(-10,-10)
            //};
            //var point = new PointF(-2, -9);
            //Console.WriteLine(IsPointInPolygon(polygon, point));
            //point = new PointF(20, -9);
            //Console.WriteLine(IsPointInPolygon(polygon, point));
            //point = new PointF(3, -2);
            //Console.WriteLine(IsPointInPolygon(polygon, point));
            //Console.ReadLine();


            //string[] wkts = new string[]
            //    {
            //    "POLYGON ((20.189872741699219 49.065097808837891, 20.189760208129883 49.0654182434082, 20.189519882202148 49.065486907958984, 20.188083648681641 49.0657844543457, 20.186750411987305 49.0662841796875, 20.186464309692383 49.066234588623047, 20.186180114746094 49.065910339355469, 20.185770034790039 49.065792083740234, 20.185176849365234 49.064716339111328, 20.18773078918457 49.064865112304688, 20.189872741699219 49.065097808837891))",
            //    "LINESTRING (20.286819458007812 49.050819396972656, 20.287004470825195 49.050750732421875, 20.28715705871582 49.050678253173828, 20.287284851074219 49.050594329833984, 20.287410736083984 49.05047607421875)",
            //    "LINEARRING (20.294984817504883 49.059276580810547, 20.294889450073242 49.059284210205078, 20.29493522644043 49.059524536132812)",
            //    "POINT (17.359364542604 49.6846752424008)"
            //};

            //foreach (var wkt in wkts)
            //{
            //    string coords = "";
            //    ProvideCoordsString(out coords, wkt);
            //    Console.WriteLine(coords);
            //    Console.WriteLine();
            //}                        
            //Console.ReadLine();

            //TestListInsert();

            //TestTrig();

            TestBufferSeg();

            //TestCurves();

            //WgsBbox();

        }

        static void RoadOSM()
        {
            //string basePath = "c:/Data/Cities_skyline/Olomouc_18km/";
            string inFilePath = basePath + fileName;
            string outFilePath = basePath + "roads_rwo.csv";
            var fs = File.OpenRead(inFilePath);           
            
            //var source = new PBFOsmStreamSource(fs);            
            var source = new XmlOsmStreamSource(fs);            
            var filtered = from osmGeo in source
                           where osmGeo.Type == OsmSharp.OsmGeoType.Node ||
                           (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.ContainsKey("highway")) //, "residential"))// &&
                           //osmGeo.Tags.Contains("highway", "service") && osmGeo.Tags.Contains("highway", "secondary") &&
                           //osmGeo.Tags.Contains("highway", "primary"))
                           select osmGeo;            
            var complete = filtered.ToComplete();            

            var features = filtered.ToFeatureSource();

            // filter out only linestrings.
            var lineStrings = from feature in features
                              where feature.Geometry is LineString
                              select feature;

            //string wholeFileTxt = "";
            List<string> txtLines = new List<string>
            {
                "Id, Road Name, Road Type, One Way, Lanes, Bridge, Geometry"
            };

            //wholeFileTxt += "Id, Road Name, Road Type, Geometry";
            //wholeFileTxt += "\n";
            foreach (var feature in lineStrings)
            {
                // just loop over all
            }

                // build feature collection.
                //var featureCollection = new FeatureCollection();            
            foreach (var feature in lineStrings)
            {
                //Console.WriteLine(feature.Geometry);                
                //featureCollection.Add(feature);                                  

                string geomTxt = "\"" + feature.Geometry.ToString() + "\"";
                string roadName = "";
                if (feature.Attributes.Exists("name"))
                {
                    roadName = feature.Attributes["name"].ToString();
                }
                string roadType = "";
                if (feature.Attributes.Exists("highway"))
                {
                    roadType = feature.Attributes["highway"].ToString();
                }
                string oneWay = "";
                if (feature.Attributes.Exists("oneway"))
                {
                    oneWay = feature.Attributes["oneway"].ToString();
                }
                string lanes = "";
                if (feature.Attributes.Exists("lanes"))
                {
                    lanes = feature.Attributes["lanes"].ToString();
                }
                string bridge = "";
                if (feature.Attributes.Exists("bridge"))
                {
                    bridge = feature.Attributes["bridge"].ToString();
                }
                string id = "";
                if (feature.Attributes.Exists("id"))
                {
                    id = feature.Attributes["id"].ToString();
                }

                //wholeFileTxt += string.Format("{0},{1},{2},{3}", feature.Attributes["id"], roadName, roadType, geomTxt);
                //wholeFileTxt += "\n";
                txtLines.Add(string.Format("{0},{1},{2},{3},{4},{5},{6}", id, roadName, roadType, oneWay, lanes, bridge, geomTxt));
            }

            //int i = 0;
            //foreach (var osmGeo in complete)
            //{                
            //    if (osmGeo.Type == OsmGeoType.Way)
            //    {                    
            //        //Console.WriteLine(osmGeo.ToString());                    
            //        //string geomTxt = "\"" + featureCollection[i].Geometry.ToString() + "\"";
            //        string geomTxt = "";
            //        string roadName = "";
            //        if (osmGeo.Tags.ContainsKey("name"))
            //        {
            //            roadName = osmGeo.Tags["name"];
            //        }
            //        string roadType = "";
            //        if (osmGeo.Tags.ContainsKey("highway"))
            //        {
            //            roadType = osmGeo.Tags["highway"];
            //        }

            //        wholeFileTxt += string.Format("{0},{1},{2},{3}", osmGeo.Id, roadName, roadType, geomTxt);
            //        wholeFileTxt += "\n";
            //        i++;
            //        //break;
            //    }
            //}
            //Console.WriteLine(i);

            //File.WriteAllText(@outFilePath, wholeFileTxt);
            StreamWriter outputFile = new StreamWriter(outFilePath, false, new UTF8Encoding(true));
            foreach (var lineTxt in txtLines)
            {
                outputFile.WriteLine(lineTxt);
            }
            outputFile.Close();
            //File.WriteAllLines(outFilePath, txtLines);
            //Console.ReadLine();

        }

        static void BldOSM()
        {
            //string basePath = "c:/Data/Cities_skyline/Olomouc_18km/";
            string inFilePath = basePath + fileName;
            string outFilePath = basePath + "buildings_rwo.csv";
            var fs = File.OpenRead(inFilePath);

            //var source = new PBFOsmStreamSource(fs);            
            var source = new XmlOsmStreamSource(fs);
            var filtered = from osmGeo in source
                           where osmGeo.Type == OsmSharp.OsmGeoType.Node ||
                           (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.ContainsKey("building")) 
                           select osmGeo;
            var complete = filtered.ToComplete();

            var features = filtered.ToFeatureSource();

            // filter out only linestrings.
            var lineStrings = from feature in features
                              where feature.Geometry is LineString
                              select feature;
            
            List<string> txtLines = new List<string>
            {
                //"Id, Buidling Type, Levels, Geometry, Centroid, Boundary, Convex Hull"
                "Id, Buidling Type, Levels, Centroid, Geometry"
            };
                       
            foreach (var feature in lineStrings)
            {            
                string geomTxt = "\"" + feature.Geometry.ToString().Replace("LINEARRING (", "POLYGON ((").Replace(")", "))") + "\"";

                string centroid = "\"" + feature.Geometry.Centroid.ToString() + "\"";
                //string boundary = "\"" + feature.Geometry.Boundary.ToString() + "\"";
                //string convexHull = "\"" + feature.Geometry.ConvexHull().ToString() + "\"";
                //feature.Geometry.Envelope

                string bldType = "";
                if (feature.Attributes.Exists("building"))
                {
                    bldType = feature.Attributes["building"].ToString();
                }
                string bldLvl = "";
                if (feature.Attributes.Exists("building:levels"))
                {
                    bldLvl = feature.Attributes["building:levels"].ToString();
                }
                
                txtLines.Add(string.Format("{0},{1},{2},{3},{4}", feature.Attributes["id"], bldType, bldLvl, centroid, geomTxt));
            }
                        
            StreamWriter outputFile = new StreamWriter(outFilePath, false, new UTF8Encoding(true));
            foreach (var lineTxt in txtLines)
            {
                outputFile.WriteLine(lineTxt);
            }
            outputFile.Close();

        }

        static void WaterOSM()
        {
            //string basePath = "c:/Data/Cities_skyline/Olomouc_18km/";
            string inFilePath = basePath + fileName;
            string outFilePath = basePath + "water_rwo.csv";
            var fs = File.OpenRead(inFilePath);

            //var source = new PBFOsmStreamSource(fs);            
            var source = new XmlOsmStreamSource(fs);
            var filtered = from osmGeo in source
                           where osmGeo.Type == OsmSharp.OsmGeoType.Node ||
                           (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.Contains("natural", "water"))
                           select osmGeo;
            var complete = filtered.ToComplete();

            var features = filtered.ToFeatureSource();

            // filter out only linestrings.
            var lineStrings = from feature in features
                              where feature.Geometry is LineString
                              select feature;

            List<string> txtLines = new List<string>
            {
                //"Id, Buidling Type, Levels, Geometry, Centroid, Boundary, Convex Hull"
                "Id, Geometry"
            };

            foreach (var feature in lineStrings)
            {                
                string geomTxt = "\"" + feature.Geometry.ToString().Replace("LINEARRING (", "POLYGON ((").Replace(")", "))") + "\"";

                //string centroid = feature.Geometry.Centroid.ToString();
                //string boundary = "\"" + feature.Geometry.Boundary.ToString() + "\"";
                //string convexHull = "\"" + feature.Geometry.ConvexHull().ToString() + "\"";
                //feature.Geometry.Envelope

                txtLines.Add(string.Format("{0},{1}", feature.Attributes["id"], geomTxt));
            }

            StreamWriter outputFile = new StreamWriter(outFilePath, false, new UTF8Encoding(true));
            foreach (var lineTxt in txtLines)
            {
                outputFile.WriteLine(lineTxt);
            }
            outputFile.Close();

        }

        static void WaterWayOSM()
        {
            string inFilePath = basePath + fileName;
            string outFilePath = basePath + "waterway_rwo.csv";
            var fs = File.OpenRead(inFilePath);
           
            var source = new XmlOsmStreamSource(fs);
            var filtered = from osmGeo in source
                           where osmGeo.Type == OsmSharp.OsmGeoType.Node ||
                           (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.ContainsKey("waterway")) //&&
                           //osmGeo.Tags.Contains("waterway", "river")) 
                           select osmGeo;
            var complete = filtered.ToComplete();

            var features = filtered.ToFeatureSource();

            // filter out only linestrings.
            var lineStrings = from feature in features
                              where feature.Geometry is LineString
                              select feature;

            List<string> txtLines = new List<string>
            {
                "Id, Waterway, Geometry"
            };
            
            foreach (var feature in lineStrings)
            {                              
                string geomTxt = "\"" + feature.Geometry.ToString() + "\"";
                string waterWay = "";
                if (feature.Attributes.Exists("waterway"))
                {
                    waterWay = feature.Attributes["waterway"].ToString();
                }

                txtLines.Add(string.Format("{0},{1},{2}", feature.Attributes["id"], waterWay, geomTxt));
            }

            StreamWriter outputFile = new StreamWriter(outFilePath, false, new UTF8Encoding(true));
            foreach (var lineTxt in txtLines)
            {
                outputFile.WriteLine(lineTxt);
            }
            outputFile.Close();
        }

        static void BridgeOSM()
        {
            //string basePath = "c:/Data/Cities_skyline/Olomouc_18km/";
            string inFilePath = basePath + fileName;
            string outFilePath = basePath + "bridge_rwo.csv";
            var fs = File.OpenRead(inFilePath);

            //var source = new PBFOsmStreamSource(fs);            
            var source = new XmlOsmStreamSource(fs);
            var filtered = from osmGeo in source
                           where osmGeo.Type == OsmSharp.OsmGeoType.Node ||
                           (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.ContainsKey("highway"))
                           select osmGeo;
            var complete = filtered.ToComplete();

            var features = filtered.ToFeatureSource();

            // filter out only linestrings.
            var lineStrings = from feature in features
                              where feature.Geometry is LineString
                              select feature;

            List<string> txtLines = new List<string>
            {
                //"Id, Buidling Type, Levels, Geometry, Centroid, Boundary, Convex Hull"
                "Id, Geometry"
            };

            foreach (var feature in lineStrings)
            {
                var tmp = feature.Attributes;
                var tmp2 = tmp.GetNames();
                if (tmp2.Contains("layer"))
                {
                    Console.WriteLine(string.Join(", ", tmp2));
                    var tmp3 = tmp.GetValues();
                    Console.WriteLine(string.Join(", ", tmp3));
                }
                
                
                //break;

                //string geomTxt = "\"" + feature.Geometry.ToString().Replace("LINEARRING (", "POLYGON ((").Replace(")", "))") + "\"";

                //string centroid = feature.Geometry.Centroid.ToString();
                //string boundary = "\"" + feature.Geometry.Boundary.ToString() + "\"";
                //string convexHull = "\"" + feature.Geometry.ConvexHull().ToString() + "\"";
                //feature.Geometry.Envelope

                //txtLines.Add(string.Format("{0},{1}", feature.Attributes["id"], geomTxt));
            }
            Console.ReadLine();

            //StreamWriter outputFile = new StreamWriter(outFilePath, false, new UTF8Encoding(true));
            //foreach (var lineTxt in txtLines)
            //{
            //    outputFile.WriteLine(lineTxt);
            //}
            //outputFile.Close();

        }

        static void AmenityOSM()
        {
            //string basePath = "c:/Data/Cities_skyline/Olomouc_18km/";
            string inFilePath = basePath + fileName;
            string outFilePath = basePath + "amenity_rwo.csv";
            var fs = File.OpenRead(inFilePath);

            //var source = new PBFOsmStreamSource(fs);            
            var source = new XmlOsmStreamSource(fs);
            var filtered = from osmGeo in source
                           where osmGeo.Type == OsmSharp.OsmGeoType.Node ||
                           (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.ContainsKey("amenity"))
                           select osmGeo;
            var complete = filtered.ToComplete();

            var features = filtered.ToFeatureSource();

            // filter out only linestrings.
            var lineStrings = from feature in features
                              where feature.Geometry is LineString
                              select feature;

            List<string> txtLines = new List<string>
            {
                //"Id, Buidling Type, Levels, Geometry, Centroid, Boundary, Convex Hull"
                "Id, Amenity, Centroid, Geometry"
            };

            foreach (var feature in lineStrings)
            {                
                string geomTxt = "\"" + feature.Geometry.ToString() + "\"";
                string centroid = "\"" + feature.Geometry.Centroid.ToString() + "\"";
                string amenityType = "";
                if (feature.Attributes.Exists("amenity"))
                {
                    amenityType = feature.Attributes["amenity"].ToString();
                }
                txtLines.Add(string.Format("{0},{1},{2},{3}", feature.Attributes["id"], amenityType, centroid, geomTxt));
            }            

            StreamWriter outputFile = new StreamWriter(outFilePath, false, new UTF8Encoding(true));
            foreach (var lineTxt in txtLines)
            {
                outputFile.WriteLine(lineTxt);
            }
            outputFile.Close();

        }

        static void RailOSM()
        {
            //string basePath = "c:/Data/Cities_skyline/Olomouc_18km/";
            string inFilePath = basePath + fileName;
            string outFilePath = basePath + "rails_rwo.csv";
            var fs = File.OpenRead(inFilePath);

            //var source = new PBFOsmStreamSource(fs);            
            var source = new XmlOsmStreamSource(fs);
            var filtered = from osmGeo in source
                           where osmGeo.Type == OsmSharp.OsmGeoType.Node ||
                           (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.ContainsKey("railway"))
                           select osmGeo;
            var complete = filtered.ToComplete();

            var features = filtered.ToFeatureSource();

            // filter out only linestrings.
            var lineStrings = from feature in features
                              where feature.Geometry is LineString
                              select feature;

            List<string> txtLines = new List<string>
            {
                "Id, Rail Type, Bridge, Geometry"
            };

            foreach (var feature in lineStrings)
            {                               
                string geomTxt = "\"" + feature.Geometry.ToString() + "\"";
                string railType = "";
                if (feature.Attributes.Exists("railway"))
                {
                    railType = feature.Attributes["railway"].ToString();
                }
                string bridge = "";
                if (feature.Attributes.Exists("bridge"))
                {
                    bridge = feature.Attributes["bridge"].ToString();
                }

                txtLines.Add(string.Format("{0},{1},{2},{3}", feature.Attributes["id"], railType, bridge, geomTxt));
            }

            StreamWriter outputFile = new StreamWriter(outFilePath, false, new UTF8Encoding(true));
            foreach (var lineTxt in txtLines)
            {
                outputFile.WriteLine(lineTxt);
            }
            outputFile.Close();

        }

        static void ReadFile()
        {
            //Center lon: -93.4016845 Center lat: 37.944249
            
            WGS84_UTM convertor = new WGS84_UTM(null);
            UTMResult centerUTM = convertor.convertLatLngToUtm(centerLat, centerLon);

            string inFilePath = basePath + fileName;
            StreamReader sr = File.OpenText(inFilePath);
            sr.ReadLine();
            TextFieldParser parser = new TextFieldParser(sr);

            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters(",");

            string[] fields;

            while (!parser.EndOfData)
            {
                fields = parser.ReadFields();                
                var coords = fields[3].Replace("LINESTRING (", "");
                coords = coords.Replace(")", "");
                string[] coords_v = coords.Split(',');

                foreach (var nodeCoords in coords_v)
                {
                    //Console.WriteLine(nodeCoords);                    
                    string[] separatingChars = {" "};
                    string[] nodeCoords_v = nodeCoords.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
                    //Console.WriteLine(nodeCoords_v.Length);
                    //foreach (var coord in nodeCoords_v)
                    //{
                    //    Console.WriteLine(coord);
                    //}
                    //for (var i=0; i < nodeCoords_v.Length; i++)
                    //{
                    //    Console.WriteLine(i.ToString() + " " + nodeCoords_v[i].ToString());
                    //}
                    var lonTxt = nodeCoords_v[0].Trim();
                    var latTxt = nodeCoords_v[1].Trim();

                    var lat = double.Parse(latTxt);
                    var lon = double.Parse(lonTxt);
                    UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);

                    //Vector3 pos = new Vector3((float)(utmCoords.Easting - originUtmCoords.Easting), 0, (float)(utmCoords.Northing - originUtmCoords.Northing));



                    Console.WriteLine("In game coords>> x: " + (utmCoords.Easting - centerUTM.Easting).ToString() + ", z: " + (utmCoords.Northing - centerUTM.Northing).ToString());                    

                }
                Console.WriteLine();

                //foreach (string field in fields)
                //{
                //    Console.WriteLine(field);
                //}

                break;
            }

            parser.Close();

            //string read = null;
            //while ((read = sr.ReadLine()) != null)
            //{
            //    var split_v = read.Split(',');
            //    Console.WriteLine(split_v.Length);
            //}

            sr.Close();
            Console.ReadLine();
        }

        static void ReadFileRegex()
        {
            //Center lon: -93.4016845 Center lat: 37.944249
            
            WGS84_UTM convertor = new WGS84_UTM(null);
            UTMResult centerUTM = convertor.convertLatLngToUtm(centerLat, centerLon);

            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string inFilePath = basePath + "roads_cs.csv";
            StreamReader sr = File.OpenText(inFilePath);

            string[] fields;
            int cnt = 0;
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                cnt++;                
                fields = CSVParser.Split(sr.ReadLine());
                
                //Console.WriteLine(string.Join(",", fields));
                //foreach (var field in fields)
                //{                    
                //    if (field.Length > 15)
                //        Console.WriteLine(field.Remove(15));
                //    else
                //        Console.WriteLine(field);
                //}
                var coords = fields[5];
                if (!coords.Contains("LINESTRING"))
                {
                    Console.WriteLine(coords);
                    continue;
                }
                    
                coords = coords.Replace("LINESTRING (", "");
                coords = coords.Replace(")", "");
                if (coords.Contains("\""))
                    coords = coords.Replace("\"", "");
                string[] coords_v = coords.Split(',');

                var streetName = fields[1];
                var roadId = ulong.Parse(fields[0]);
                var roadType = fields[2];
                string oneWay;
                if (fields[3].Length == 0)
                    oneWay = "no";
                else
                    oneWay = "yes";
                int lanes;
                if (fields[4].Length == 0)
                    lanes = 1;
                else
                    lanes = int.Parse(fields[4]);

                //Console.WriteLine("ID: " + roadId.ToString());
                //Console.WriteLine("Road Name: " + streetName);
                //Console.WriteLine("Road Type: " + roadType);
                //Console.WriteLine("OneWay: " + oneWay);
                //Console.WriteLine("Lanes: " + lanes.ToString());

                foreach (var nodeCoords in coords_v)
                {
                    //Console.WriteLine(nodeCoords);                    
                    string[] separatingChars = { " " };
                    string[] nodeCoords_v = nodeCoords.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                    var lonTxt = nodeCoords_v[0].Trim();
                    var latTxt = nodeCoords_v[1].Trim();

                    var lat = double.Parse(latTxt);
                    var lon = double.Parse(lonTxt);
                    UTMResult utmCoords = convertor.convertLatLngToUtm(lat, lon);

                    //Vector3 pos = new Vector3((float)(utmCoords.Easting - originUtmCoords.Easting), 0, (float)(utmCoords.Northing - originUtmCoords.Northing));

                    Console.WriteLine("In game coords>> x: " + (utmCoords.Easting - centerUTM.Easting).ToString() + ", z: " + (utmCoords.Northing - centerUTM.Northing).ToString());

                }
                Console.WriteLine();

                //if (cnt > 10)
                //    break;
            }

            //parser.Close();

            //string read = null;
            //while ((read = sr.ReadLine()) != null)
            //{
            //    var split_v = read.Split(',');
            //    Console.WriteLine(split_v.Length);
            //}

            sr.Close();
            Console.ReadLine();
        }

        static void LoadConf()
        {
            string filePath = "c:/Program Files (x86)/Steam/steamapps/common/Cities_Skylines/Files/import_export.conf";
            StreamReader confSr = File.OpenText(filePath);

            Dictionary<string, string> conf = new Dictionary<string, string>();
            while (!confSr.EndOfStream)
            {
                string[] keyVal = confSr.ReadLine().Split(':');
                if (keyVal.Length == 2)
                    conf.Add(keyVal[0], keyVal[1]);
            }

            double centerLon = double.Parse(conf["CenterLongitude"]);
            double centerLat = double.Parse(conf["CenterLatitude"]);

            Console.WriteLine("MidLat: " + centerLat.ToString());
            Console.WriteLine("MidLon: " + centerLon.ToString());
            Console.ReadLine();

        }

        static void WriteFile()
        {
            string testTxt = "";
            testTxt += "Id, Road Name, Road Type, Geometry";
            testTxt += "\n";
            testTxt += "18488969, North Clark Street, residential,";
            testTxt += "\"LINESTRING (-93.405502319335938 37.946266174316406, -93.405509948730469 37.945781707763672, -93.405563354492188 37.945068359375, -93.40557861328125 37.944889068603516, -93.405609130859375 37.944011688232422, -93.405609130859375 37.943817138671875, -93.4056625366211 37.943111419677734)\"";
            testTxt += "\n";
            // File.OpenWrite("c:/Data/Cities_skyline/Wheatland/test.csv");

            File.WriteAllText(@"c:/Data/Cities_skyline/Wheatland/test.csv", testTxt);

            //StringWriter csv = new StringWriter();
            //csv.WriteLine(string.Format("{0},{1},{2},{3}", "Id", "Road Name", "Road Type", "Geometry"));
            //csv.WriteLine(string.Format("{0},{1},{2},{3}", item.Data1, "\"" + item.Data2 + "\"", item.Data3, "\"" + item.Data4 + "\""));
            //return File(new System.Text.UTF8Encoding().GetBytes(csv.ToString()), "application/csv", string.Format("{0}{1}", "YourFileName", ".csv"));
        }

        static void Taranaki()
        {
            //taranaki summit
            // lat: -39.296719
            // lon: 174.063385
            double lat = -39.296719;
            double lon = 174.063385;
            WGS84_UTM convertor = new WGS84_UTM(null);
            UTMResult summitUTM = convertor.convertLatLngToUtm(lat, lon);
            Console.WriteLine("summitUTM: " + summitUTM.ToString());
            Console.WriteLine("summmit Easting: " + summitUTM.Easting.ToString());
            Console.WriteLine("summmit Northing: " + summitUTM.Northing.ToString()); 

            // center calculate from used terrain.party bbox
            double centerLat = -39.2464812867486;
            double centerLon = 173.98442252349;
            UTMResult centerUTM = convertor.convertLatLngToUtm(centerLat, centerLon);
            Console.WriteLine("centerUTM: " + centerUTM.ToString());
            Console.WriteLine("center Easting: " + centerUTM.Easting.ToString());
            Console.WriteLine("center Northing: " + centerUTM.Northing.ToString());

            var dist = Math.Sqrt(Math.Pow(summitUTM.Easting - centerUTM.Easting, 2) + Math.Pow(summitUTM.Northing - centerUTM.Northing, 2));
            Console.WriteLine("Dist: " + dist.ToString());

            float xCoord = (float)(summitUTM.Easting - centerUTM.Easting);
            float zCoord = (float)(summitUTM.Northing - centerUTM.Northing);

            Console.WriteLine("xCoord: " + xCoord.ToString());
            Console.WriteLine("zCoord: " + zCoord.ToString());

            Console.ReadLine();

        }

        static void GameArea()
        {
            int xCoord = -8640;
            int zCoord = -8640;
            for (int i = 1; i < 82; i++)
            {                                                           
                Console.WriteLine("xCoord: " + xCoord.ToString());
                Console.WriteLine("zCoord: " + zCoord.ToString());
                Console.WriteLine("--------------------");

                if (i % 9 == 0)
                {
                    xCoord = -8640;
                    zCoord += 1920;
                }
                else
                    xCoord += 1920;
            }

            Console.ReadLine();

        }        

        static void Buildings()
        {            
            string building = "Threeway Intersection, RoundaboutL, RoundaboutS, Road Connection Small, GravelBridgePillar, Oneway Toll Booth Large 01, Oneway Toll Booth Large 01 Sub, Oneway Toll Booth Medium 01, Twoway Toll Booth Large 01, Twoway Toll Booth Large 01 Sub, Twoway Toll Booth Medium 01, Twoway Toll Booth Medium 01 Sub, Agricultural 4x4 Processing 01, Agricultural 4x4 Processing 02, H1 1x1 FarmingFacility03, Farming 4x4 Farm, Farming 4x4 Farm 2, Farming 4x4 Farm 3, Agricultural 3x3 Processing 03, Agricultural 3x3 Processing 04, agricultural_building_05, Farming3x2, Farming2x2, Agricultural 1x1 processing 1, Agricultural 2x2 processing 2, Farming4x4, Farming4x4_02, Farming4x4_03, Agricultural 3x2 processing 2, Farming2x2cornerleft, Farming2x2cornerright, Farming4x4cornerleft, Farming4x4cornerright, Water Pipe Junction, Water Intake, Water Outlet, Water Treatment Plant, Water Tower, Forestry 4x3 Processing, Forestry 3x3 Extractor, Forestry 3x3 Forest, Forestry 4x4 Forest, Forestry 3x3 Processing, Forestry 3x3 Processing 2, Forestry2x2 forest, Forestry1x1 forest, Forestry 2x2, Forestry 1x1, Forestry 4x4 Forest 1, Medical Clinic, Hospital, Medical Center, Crematory, Cemetery, ChirpX Launch Control Center, ChirpX Launch Tower, ChirpX Empty Area 01, ChirpX Empty Area 02, ChirpX Empty Area 03, ChirpX Empty Area 04, ChirpX Empty Area 05, ChirpX Empty Area 06, ChirpX Empty Area 07, ChirpX Empty Area 08, ChirpX Empty Area 09, ChirpX Vehicle Assembly Building, ChirpX Vehicles Parking, Electricity Pole, Nuclear Power Plant, Wind Turbine, Solar Power Plant, Dam Power House, Dam Node Building, Advanced Wind Turbine, Coal Power Plant, Oil Power Plant, Fusion Power Plant, Fire Station, Fire House, H2 3x3 Office06, H2 3x3 Office08, H1 2x3 Office01, H1 2x3 Office07, H1 3x3 Office04, H1 4x3 Office06, H2 1x1 Office01, H2 2x2 Office04, H2 2x2 Office07, H2 4x4 Office06, H2 4x3 Office09, H2 4x3 Office06, H3 3x2 Office01, H3 3x4 Office08, H3 4x4 Office12, H3 4x4 Office07, H3 4x4 Office05, H3 4x4 Office02, H3 4x4 Office01, H3 4x3 Office06, H3 4x3 Office04, H3 4x3 Office03, H3 3x3 Office10, H1 3x4 Office01, H1 4x4 Office06, H2 2x2 Office08, H2 2x3 Office04, H2 2x3 Office07, H2 3x2 Office08, H1 4x4 Office05a, H1 3x4 Office08a, H2 3x4 Office05a, H2 4x3 Office09a, H1 3x2 Office06, H1 1x1 Office, H1 2x2 Office02, H1 2x2 Office03, H1 2x2 Office05, H1 3x4 Office02a, H1 3x4 Office06, H3 1x1 Office01, H3 2x2 Office11, H3 2x3 Office10, H3 3x4 Office11a, H3 4x3 Office02a, Police Headquarters, Police Station, Ore 4x3 Processing, Ore 4x3 Processing02, Ore 3x2 Processing, Ore 3x3 Processing, Ore 4x4 Extractor, Ore 2x2 Extractor, Ore1x1, Ore 1x1 processing, Ore 2x2 Processing, OreCrusher, H1 1x1 Tenement, H5 3x3 Tenement03, H5 2x3 Tenement06, H5 3x3 Tenement05, H5 3x2 Highrise06, H4 4x3 Tenement07, H2 3x3 Tenement01, H1 4x3 Tenement07, H3 4x3 Tenement08, H4 4x4 Tenement07b, H1 3x3 Tenement08, H1 3x2 Tenement01, H1 3x4 Tenement07, H3 2x2 Tenement04, H2 1x1 Tenement01, H2 3x4 Tenement06, H4 2x2 Tenement09, H4 1x1 Tenement06, H5 2x2 tenement03, H3 1x1 Tenement05, H1 4x4 Tenement03, H5 3x2 Tenement06, H1 2x2 Tenement05, H1 2x3 Tenement02, H1 3x2 Tenement04, H1 3x3 Tenement03, H5 3x4 Highrise05, H5 3x4 Tenement09, H5 3x2 Tenement02, H5 4x3 Highrise03, H5 4x3 Highrise04, H5 4x3 Tenement04, H5 4x4 Highrise07, H5 4x4 Highrise08, H1 3x4 Tenement03a, H1 4x3 Tenement02, H1 4x3 Tenement05, H1 4x4 Tenement04a, H4 4x3 tenement08a, H2 2x2 tenement06, H2 2x3 Tenement01 , H2 3x2 Tenement05, H2 3x3 Tenement06, H2 3x4 Tenement01 1a, H2 4x3 Tenement06, H2 4x3 Tenement06a, H2 4x4 Tenement02a, H2 4x4 Tenement05b, H2 4x4 Tenement07a, H3 2x3 tenement02, H3 3x3 Tenement04, H3 3x4 tenement03a, H3 3x4 Tenement08, H3 4x3 Tenement04a, H3 4x4 Tenement05a, H3 4x4 Tenement08b, H4 2x3 tenement04, H4 3x2 tenement07, H4 3x3 Tenement08, H4 3x4 tenement05a, H4 3x4 Tenement07b, H4 4x4 Tenement09a, H5 1x1 highrise_hiden_hightech01, H5 4x3 Highrise01, H5 4x3 Highrise07a, H5 4x4 Highrise02, H5 4x4 Tenement01, H3 3x2 Tenement04a, Abandoned Building 01, Abandoned Factory 01, Boulder 01, Boulder 02, Boulder 03, Boulder 04, Bunker Ruins 01, Rock Area 01, Rock Formation 01, Rock Formation 02, Rock Formation 03, Rock Formation 04, Abandoned Lighthouse 01, Ancient Cemetery 01, Pier Ruins 01, Rock Area 02, Rock Area 03, Rock Area 04, Castle Ruins 01, Ship Wreck 01, Castle Ruins 03, Bunker Ruins 02, Castle Ruins 02, Cave 01, Cave 02, Cave 03, Cliff 01, Cliff 02, Cliff 03, Cliff 04, Cliff 05, Cliff 06, Cliff 07, Cliff 08, Cliff 09, Cliff 10, Cliff 11, Cliff 12, Rock Formation 01 B, Rock Formation 01 C, Rock Formation 02 B, Rock Formation 02 C, Rock Formation 03 B, Rock Formation 03 C, Rock Formation 04 B, Rock Formation 04 C, L3 1x1 Shop, L3 2x2 Shop03, L1 4x3 Shop06a, L3 4x3 Shop13a, L1 3x2 Shop05, L1 1x1 Shop, L1 1x2 Shop04, L1 2x2 Shop02, L1 2x2 Shop03, L1 2x2 Shop04, L1 3x2 Shop01, L1 3x2 Shop03b, L1 3x3 Shop07, L1 3x4 Shop05, L1 4x3 Shop03b 1a, L1 4x3 Shop06, L3 3x3 Shop05, L3 4x4 Shop07a, L1 4x4 Shop08a, L2 1x1 Shop08, L2 1x2 Shop07, L2 2x2 Shop2, L2 3x2 Shop09, L2 3x3 Shop03, L2 3x4 Shop04, L2 4x3 Shop05, L2 4x3 Shop06, L2 4x3 Shop10a, L2 4x4 Shop04, L3 1x2 Shop07, L3 2x2 Shop11, L3 3x2 Shop11, L3 3x2 Shop12, L3 3x3 Shop06, L3 3x4 Shop03, L3 4x3 Shop10, L1 4x3 Shop02a, L3 1x1 Shop07, Elementary School, Hadron Collider, High School, University, L5 3x3 Villa02, L2 2x3 Detached01, L1 4x3 detached04, L1 3x4 detached04, L1 1x1 Detached, L2 1x1 detached01, L2 3x4 Detached02, L3 1x1 Detached, L3 3x4 detached01, L4 1x1 Villa04, L4 2x2 Villa02, L4 2x2 Villa07, L5 4x3 Villa05, L5 3x4 Villa05, L5 3x3 Villa08, L5 3x2 Villa06, L5 2x3 Villa07, L5 2x2 Villa09, L5 2x2 Villa07, L5 1x1 DetachedEF, L4 4x4 Villa08a, L1 3x3 Detached 1a, L2 2x2 Detached01, L2 3x2 Detached01, L2 3x4 Detached04a, L1 2x2 Detached01, L1 2x2 Detached03, L1 2x2 Detached04, L1 2x2 Detached06, L1 2x3 Detached03, L1 2x3 Detached05, L1 3x2 Detached04, L1 3x3 Detached02, L1 3x4 Detached04a, L1 3x4 Detached07a, L1 4x3 Detached05, L1 4x4 Detached02, L1 4x4 Detached06a, L2 2x2 Detached05, L2 2x3 Detached03, L2 2x3 Semi-detachedhouse01, L2 3x3 Detached02, L2 3x4 Detached02a, L2 3x4 Semi-detachedhouse02a, L2 4x3 Detached02, L2 4x3 Detached02a, L2 4x4 Detached04, L3 2x2 Detached04, L3 2x2 Detached05, L3 2x3 Detached02, L3 2x3 Semi-detachedhouse02, L3 3x2 Detached03, L3 3x2 Detached06, L3 3x3 Semi-detachedhouse02, L3 3x4 Detached03, L3 3x4 Semi-detachedhouse03a, L3 4x3 Detached01, L3 4x3 Detached04a, L3 4x4 Detached07, L3 4x4 Semi-detachedhouse03a, L4 2x3 Villa04, L4 3x2 Villa02, L4 3x2 Villa02b, L4 3x2 Villa04, L4 3x3 Villa05, L4 3x4 Villa01, L4 4x3 Villa01, L4 4x4 Villa06a, L5 1x1 Detached, L5 3x3 Villa01, L5 4x2 Villa04, L5 4x3 Villa07, L5 4x4 Villa03, L5 4x4 Villa08a, Oil 3x3 Processing, H1 1x1 Facility01, H1 1x1 Facility02, Oil 4x4 Processing, Oil 4x4 Processing02, Oil 3x2 Processing, Oil 3x3 Extractor, Oil2x2, Oil 1x1 processing, Oil 2x2 Processing, H3 4x3 Shop03, H1 1x1 Shop07, H1 3x4 Shop04, H1 4x3 Shop01, H1 4x3 Shop04a, H2 1x1 Shop, H2 2x2 Shop01, H2 2x3 Shop03, H2 3x2 Shop01, H2 3x3 Shop02, H2 3x3 Shop05, H2 3x4 Shop04a, H2 3x4 Shop05, H2 4x3 Shop02a, H2 4x3 Shop06, H2 4x4 Shop06, H3 1x1 Shop10, H3 1x1 Shop13, H3 2x2 Shop01, H3 2x3 Shop04, H3 3x2 Shop04, H3 3x3 Shop06, H3 3x4 Hotel, H3 4x3 Shop05a, H3 4x4 Shop04, H3 4x4 Shop07a, H3 4x4 Shop11, H1 4x3 Shop02, H1  1x1 Shop01, H1 2x2 Shop01, H1 2x3 Shop01, H1 3x2 Shop03, H1 3x3 Shop01, H1 3x3 Shop04, H1 3x4 Shop02a, H1 4x4 Shop03, Landfill Site, Combustion Plant, Bus Depot, Train Station, Train Connection, Airport, Airplane Connection, Ship Connection, Harbor, Cargo Center, Cargo Harbor, RailwayBridgePillar, RailwayElevatedPillar, Space Elevator, Metro Entrance, Airport Apron, Integrated Metro Station, RailOnewayBridgePillar, RailOnewayElevatedPillar, H2 4x3 Sweatshop01, H1 4x4 Sweatshop02, H1 4x4 Mediumfactory02, H1 4x4 Mediumfactory03, H3 4x4 Mediumfactory06, H1 2x2 Sweatshop03, H1 4x4 Bigfactory01, H3 4x3 Bigfactory06, H2 4x4 Bigfactory02, H1 4x3 Bigfactory05, H2 4x4 Bigfactory07, H1 2x2 Sweatshop06, H1 3x3 Sweatshop07, H2 3x3 Sweatshop04, H1 2x2 Sweatshop05, H2 1x1 Facility03, H2 1x1 Facility04, H3 1x1 Facility05, H3 1x1 Facility06, H2 4x4 Sweatshop04, H2 2x2 sweatshop01, cargoyard, H1 3x3 Sweatshop04, H3 2x2 Bigfactory06, H3 4x4 Bigfactory 04, H3 3x3 Mediumfactory08, Empty Park 12x8, Eden Project, Wildlife Spawn Point, Expensive Park, Expensive Playground, Expensive Plaza, Regular Park, Regular Playground, Regular Plaza, Pedestrian Elevated Pillar, Fishing Island, Fishing Island Sub, Floating Cafe, Floating Cafe Sub, Tropical Garden, Wooden Footbridge Pillar 6, Wooden Footbridge Pillar 12, Wooden Footbridge Pillar 18, Wooden Footbridge Pillar 24, ExpoCenter, Oppression Office, Opera House, Grand Mall, SeaAndSky Scraper, High Interest Tower, Trash Mall, Theater of Wonders, Statue of Shopping, Posh Mall, SeaWorld, Modern Art Museum, Plaza of the Dead, Observatory, ScienceCenter, Cathedral of Plentitude, Business Park, Servicing Services, Transport Tower, City Hall, Court House, Colossal Offices, Fountain of LifeDeath, Friendly Neighborhood, Lazaret Plaza, Library, Official Park, Stadium, Statue of Industry, StatueOfWealth, Winter Market 01, Winter Market 01 Tree Sub, MerryGoRound, bouncer_castle, Basketball Court, Botanical garden, dog-park-fence, JapaneseGarden, Statue of Liberty, Arc de Triomphe, Brandenburg Gate, Eiffel Tower, Grand Central Terminal";
            foreach (var build in building.Split(','))
            {
                Console.WriteLine(build);
            }
            Console.ReadLine();
        }

        public void DirsAndFiles()
        {
            DirectoryInfo dir1 = new DirectoryInfo("c:/Data/Cities_skyline");
            Console.WriteLine("Dir full name is: {0}", dir1.FullName);
            Console.WriteLine("Attributes are: {0}", dir1.Attributes.ToString());

            FileInfo[] files = dir1.GetFiles("*");
            Console.WriteLine("Total number of files in folder: {0}", files.Length);
            foreach (FileInfo f in files)
            {
                Console.WriteLine("Name is: {0}", f.Name);
                Console.WriteLine("Length of the file is: {0}", f.Length);
                Console.WriteLine("Creation time is: {0}", f.CreationTime);
            }
        }

        static void ExceptDicts()
        {
            Dictionary<ushort, ushort> nodes_segments = new Dictionary<ushort, ushort>();
            nodes_segments[1] = 2;
            nodes_segments[2] = 3;
            nodes_segments[3] = 4;

            IEnumerable<ushort> res = nodes_segments.Keys.AsQueryable().Except(nodes_segments.Values);
            ushort startNode = res.First();         

            Console.WriteLine(startNode);
            Console.ReadLine();
        }

        static void TestParse()
        {
            //var id = ulong.Parse("\"62267884\"".Replace("\"", ""));
            int id;
            int.TryParse("1/", out id);
            Console.WriteLine(id);
            Console.ReadLine();
        }

        static void TestBitmap()
        {
            var image = new Bitmap(@"h:\Documents\from_school_pc\school\thesis\1praxe\Cities_skyline\Olomouc_18km\olomouc_trees_raster.png", true);
            Console.WriteLine(image.Height);
            Console.ReadLine();
        }

        static void TestRandom()
        {
            var random = new System.Random();
            //var ranNum = random.NextDouble();
            //Console.WriteLine("random: " + ranNum);
            //var res = (0.5 - ranNum) * 16;
            //Console.WriteLine("treemaper random: " + res);

            //for (int i=0; i < 10; i++)
            //{
            //    var justNext = random.Next();
            //    var nextDouble = random.NextDouble();
            //    Console.WriteLine("justNext: " + justNext + " nextDouble: " + nextDouble + " nextDouble*scale: " + (0.5-nextDouble)*16);
            //}

            var start = new DateTime();

            for (int i = 0; i < 10; i++)
            {
                List<int> randInts = new List<int>();
                while (randInts.Count < 5)
                {
                    var tmp = random.Next(1, 10);
                    if (randInts.Contains(tmp))
                        continue;
                    randInts.Add(tmp);
                }
            }
            var end = new DateTime();
            var time_spent = end - start;
            Console.WriteLine(time_spent.Milliseconds);

            start = new DateTime();
            List<int> randInts2 = new List<int>();
            while (randInts2.Count < 500)
            {
                var tmp = random.Next(1, 1000);
                if (randInts2.Contains(tmp))
                    continue;
                randInts2.Add(tmp);
            }
            end = new DateTime();
            time_spent = end - start;
            Console.WriteLine(time_spent.Milliseconds);

            Console.WriteLine("Set of 5 random numbers 1-10:");
            foreach (var randInt in randInts2)
                Console.Write(randInt+" ");
            Console.ReadLine();
        }

        static bool IsPointInPolygon(PointF[] polygon, PointF point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
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

        static string CreateWkt(LatLng[] listOfPositions)
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

        static void TestListInsert()
        {
            List<int> aList = new List<int>
            {
                1,
                10
            };

            bool justOnce = true;
            for (int i = 0; i < aList.Count-1; i++)
            {
                var el1 = aList[i];
                var el2 = aList[i + 1];
                Console.WriteLine(el1 + " : " + el2);
                if (el2-el1 > 1)
                //if (justOnce)
                {
                    justOnce = false;
                    var el3 = el1 + 1;                    
                    aList.Insert(i+1, el3);
                }
            }
            //foreach (var el in aList)
            //    Console.WriteLine(el);
            Console.ReadLine();
        }

        static void TestTrig()
        {
            float angle = 5.66061f;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            var sin8 = Math.Sin(angle) * 8f;
            Console.WriteLine(angle);
            Console.WriteLine(cos);
            Console.WriteLine(sin);
            Console.WriteLine(sin8);
            Console.ReadLine();
        }

        static void TestDict()
        {
            Dictionary<string, List<uint>> zoneBlockMap = new Dictionary<string, List<uint>>();


        }

        static void TestBufferSeg()
        {
            WGS84_UTM convertor = new WGS84_UTM(null);
            UTMResult centerUTM = convertor.convertLatLngToUtm(centerLat, centerLon);
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string csv_path = @"c:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Files\roads_rwo_small.csv";
            StreamReader sr = File.OpenText(csv_path);
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                string[] fields = CSVParser.Split(sr.ReadLine());

                string coords = "";
                for (int i = 0; i < fields.Length; i++)
                {
                    var columnValue = fields[i].Replace("\"", "");                    
                    if (i == 0)
                        ProvideCoordsString(out coords, columnValue);
                }
                if (coords == "")
                    continue;
                
                List<float[]> segCoords = new List<float[]>();
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
                    segCoords.Add(new float [] { xCoord, zCoord});
                }

                int buffer = 5;
                
                int cnt = 0;
                for (int i = 0; i < fields.Length-1; i+=2)
                {
                    var startNode = segCoords[i];
                    var endNode = segCoords[i + 1];
                    Console.WriteLine("StartX: " + startNode[0] + "; StartY: " + startNode[1]);
                    Console.WriteLine("EndX: " + endNode[0] + "; EndY: " + endNode[1]);

                    var length = Length(startNode, endNode);
                    Console.WriteLine("Length: " + length);
                    
                    List<float[]> bufferPoints = new List<float[]>();

                    Dictionary<int, float[]> startBufferPoints = new Dictionary<int, float[]>();
                    startBufferPoints.Add(1, new float[] { startNode[0] - buffer, startNode[1] + buffer });
                    startBufferPoints.Add(2, new float[] { startNode[0] + buffer, startNode[1] + buffer });
                    startBufferPoints.Add(3, new float[] { startNode[0] + buffer, startNode[1] - buffer });
                    startBufferPoints.Add(4, new float[] { startNode[0] - buffer, startNode[1] - buffer });

                    Dictionary<int, float[]> endBufferPoints = new Dictionary<int, float[]>();
                    endBufferPoints.Add(1, new float[] { endNode[0] - buffer, endNode[1] + buffer });
                    endBufferPoints.Add(2, new float[] { endNode[0] + buffer, endNode[1] + buffer });
                    endBufferPoints.Add(3, new float[] { endNode[0] + buffer, endNode[1] - buffer });
                    endBufferPoints.Add(4, new float[] { endNode[0] - buffer, endNode[1] - buffer });

                    VertexLoop vertLoop = new VertexLoop(4);
                    float shortest = 9999;
                    Dictionary<float, List<int>> distPointIndexes = new Dictionary<float, List<int>>();
                    foreach (KeyValuePair<int, float[]> tmpPoint in startBufferPoints)
                    {
                        float tmpDist = Length(endNode, tmpPoint.Value);
                        if (!distPointIndexes.ContainsKey(tmpDist))
                            distPointIndexes.Add(tmpDist, new List<int>());
                        if (tmpDist <= shortest)
                        {
                            shortest = tmpDist;
                            distPointIndexes[shortest].Add(tmpPoint.Key);
                        }
                    }

                    Console.WriteLine("Shortest: " + shortest);
                    foreach (KeyValuePair<float, List<int>> distInd in distPointIndexes)
                        Console.WriteLine("Dist: " + distInd.Key + " ; Indexes: " + distInd.Value.Count);

                    int startInd = 0;
                    var pointIndexes = distPointIndexes[shortest];
                    if (pointIndexes.Count == 1)
                    {
                        startInd = vertLoop.NextVertex(pointIndexes[0]);
                    }
                    else if (pointIndexes.Count == 2)
                    {
                        int l = 9;
                        int h = 0;
                        foreach (var ind in pointIndexes)
                        {
                            l = Math.Min(l, ind);
                            h = Math.Max(h, ind);
                        }
                        if (h - l > 2)
                            h = l;
                        startInd = vertLoop.NextVertex(h);
                    }

                    shortest = 9999;
                    distPointIndexes = new Dictionary<float, List<int>>();
                    foreach (KeyValuePair<int, float[]> tmpPoint in endBufferPoints)
                    {
                        float tmpDist = Length(startNode, tmpPoint.Value);
                        if (!distPointIndexes.ContainsKey(tmpDist))
                            distPointIndexes.Add(tmpDist, new List<int>());
                        if (tmpDist <= shortest)
                        {
                            shortest = tmpDist;
                            distPointIndexes[shortest].Add(tmpPoint.Key);
                        }
                    }

                    int connectInd = 0;
                    pointIndexes = distPointIndexes[shortest];
                    if (pointIndexes.Count == 1)
                    {
                        connectInd = vertLoop.NextVertex(pointIndexes[0]);
                    }
                    else if (pointIndexes.Count == 2)
                    {
                        int l = 9;
                        int h = 0;
                        foreach (var ind in pointIndexes)
                        {
                            l = Math.Min(l, ind);
                            h = Math.Max(h, ind);
                        }
                        if (h - l > 2)
                            h = l;
                        connectInd = vertLoop.NextVertex(h);
                    }

                    for (int j = 0; j < 4 - pointIndexes.Count; j++)
                    {
                        var tmpInd = startInd;
                        for (int k = 0; k < j; k++)
                            tmpInd = vertLoop.NextVertex(tmpInd);
                        bufferPoints.Add(startBufferPoints[tmpInd]);
                    }

                    for (int j = 0; j < 4 - pointIndexes.Count; j++)
                    {
                        var tmpInd = connectInd;
                        for (int k = 0; k < j; k++)
                            tmpInd = vertLoop.NextVertex(tmpInd);
                        bufferPoints.Add(endBufferPoints[tmpInd]);
                    }

                    bufferPoints.Add(startBufferPoints[startInd]);
                    string msg = "";
                    foreach (var p in bufferPoints)
                    {
                        msg += p[0] + ", " + p[1] + "\n";
                    }

                    Console.WriteLine(msg);
                    Console.WriteLine("---------------");

                    cnt++;
                    if (cnt > 10)
                        break;
                }

            }


         }

        public void TestVectors()
        {

        }

        static float Length(float[] start, float[] end)
        {
            var startX = start[0];
            var startY = start[1];
            var endX = end[0];
            var endY = end[1];

            var diffX = endX - startX;
            var diffY = endY - startY;
            var length = Math.Sqrt((diffX * diffX) + (diffY * diffY));
            return (float)length;
        }

        static void TestCurves()
        {
            string filePath = "c:/data/CS/Olomouc/roads_rwo_small.csv";
            StreamReader sr = File.OpenText(filePath);
            sr.ReadLine();

            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            WGS84_UTM convertor = new WGS84_UTM(null);
            UTMResult centerUTM = convertor.convertLatLngToUtm(centerLat, centerLon);

            int cnt = 0;

            string[] fields;
            while (!sr.EndOfStream)
            {
                cnt++;
                if (cnt > 20)
                    break;

                fields = CSVParser.Split(sr.ReadLine());

                string coords = "";
                string streetName = "";
                ulong roadId = 0;

                ProvideCoordsString(out coords, fields[0].Replace("\"", ""));
                streetName = fields[2].Replace("\"", "");

                roadId = ulong.Parse(fields[1].Replace("\"", ""));

                if (coords == "")
                    continue;

                List<Vector2> segCoords = new List<Vector2>();
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
                    if (Math.Abs(xCoord) < 8640 && Math.Abs(zCoord) < 8640)
                    {
                        segCoords.Add(new Vector2(xCoord, zCoord));
                    }
                }

                List<Vector2> reduced = CurvePreprocess.RdpReduce(segCoords, 2);
                CubicBezier[] curves = CurveFit.Fit(reduced, 8);                

                Console.WriteLine("RoadName: " + streetName + "; RoadID: " + roadId);
                Console.WriteLine("Vertexes: ");
                foreach (var vec in segCoords)
                    Console.Write(vec.ToString() + "; ");
                Console.WriteLine();
                Console.WriteLine("reduced: " + reduced.Count + "; curves: " + curves.Length);
                Console.WriteLine("Reduced: ");
                foreach (var red in reduced)
                    Console.Write(red.ToString() + "; ");
                Console.WriteLine();
                Console.WriteLine("BezCurves: ");
                foreach (var curve in curves)
                    Console.Write(curve.ToString() + "; ");
                Console.WriteLine();
            }
            Console.Read();
        }

        static void WgsBbox()
        {
            WGS84_UTM convertor = new WGS84_UTM(null);
            UTMResult utmMid = convertor.convertLatLngToUtm(centerLat, centerLon);
            Console.WriteLine("Centre LON: " + centerLon);
            Console.WriteLine("Centre LAT: " + centerLat);
            Console.WriteLine();

            var maxLat = centerLat;
            var minLat = centerLat;
            var maxLon = centerLon;
            var minLon = centerLon;

            List<LatLng> edgePoints = new List<LatLng>();

            LatLng point1 = convertor.convertUtmToLatLng(utmMid.Easting + 8640, utmMid.Northing + 8640, utmMid.ZoneNumber, "N");
            edgePoints.Add(point1);
            LatLng point2 = convertor.convertUtmToLatLng(utmMid.Easting + 8640, utmMid.Northing - 8640, utmMid.ZoneNumber, "N");
            edgePoints.Add(point2);
            LatLng point3 = convertor.convertUtmToLatLng(utmMid.Easting - 8640, utmMid.Northing - 8640, utmMid.ZoneNumber, "N");
            edgePoints.Add(point3);
            LatLng point4 = convertor.convertUtmToLatLng(utmMid.Easting - 8640, utmMid.Northing + 8640, utmMid.ZoneNumber, "N");
            edgePoints.Add(point4);

            Console.WriteLine("LON      LAT");
            foreach (var point in edgePoints)
            {
                Console.WriteLine(point.Lng + " " + point.Lat);
                maxLat = Math.Max(maxLat, point.Lat);
                minLat = Math.Min(minLat, point.Lat);
                maxLon = Math.Max(maxLon, point.Lng);
                minLon = Math.Min(minLon, point.Lng);
            }

            Console.WriteLine();
            Console.WriteLine("Min Lon: " + minLon);
            Console.WriteLine("Max Lon: " + maxLon);
            Console.WriteLine("Min Lat: " + minLat);
            Console.WriteLine("Max Lat: " + maxLat);
            Console.WriteLine();

            Console.WriteLine(maxLon + "," + maxLat + "," + minLon + "," + minLat);
            Console.WriteLine();

            var wkt = CreateWkt(new LatLng[] { point1, point2, point3, point4, point1});
            Console.WriteLine(wkt);
            Console.WriteLine();

            Console.ReadLine();

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


    public static class Download
    {
        public static async Task ToFile(string url, string filename)
        {
            if (!File.Exists(filename))
            {
                var client = new HttpClient();
                using (var stream = await client.GetStreamAsync(url))
                using (var outputStream = File.OpenWrite(filename))
                {
                    stream.CopyTo(outputStream);
                }
            }
        }
    }
}
