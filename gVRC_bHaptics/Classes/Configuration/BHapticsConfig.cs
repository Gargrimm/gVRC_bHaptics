using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gVRC_bHaptics.Classes.Configuration
{
    public class BHapticsConfig
    {
        public Deviceconfig Vest = new Deviceconfig();
        public Deviceconfig Head = new Deviceconfig();
        public bool DisableWhenAFK { get; set; } = false;

        public class Deviceconfig
        {
            public bool Enabled { get; set; } = true;
            public int Mult { get; set; } = 100;
        }
    }
}
