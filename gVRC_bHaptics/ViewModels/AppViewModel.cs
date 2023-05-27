using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gVRC_bHaptics.ViewModels
{
    internal class AppViewModel
    {
        public static string AppName { get; } = "VRC OSC for bHaptics";
        public static string Version { get; } = "v0.1";

        public static string Title { get; } = $"{AppName} {Version}";
    }
}
