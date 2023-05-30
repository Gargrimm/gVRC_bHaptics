using Bhaptics.ModUI.Controls;
using Bhaptics.Tact;
using gVRC_bHaptics.Classes;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.AllJoyn;
using Windows.UI.Core;
using YamlDotNet.Serialization;

namespace gVRC_bHaptics.Modules
{
    internal class BHaptics
    {
        private class MotorStateChange
        {
            public PositionType Location { get; }
            public int Index { get; }
            public int Intensity { get; }

            public MotorStateChange(PositionType location, int index, int intensity)
            {
                Location = location;
                Index = index;
                Intensity = intensity;
            }
        }

        private readonly LogProxy Log = LogProxy.GetLogger(typeof(BHaptics), () => Common.Instance.Configuration.Logs.App);
        private readonly LogProxy LogOscMessage = LogProxy.GetLogger("OSC.BHaptics", () => Common.Instance.Configuration.Logs.HapticsOSC);
        private readonly LogProxy LogDotPoint = LogProxy.GetLogger("BHaptics.DotPoint", () => Common.Instance.Configuration.Logs.HapticsValues);

        private IHapticPlayer Player;
        private HapticPlayerManager HapticPlayerManager;

        private static readonly int[] MirroredVestMotors = new int[20]
        {
          3,2,1,0,7,6,5,4,11,10,9,8,15,14,13,12,19,18,17,16
        };

        private static readonly int[] MirroredHeadMotors = new int[6]
        {
          5,4,3,2,1,0
        };

        private readonly Dictionary<PositionType, int[]> MotorState = new Dictionary<PositionType, int[]>();

        public delegate void HapticsConnectionChangedEventHandler(bool connected);
        public event HapticsConnectionChangedEventHandler HapticsConnectionChanged;
        public bool Connected;


        public BHaptics()
        {
        }

        public void RunThread()
        {
            var delay = 40; //Delay for each update (ms)

            try
            {
                Log.Debug($"Initializing...");

                HapticPlayerManager.SetAppInfo("gVRC_bHaptics", "gVRC_bHaptics");
                this.Player = HapticPlayerManager.Instance().GetHapticPlayer();
                this.HapticPlayerManager = HapticPlayerManager.Instance();
                this.HapticPlayerManager.BhapticsPlayerConnectionChange = (x) => ConnectionChanged(x);
                ConnectionChanged(this.HapticPlayerManager.Connected);

                if (Common.Instance.OscManager != null)
                {
                    Common.Instance.OscManager.MessageReceived += OscManager_MessageReceived;
                }

                Log.Debug("Update loop...");
                while (!Common.Instance.Exit)
                {
                    Play();
                    Thread.Sleep(delay);
                }
                Log.Debug("Exit update loop");
            }
            catch (ThreadAbortException)
            {
                Log.Warn("Cancelled");
            }
            catch (Exception ex)
            {
                Log.Error("Error", ex);
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

                //Parse location name and position index
                var locationName = locationNameParts[0];
                var position = locationNameParts[1];
                if (!int.TryParse(position, out int positionValue)) return;

                //Save the data
                var changes = Update(locationName, positionValue, floatValue);

                if (changes != null)
                    LogOscMessage.Debug($"{address}: {floatValue:n2} ==> {changes.Location}[{changes.Index}] = {changes.Intensity}");
            }
            catch (Exception ex)
            {
                Log.Error("OscManager_MessageReceived", ex);
            }
        }

        /// <summary>
        /// Send the current state to the devices
        /// </summary>
        private void Play()
        {
            try
            {
                //Check if there is anything to write
                bool writeLog = MotorState.Any(x => x.Value.Any(y => y > 0));
                var disabledAFK = Common.Instance.Configuration.BHaptics.DisableWhenAFK && Common.Instance.VrcState.AFK;
                if (!disabledAFK)
                {


                    if (writeLog)
                        LogDotPoint.Debug("Begin update:");

                    //Send current state
                    foreach (var locationData in MotorState)
                    {
                        PositionType location = locationData.Key;
                        int[] motorsValues = locationData.Value;
                        if (motorsValues == null) continue;

                        //Get the state of each motor
                        List<DotPoint> values = new List<DotPoint>();
                        for (var index = 0; index < motorsValues.Length; index++)
                            if (motorsValues[index] > 0)
                                values.Add(new DotPoint(index, motorsValues[index]));

                        //Check if that location is disabled
                        bool disabled = false;
                        switch (location)
                        {
                            case PositionType.VestBack:
                            case PositionType.VestFront:
                                disabled = !Common.Instance.Configuration.BHaptics.VestEnabled;
                                break;
                            case PositionType.Head:
                                disabled = !Common.Instance.Configuration.BHaptics.HeadEnabled;
                                break;
                            default:
                                disabled = false;
                                break;
                        }

                        //Update the state
                        string locationName = $"pos{location}";
                        if (values.Count > 0 && !disabled)
                        {
                            Player.Submit(locationName, location, values, 100);

                            if (writeLog)
                                foreach (var dotpoint in values)
                                    LogDotPoint.Debug($"    {location}[{dotpoint.Index}] = {dotpoint.Intensity}");
                        }
                        else
                        {
                            if (writeLog) LogDotPoint.Debug($"    {location}: Off");
                            Player.TurnOff(locationName);
                        }
                    }
                }

                //Clear
                foreach (var locationData in MotorState)
                {
                    for (int index = 0; index < locationData.Value.Length; index++)
                        locationData.Value[index] = 0;
                }


                if (writeLog)
                    LogDotPoint.Debug("End update");
            }
            catch (Exception ex)
            {
                Log.Error("Play", ex);
            }
        }

