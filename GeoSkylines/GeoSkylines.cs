using ICities;
using UnityEngine;

namespace GeoSkylines
{
    public class GeoSkylines : IUserMod
    {
        public string Name => "GeoSkylines";
        public string Description => "Import/export various geodata into Cities: Skylines from/to a simple flat file (CSV)";
    }
}
