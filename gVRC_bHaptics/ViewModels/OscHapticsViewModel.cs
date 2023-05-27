using gVRC_bHaptics.Classes;
using System.ComponentModel;

namespace gVRC_bHaptics.ViewModels
{
    internal class OscHapticsViewModel
    {
        public ConnectionState VrcOsc { get; set; } = new ConnectionState("VRChat OSC");
        public ConnectionState BhapticsPlayer { get; set; } = new ConnectionState("BhapticsPlayer");
    }
}
