using Newtonsoft.Json;
using gVRC_bHaptics.Classes;
using Rug.Osc;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using gVRC_bHaptics.Classes.Configuration;
using System.Windows.Interop;
using log4net;

namespace gVRC_bHaptics.Modules
{
    internal class OSC
    {
        public delegate void OscConnectionEventHandler(bool connected);
        public event OscConnectionEventHandler ConnectionStateChanged;

        public delegate void OscMessageEventHandler(string address, object value);
        public event OscMessageEventHandler MessageReceived;
        public bool Enabled { get; set; } = true;
        public bool Connected { get; private set; }

        private OscListener OSCListener = null;
        private OscSender OSCSender = null;

        private readonly LogProxy Log = LogProxy.GetLogger(typeof(OSC), () => Common.Instance.Configuration.Logs.App);
        private readonly LogProxy LogOsc = LogProxy.GetLogger("OSC.App", () => Common.Instance.Configuration.Logs.Osc);
        private readonly string IP = "127.0.0.1";
        private readonly int Delay = 100;

        private bool Reload;
        private IPAddress Address;

        Configuration ProxyConfig { get { return Common.Instance?.Configuration; } }

        public OSC()
        {
        }

        public void RunThread()
        {
            try
            {
                Address = IPAddress.Parse(IP);
                Common.Instance.ConfigurationLoaded += (sender, args) => { Reload = true; };

                while (!Common.Instance.Exit)
                {
                    try
                    {
                        if (!Enabled) continue;

                        Log.Debug("Connecting...");

                        using (OSCSender = new OscSender(Address, 0, ProxyConfig.VRCPortIn))
                        using (OSCListener = new OscListener(Address, ProxyConfig.VRCPortOut))
                        {
                            OSCSender.Connect();
                            OSCListener.Connect();

                            OSCListener.Attach("/world/*", OnWorldParamEvent);
                            OSCListener.Attach("/avatar/parameters/*", OnAvatarParamEvent);

                            Log.Debug("Initialized");
                            Connected = true;
                            ConnectionStateChanged?.Invoke(true);

                            while (!Common.Instance.Exit && !Reload && Enabled)
                            {
                                ProcessCommand();
                                Thread.Sleep(Delay);
                            }

                            Disconnect();

                            if (Reload)
                            {
                                Log.Debug("Reloading");
                                Reload = false;
                            }

                            if (!Enabled)
                                Log.Debug("Disabled");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("RunThread loop", ex);
                        Log.Debug("Retrying in 5 second...");
                        Thread.Sleep(5000);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Log.Warn("RunThread cancelled");
            }
            catch (Exception ex)
            {
                Log.Error("RunThread", ex);
                Common.Instance.Exit = true;
            }
            finally
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            try
            {
                OSCSender?.Dispose();
                OSCListener?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("Disconnect", ex);
            }
            finally
            {
                OSCSender = null;
                OSCListener = null;
                Connected = false;
                ConnectionStateChanged?.Invoke(false);
            }
        }

        public void Reconnect()
        {
            Reload = true;
        }

        private bool IsMessageForEndpoint(OscProxyInfo endpoint, OscMessage message)
        {
            try
            {
                //Check regex patterns
                if ((endpoint.PatternsRegEx?.Count).GetValueOrDefault() > 0)
                {
                    bool match = false;
                    foreach (var pattern in endpoint.PatternsRegEx)
                    {
                        var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                        match = rgx.IsMatch(message.Address);
                        if (match) return true; //found
                    }
                }

                //Check contains patterns
                if ((endpoint.PatternsContains?.Count).GetValueOrDefault() > 0)
                {
                    bool match = false;
                    foreach (var pattern in endpoint.PatternsContains)
                    {
                        match = message.Address.Contains(pattern);
                        if (match) return true; //found
                    }
                }

                return false; //none found
            }
            catch (Exception ex)
            {
                Log.Error("IsMessageForEndpoint", ex);
                return true; //defaults to found
            }
        }

        private void RelayMsgToProxyEntries(OscMessage message)
        {
            try
            {
                if ((ProxyConfig?.ProxtyEndPoints?.Count).GetValueOrDefault() == 0) return;

                foreach (var endpoint in ProxyConfig?.ProxtyEndPoints)
                {
                    if (!IsMessageForEndpoint(endpoint, message)) continue;

                    var address = IPAddress.Parse(endpoint.IP);
                    using (var proxySender = new OscSender(address, ProxyConfig.ProxyPortOut, endpoint.Port))
                    {
                        try
                        {
                            proxySender.Connect();
                            proxySender.Send(message);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("RelayMsgToProxyEntries on connect and send", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("RelayMsgToProxyEntries", ex);
            }
        }

        private void ProcessCommand()
        {
            try
            {
                if (Common.Instance.CommnadQueue.TryDequeue(out var cmd))
                {
                    OscMessage msg = null;

                    switch (cmd.Type)
                    {
                        case OscCommandType.Any:
                            msg = new OscMessage($"{cmd.Path}", cmd.Value);
                            break;
                        case OscCommandType.Parameter:
                            msg = new OscMessage($"/avatar/parameters/{cmd.Path}", cmd.Value);
                            break;
                        case OscCommandType.ChatSend:
                            msg = new OscMessage("/chatbox/input", cmd.Value);
                            break;
                        default:
                            return;
                    }

                    OSCSender.Send(msg);
                }
            }
            catch (Exception ex)
            {
                Log.Error("ProcessCommand Error", ex);
            }
        }

        private void OnWorldParamEvent(OscMessage message)
        {
            try
            {
                var addr = message.Address;
                var val = message.FirstOrDefault()?.ToString();

                LogOsc.Debug($"{addr} => {val}");
            }
            catch (Exception ex)
            {
                Log.Error("OnAvatarParamEvent", ex);
            }
        }

        private void OnAvatarParamEvent(OscMessage message)
        {
            try
            {
                var addr = message.Address;
                var val = message.FirstOrDefault()?.ToString();
                Common.Instance.OnAvatarParameterEvent(addr, val);

                RelayMsgToProxyEntries(message);

                MessageReceived?.Invoke(message.Address, message.FirstOrDefault());
                LogOsc.Debug($"{addr} => {val}");
            }
            catch (Exception ex)
            {
                Log.Error("OnAvatarParamEvent", ex);
            }
        }
    }
}
