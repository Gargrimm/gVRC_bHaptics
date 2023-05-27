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

        OscListener OSCListener = null;
        OscSender OSCSender = null;

        readonly Common Common;
        readonly string IP = "127.0.0.1";
        readonly int Delay = 100;

        bool Reload;
        IPAddress Address;

        Configuration ProxyConfig { get { return Common?.Configuration; } }

        public OSC(Common common)
        {
            Common = common;
        }

        public void RunThread()
        {
            try
            {
                Address = IPAddress.Parse(IP);
                Common.ConfigurationLoaded += (sender, args) => { Reload = true; };

                while (!Common.Exit)
                {
                    try
                    {
                        if (!Enabled) continue;

                        Common.GeneralLog.Log("(OSC.RunThread) Connecting...");

                        using (OSCSender = new Rug.Osc.OscSender(Address, 0, ProxyConfig.VRCPortIn))
                        using (OSCListener = new Rug.Osc.OscListener(Address, ProxyConfig.VRCPortOut))
                        {
                            OSCSender.Connect();
                            OSCListener.Connect();

                            OSCListener.Attach("/world/*", OnWorldParamEvent);
                            OSCListener.Attach("/avatar/parameters/*", OnAvatarParamEvent);

                            Common.GeneralLog.Log("(OSC.RunThread) Initialized");
                            Connected = true;
                            ConnectionStateChanged?.Invoke(true);

                            while (!Common.Exit && !Reload && Enabled)
                            {
                                ProcessCommand();
                                Thread.Sleep(Delay);
                            }

                            Disconnect();

                            if (Reload)
                            {
                                Common.GeneralLog.Log("(OSC.RunThread) Reloading");
                                Reload = false;
                            }

                            if (!Enabled)
                            {
                                Common.GeneralLog.Log("(OSC.RunThread) Disabled");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.GeneralLog.Log("(OSC.RunThread) Error");
                        Common.GeneralLog.Log(ex);
                        Common.GeneralLog.Log("(OSC.RunThread) Retrying in 5 second...");
                        Thread.Sleep(5000);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Common.GeneralLog.Log("(OSC.RunThread) Cancelled");
            }
            catch (Exception ex)
            {
                Common.GeneralLog.Log("(OSC.RunThread) Error");
                Common.GeneralLog.Log(ex);
                Common.Exit = true;
            }
            finally
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            OSCSender?.Dispose();
            OSCListener?.Dispose();
            OSCSender = null;
            OSCListener = null;
            Connected = false;
            ConnectionStateChanged?.Invoke(false);
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
                        if (match) { return true; } //found
                    }
                }

                //Check contains patterns
                if ((endpoint.PatternsContains?.Count).GetValueOrDefault() > 0)
                {
                    bool match = false;
                    foreach (var pattern in endpoint.PatternsContains)
                    {
                        match = message.Address.Contains(pattern);
                        if (match) { return true; } //found
                    }
                }

                return false; //none found
            }
            catch
            {
                return true; //default to found
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
                    using (var proxySender = new Rug.Osc.OscSender(address, ProxyConfig.ProxyPortOut, endpoint.Port))
                    {
                        try
                        {
                            proxySender.Connect();
                            proxySender.Send(message);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }                

        private void ProcessCommand()
        {
            try
            {
                if (Common.CommnadQueue.TryDequeue(out var cmd))
                {
                    OscMessage msg = null;

                    if (cmd.Type == OscCommandType.Any)
                    {
                        msg = new OscMessage($"{cmd.Path}", cmd.Value);
                    }
                    else if (cmd.Type == OscCommandType.Parameter)
                    {
                        msg = new OscMessage($"/avatar/parameters/{cmd.Path}", cmd.Value);

                    }
                    else if (cmd.Type == OscCommandType.ChatSend)
                    {
                        msg = new OscMessage("/chatbox/input", cmd.Value);
                    }

                    OSCSender.Send(msg);
                }
            }
            catch (Exception ex)
            {
                Common.GeneralLog.Log("(OSC.ProcessCommand) Error");
                Common.GeneralLog.Log(ex);
            }
        }

        private void OnWorldParamEvent(OscMessage message)
        {
        }

        private void OnAvatarParamEvent(OscMessage message)
        {
            try
            {
                if (message.Address.Equals("/avatar/parameters/Upright")) return;
                if (message.Address.StartsWith("/avatar/parameters/Cheek_")) return;
                if (message.Address.StartsWith("/avatar/parameters/Angular")) return;

                if ((Common.Instance?.Configuration?.OscLog).GetValueOrDefault())
                {
                    File.AppendAllText("osc.log", $"=> {message.Address}, {message.FirstOrDefault()}{Environment.NewLine}");
                }

                //Common.OSCLog.Log($"=> {message.Address} {message.FirstOrDefault()}");
                var addr = message.Address;
                var val = message.FirstOrDefault()?.ToString();
                Common.OnAvatarParameterEvent(addr, val);
                Console.WriteLine($"{addr} {val}");

                RelayMsgToProxyEntries(message);

                MessageReceived?.Invoke(message.Address, message.FirstOrDefault());
            }
            catch (Exception ex)
            {
                Common.GeneralLog.Log("(OSC.OnAvatarParamEvent) Error");
                Common.GeneralLog.Log(ex);
            }
        }

        public void Send(string address, object value)
        {
            //Common.Instance.CommnadQueue.Enqueue(new OscCommand
            //{
            //    Type = OscCommandType.ChatSend,
            //    Value = new object[] { value, true }
            //});

            ////OscMessage message = new OscMessage(address, value);
            ////OSCSender.Send(message);
        }
    }
}
