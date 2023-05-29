using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gVRC_bHaptics.Classes.Configuration
{
    public class Logs
    {
        public bool App { get; set; } = true;
        public bool Osc { get; set; } = false;
        public bool HapticsOSC { get; set; } = false;
        public bool HapticsValues { get; set; } = false;
    }
}
