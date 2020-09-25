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

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // getting game area coordinates and WGS84 latitude and longitude
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.Mouse0))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesExport.DisplayLLOnMouseClick();                
            }
            else
            {
                _processed = false;
            }

            // output prefab information to a log file (c:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\) 
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.P))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("GeoSkylines: Output Prefab info", "Do you want to output Prefab info? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesExport exp = new GeoSkylinesExport();
                    exp.OutputPrefabInfo();
                });
            }
            else
            {
                _processed = false;
            }

            // Import of services 
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.S))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesImport imp = new GeoSkylinesImport();
                string msg = "Parameters: \n";
                msg += imp.OutputConfiguration("service");
                ConfirmPanel.ShowModal("GeoSkylines: Import Services", msg, (s, r) =>
                {
                    if (r != 1)
                        return;                    
                    imp.ImportServices();
                });
            }
            else
            {
                _processed = false;
            }

            // import of railway
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.L))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesImport imp = new GeoSkylinesImport();
                string msg = "Parameters: \n";
                msg += imp.OutputConfiguration("rail");
                ConfirmPanel.ShowModal("GeoSkylines: Import RailWays", msg, (s, r) =>
                {
                    if (r != 1)
                        return;                    
                    imp.ImportRails();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import of roads
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.R))
            { 
                if (_processed) return;

                _processed = true;

                GeoSkylinesImport imp = new GeoSkylinesImport();
                string msg = "Parameters: \n";
                msg += imp.OutputConfiguration("road");
                ConfirmPanel.ShowModal("GeoSkylines: Import roads", msg, (s, r) =>
                {
                    if (r != 1)
                        return;
                    
                    imp.ImportRoads();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import of standing water basins
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.W))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesImport imp = new GeoSkylinesImport();
                string msg = "Parameters: \n";
                msg += imp.OutputConfiguration("water2");
                ConfirmPanel.ShowModal("GeoSkylines: Dig water reservoirs", msg, (s, r) =>
                {
                    if (r != 1)
                        return;                    
                    imp.ImportWaterReservoirs();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import water way basins
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.Q))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesImport imp = new GeoSkylinesImport();
                string msg = "Parameters: \n";
                msg += imp.OutputConfiguration("water1");
                ConfirmPanel.ShowModal("GeoSkylines: Dig waterways", msg, (s, r) =>
                {
                    if (r != 1)
                        return;                    
                    imp.ImportWaterWay();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import trees from raster
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.T))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesImport imp = new GeoSkylinesImport();
                string msg = "Parameters: \n";
                msg += imp.OutputConfiguration("tree1");
                ConfirmPanel.ShowModal("GeoSkylines: Import trees (raster)", msg, (s, r) =>
                {
                    if (r != 1)
                        return;                    
                    imp.ImportTreesRaster();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import trees from vector
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.V))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesImport imp = new GeoSkylinesImport();
                string msg = "Parameters: \n";
                msg += imp.OutputConfiguration("tree2");
                ConfirmPanel.ShowModal("GeoSkylines: Import trees (vector)", msg, (s, r) =>
                {
                    if (r != 1)
                        return;                    
                    imp.ImportTreesVector();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            //import zones
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.Z))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesImport imp = new GeoSkylinesImport();
                string msg = "Parameters: \n";
                msg += imp.OutputConfiguration("zone");
                ConfirmPanel.ShowModal("GeoSkylines: Import zones", msg, (s, r) =>
                {
                    if (r != 1)
                        return;
                    
                    imp.ImportZonesArea();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // export roads
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.G))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesExport exp = new GeoSkylinesExport();
                string msg = "Parameters: \n";
                msg += exp.OutputConfiguration();

                ConfirmPanel.ShowModal("GeoSkylines: Export roads", msg, (s, r) =>
                {
                    if (r != 1)
                        return;
                    exp.ExportSegments();
                });

            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // export buildings
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.H))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesExport exp = new GeoSkylinesExport();
                string msg = "Parameters: \n";
                msg += exp.OutputConfiguration();

                ConfirmPanel.ShowModal("GeoSkylines: Export buildings", msg, (s, r) =>
                {
                    if (r != 1)
                        return;                    
                    exp.ExportBuildings();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // export zones
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.J))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesExport exp = new GeoSkylinesExport();
                string msg = "Parameters: \n";
                msg += exp.OutputConfiguration();

                ConfirmPanel.ShowModal("GeoSkylines: Export zones", msg, (s, r) =>
                {
                    if (r != 1)
                        return;
                    
                    exp.ExportZones();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // export trees
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.K))
            {
                if (_processed) return;

                _processed = true;

                GeoSkylinesExport exp = new GeoSkylinesExport();
                string msg = "Parameters: \n";
                msg += exp.OutputConfiguration();

                ConfirmPanel.ShowModal("GeoSkylines: Export trees", msg, (s, r) =>
                {
                    if (r != 1)
                        return;
                    
                    exp.ExportTrees();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // node connect
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.F))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("GeoSkylines: Fix segments", "This will remove disconnected segments and then run Update on each segment. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesImport imp = new GeoSkylinesImport();
                    imp.FixSegments();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // node connect
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.M))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("GeoSkylines: Traffic lights", "Switch all intersections to traffic lights? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesImport imp = new GeoSkylinesImport();
                    imp.ImportRoadNames();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // release disconnected segments 
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.N))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("GeoSkylines: Debug Roads", "Debug roads. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesExport exp = new GeoSkylinesExport();
                    //GeoSkylinesImport imp = new GeoSkylinesImport();
                    //imp.DebugRoad();
                    exp.RemoveAllOfSomething("train");
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            //base.OnUpdate(realTimeDelta, simulationTimeDelta);
        }
    }
}
