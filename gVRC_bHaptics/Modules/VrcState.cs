using gVRC_bHaptics.Classes;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gVRC_bHaptics.Modules
{
    internal class VrcState
    {
        private readonly LogProxy Log = LogProxy.GetLogger(typeof(VrcState), () => Common.Instance.Configuration.Logs.App);
        public bool AFK { get; set; }

        public VrcState() {
            Common.Instance.OscManager.MessageReceived += OscManager_MessageReceived;
        }

        private void OscManager_MessageReceived(string address, object value)
        {
            try
            {
                if (address.Equals("/avatar/parameters/AFK", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (bool.TryParse(value?.ToString(), out bool boolVal))
                    {
                        AFK = boolVal;
                        Log.Debug($"AFK => {boolVal}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("VrcState.OscManager_MessageReceived", ex);
            }
        }
    }
}
