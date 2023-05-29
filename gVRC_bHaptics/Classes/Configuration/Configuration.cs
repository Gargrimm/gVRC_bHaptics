using System.Collections.Generic;

namespace gVRC_bHaptics.Classes.Configuration
{
    public class Configuration
    {
        public int VRCPortIn { get; set; } = 9000;
        public int VRCPortOut { get; set; } = 9001;
        public int ProxyPortOut { get; set; } = 9002;
        public List<OscProxyInfo> ProxtyEndPoints { get; set; } = new List<OscProxyInfo>();
        public BHapticsConfig BHaptics { get; set; } = new BHapticsConfig();
        public Logs Logs { get; set; } = new Logs();

        public Configuration() { }
    }
}
