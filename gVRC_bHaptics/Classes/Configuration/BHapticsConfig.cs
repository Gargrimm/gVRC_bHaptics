using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gVRC_bHaptics.Classes.Configuration
{
    public class BHapticsConfig
    {
        public bool VestEnabled { get; set; } = true;
        public bool HeadEnabled { get; set; } = true;
        public int VestMult { get; set; } = 100;
        public int HeadMult { get; set; } = 100;
        public bool DisableWhenAFK { get; set; } = false;
    }
}