        /// <summary>
        /// Save the value for a motor
        /// </summary>
        /// <param name="oscLocation"></param>
        /// <param name="index"></param>
        /// <param name="oscIntensity"></param>
        /// <returns></returns>
        private MotorStateChange Update(string oscLocation, int index, float oscIntensity)
        {
            try
            {
                PositionType? location = GetLocation(oscLocation);
                if (location == null) return null;

                if (!MotorState.ContainsKey(location.Value))
                    MotorState.Add(location.Value, new int[20]);
                if (index >= 20) return null;

                //Save the intensity for next update
                int intensity = FinalIntensity(oscIntensity, location.Value);
                int fixedIndex = GetIndex(location.Value, index);
                MotorState[location.Value][fixedIndex] = intensity;

                return new MotorStateChange(location.Value, fixedIndex, intensity);
            }
            catch (Exception ex)
            {
                Log.Error("Update", ex);
                return null;
            }

        }

        /// <summary>
        /// Parse OSC location name to a PositionType
        /// </summary>
        /// <param name="oscLocation"></param>
        /// <returns></returns>
        private PositionType? GetLocation(string oscLocation)
        {
            try
            {
                if (String.IsNullOrEmpty(oscLocation))
                {
                    Log.Warn($"GetLocation() oscLocation is empty");
                    return null;
                }

                switch (oscLocation.ToLower().Trim())
                {
                    case "vestfront":
                        return PositionType.VestFront;
                    case "vestback":
                        return PositionType.VestBack;
                    case "head":
                        return PositionType.Head;
                    default:
                        Log.Warn($"GetLocation() \"{oscLocation}\" not yet implemented");
                        return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error("GetLocation", ex);
                return null;
            }
        }


        /// <summary>
        /// Gets the internal index for each position
        /// </summary>
        /// <param name="location"></param>
        /// <param name="rawIndex"></param>
        /// <returns></returns>
        private int GetIndex(PositionType location, int rawIndex)
        {
            try
            {
                switch (location)
                {
                    case PositionType.VestBack:
                    case PositionType.VestFront:
                        if (rawIndex >= MirroredVestMotors.Length) return rawIndex;
                        return rawIndex;

                    case PositionType.Head:
                        if (rawIndex >= MirroredHeadMotors.Length) return rawIndex;
                        return rawIndex;

                    default:
                        Log.Warn($"GetIndex() \"{location}\" not yet implemented.");
                        return rawIndex;
                }

            }
            catch (Exception ex)
            {
                Log.Error("GetIndex", ex);
                return rawIndex;
            }
        }

        /// <summary>
        /// Gets the final intensity value
        /// </summary>
        /// <param name="oscValue"></param>
        /// <param name="positionType"></param>
        /// <returns></returns>
        private int FinalIntensity(float oscValue, PositionType positionType)
        {
            int multiplier = 100;
            float value = oscValue;

            try
            {

                switch (positionType)
                {
                    case PositionType.VestBack:
                    case PositionType.VestFront:
                        multiplier = Common.Instance.Configuration.BHaptics.VestMult;
                        break;
                    case PositionType.Head:
                        multiplier = Common.Instance.Configuration.BHaptics.HeadMult;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error("MultValue", ex);
            }

            return (int)(value * multiplier);
        }

        private void ConnectionChanged(bool connected)
        {
            try
            {
                Log.Debug($"Connection => {connected}");
                this.Connected = connected;
                HapticsConnectionChanged?.Invoke(connected);
            }
            catch (Exception ex)
            {
                Log.Error("ConnectionChanged", ex);
            }
        }

        internal void VestTest()
        {
            Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i <= 1; i++)
                        for (int node = 0; node < 20; node++)
                            for (float val = 0.1f; val <= 1f; val += 0.2f)
                            {
                                string pos = "VestFront";
                                if (i == 1) pos = "VestBack";

                                string addr = $"/avatar/parameters/bOSC_v1_{pos}_{node}";
                                OscManager_MessageReceived(addr, val);
                                Thread.Sleep(10);
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

                        OscManager_MessageReceived(addr, 1f);
                        Thread.Sleep(10);
                    }
                }
                catch (Exception) { }
            });
        }
    }
}
