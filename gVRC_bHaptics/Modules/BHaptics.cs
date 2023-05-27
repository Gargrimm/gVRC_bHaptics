using Bhaptics.ModUI.Controls;
using Bhaptics.Tact;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;

namespace gVRC_bHaptics.Modules
{
    internal class BHaptics
    {

        public delegate void HapticsConnectionChangedEventHandler(bool connected);
        public event HapticsConnectionChangedEventHandler HapticsConnectionChanged;

        public bool Connected;

        readonly Common Common;

        private IHapticPlayer Player;
        private HapticPlayerManager HapticPlayerManager;

        private static readonly int[] mirroredVestMotors = new int[20]
        {
          3,2,1,0,7,6,5,4,11,10,9,8,15,14,13,12,19,18,17,16
        };

        private static readonly int[] mirroredHeadMotors = new int[6]
        {
          5,4,3,2,1,0
        };

        public BHaptics(Common Common)
        {
            this.Common = Common;
        }

        public void RunThread()
        {
            var delay = 100;

            try
            {
                HapticPlayerManager.SetAppInfo("gVRC_bHaptics", "gVRC_bHaptics");
                this.Player = HapticPlayerManager.Instance().GetHapticPlayer();
                this.HapticPlayerManager = HapticPlayerManager.Instance();
                this.HapticPlayerManager.BhapticsPlayerConnectionChange = (x) => ConnectionChanged(x);
                ConnectionChanged(this.HapticPlayerManager.Connected);

                if (this.Common.OscManager != null)
                {
                    this.Common.OscManager.MessageReceived += OscManager_MessageReceived;
                }

                while (!Common.Instance.Exit)
                {
                    System.Threading.Thread.Sleep(delay);
                }
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine($"(BHaptics.RunThread) Cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"(BHaptics.RunThread) Error: {ex.ToString()}");
            }
        }

        private void OscManager_MessageReceived(string address, object value)
        {
            try
            {
                //Check address
                if (!address.StartsWith("/avatar/parameters/bOSC_v1_", StringComparison.InvariantCultureIgnoreCase)) return;

                //Parse value
                if (!float.TryParse(value?.ToString(), out float floatValue)) return;
                if (floatValue > 1) floatValue = 1;
                if (floatValue < 0) floatValue = 0;

                //Parse address into location
                var locationNameRaw = address.Replace("/avatar/parameters/bOSC_v1_", string.Empty);
                var locationNameParts = locationNameRaw.Split('_');
                if (locationNameParts.Length != 2) return;

                var locationName = locationNameParts[0];
                var position = locationNameParts[1];
                if (!int.TryParse(position, out int positionValue)) return;

                //Set motor values
                PositionType? location = null;
                byte[] bytes = null;
                switch (locationName.ToLower())
                {
                    case "vestfront":
                        if (!Common.Instance.Configuration.BHaptics.VestEnabled) return;
                        location = PositionType.VestFront;
                        bytes = new byte[20];
                        var index = mirroredVestMotors[positionValue];
                        bytes[index] = (byte)MultValue(floatValue, location.Value);
                        break;
                    case "vestback":
                        if (!Common.Instance.Configuration.BHaptics.VestEnabled) return;
                        location = PositionType.VestBack;
                        bytes = new byte[20];
                        bytes[positionValue] = (byte)MultValue(floatValue, location.Value);
                        break;
                    case "head":
                        if (!Common.Instance.Configuration.BHaptics.HeadEnabled) return;
                        location = PositionType.Head;
                        bytes = new byte[6];
                        bytes[positionValue] = (byte)MultValue(floatValue, location.Value);
                        break;
                }

                //Validations
                if (location == null) return;
                if (bytes == null) return;

                //Send data to device
                Player.Submit("Bytes", location.Value, bytes, 100);
            }
            catch (Exception)
            {
            }
        }

        private int MultValue(float oscValue, PositionType positionType)
        {
            float multiplier = 1;

            if (positionType == PositionType.VestFront || positionType == PositionType.VestBack)
            {
                multiplier = ((float)Common.Instance.Configuration.BHaptics.VestMult / 100);
            }
            else if (positionType == PositionType.Head)
            {
                multiplier = ((float)Common.Instance.Configuration.BHaptics.HeadMult / 100);
            }

            float value = oscValue * multiplier;

            return (int)(value * 100);
        }

        private void ConnectionChanged(bool connected)
        {
            this.Connected = connected;
            HapticsConnectionChanged?.Invoke(connected);
        }

        internal void VestTest()
        {
            Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i <= 1; i++)
                    {
                        for (int node = 0; node < 20; node++)
                        {
                            string pos = "VestFront";
                            if (i == 1) pos = "VestBack";

                            string addr = $"/avatar/parameters/bOSC_v1_{pos}_{node}";
                            OscManager_MessageReceived(addr, 1);
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception) { }
            });
        }

        internal void HeadTest()
        {
            Task.Run(() =>
            {
                try
                {
                    for (int node = 0; node < 6; node++)
                    {
                        string pos = "Head";
                        string addr = $"/avatar/parameters/bOSC_v1_{pos}_{node}";

                        OscManager_MessageReceived(addr, 1);
                        Thread.Sleep(100);
                    }
                }
                catch (Exception) { }
            });
        }
    }
}
