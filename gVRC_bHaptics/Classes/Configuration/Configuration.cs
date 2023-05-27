using System.Collections.Generic;

namespace gVRC_bHaptics.Classes.Configuration
{
    internal class Configuration
    {
        public int VRCPortIn { get; set; } = 9000;
        public int VRCPortOut { get; set; } = 9001;
        public int ProxyPortOut { get; set; } = 9002;
        public List<OscProxyInfo> ProxtyEndPoints { get; set; } = new List<OscProxyInfo>();
        public BHapticsConfig BHaptics { get; set; } = new BHapticsConfig();
        public bool OscLog { get; set; } = false;

        public Configuration() { }
    }
}
