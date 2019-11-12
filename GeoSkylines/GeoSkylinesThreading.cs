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
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.Mouse0))
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
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.P))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Output Prefab info", "Do you want to output Prefab info? ", (s, r) =>
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
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.S))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Import Services", "You are about to import services. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesImport imp = new GeoSkylinesImport();
                    imp.ImportServices();
                });
            }
            else
            {
                _processed = false;
            }

            // import of railway
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.L))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Import RailWays", "You are about to import railways. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesImport imp = new GeoSkylinesImport();
                    imp.ImportRails();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import of roads
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.R))
            { 
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Import roads", "You are about to import roads. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesImport imp = new GeoSkylinesImport();
                    imp.ImportRoads();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import of standing water basins
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.W))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Dig water reservoirs", "You are about to dig water reservoirs. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesImport imp = new GeoSkylinesImport();
                    imp.ImportWaterBody();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import water way basins
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.Q))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Dig waterways", "You are about to dig waterways. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesImport imp = new GeoSkylinesImport();
                    imp.ImportWaterWay();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import trees from raster
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.T))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Import trees", "You are about to import trees (from raster data). Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesImport imp = new GeoSkylinesImport();
                    imp.ImportTreesRaster();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import trees from vector
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.V))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Import trees", "You are about to import trees (from vector data). Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesImport imp = new GeoSkylinesImport();
                    imp.ImportTreesVector();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // import zones
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.Z))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Import zones", "You are about to set zones (from building data). Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesImport imp = new GeoSkylinesImport();
                    imp.ImportZones();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // export roads
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.G))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Export roads", "You are about to export roads. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesExport exp = new GeoSkylinesExport();
                    exp.ExportSegments();
                });

            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // export buildings
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.H))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Export buildings", "You are about to export buildings. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesExport exp = new GeoSkylinesExport();
                    exp.ExportBuildings();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // export zones
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.J))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Export zones", "You are about to export zones. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesExport exp = new GeoSkylinesExport();
                    exp.ExportZones();
                });
            }
            else
            {
                // not both keys pressed: Reset processed state
                _processed = false;
            }

            // export trees
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.K))
            {
                if (_processed) return;

                _processed = true;

                ConfirmPanel.ShowModal("Export trees", "You are about to export trees. Proceed? ", (s, r) =>
                {
                    if (r != 1)
                        return;
                    GeoSkylinesExport exp = new GeoSkylinesExport();
                    exp.ExportTrees();
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
