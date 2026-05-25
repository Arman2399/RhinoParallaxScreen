using Rhino.PlugIns;
using System;
using System.Runtime.InteropServices;

namespace ParallaxScreenPlugin
{
    // Replace the string below with a newly generated GUID
    [Guid("FB1F442E-8EF2-49E6-95F0-F7286C464595")]
    public class ParallaxScreenPluginClass : PlugIn
    {
        public ParallaxScreenPluginClass()
        {
            Instance = this;
        }

        public static ParallaxScreenPluginClass Instance { get; private set; }
    }
}

