using System.Collections.Generic;

namespace gVRC_bHaptics.Classes.Configuration
{
    public class OscProxyInfo
    {
        public bool Enabled { get; set; } = false;
        public string Description { get; set; } = string.Empty;
        public string IP { get; set; }
        public int Port { get; set; }
        public List<string> PatternsRegEx { get; set; } = new List<string>();
        public List<string> PatternsContains { get; set; } = new List<string>();


        public OscProxyInfo() { }
    }
}
