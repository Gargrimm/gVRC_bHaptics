using gVRC_bHaptics.Classes;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using gVRC_bHaptics.Classes.Configuration;
using System.Windows;
using log4net;

namespace gVRC_bHaptics.Modules
{
    internal class Common
    {
        private static Common _Instance;
        public static Common Instance
        {
            get
            {
                if (_Instance == null) _Instance = new Common();

                return _Instance;
            }
        }

        public Task TaskOSC = null;
        public Task TaskBHaptics = null;

        public bool Exit;
        public ConcurrentQueue<OscCommand> CommnadQueue = new ConcurrentQueue<OscCommand>();

        public delegate void AvatarParameterChangedEventHandler(string address, string value);
        public event AvatarParameterChangedEventHandler AvatarParameterEvent;

        public delegate void ConfigurationLoadedEventHandler(object sender, Configuration config);
        public event ConfigurationLoadedEventHandler ConfigurationLoaded;

        public OSC OscManager { get; private set; }
        public BHaptics BHapticsMaanger { get; private set; }
        public Configuration Configuration { get; private set; }
        public VrcState VrcState { get; private set; }

        private CancellationTokenSource ThreadsCancel = null;


        private Common()
        {
        }

        public void OnAvatarParameterEvent(string address, string value)
        {
            try
            {
                AvatarParameterEvent?.Invoke(address, value);
            }
            catch (Exception ex)
            {
                App.Log.Error("OnAvatarParameterEvent", ex);
            }
        }

        public void Init()
        {
            try
            {
                Exit = false;
                ThreadsCancel = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                App.Log.Error("Init", ex);
            }
        }

        public void Shutdown()
        {
            try
            {
                Exit = true;
                VrcState = null;

                StopThreads();
                SaveConfig();
            }
            catch (Exception ex)
            {
                App.Log.Error("Shutdown", ex);
            }
        }

        public void ReadConfig()
        {
            try
            {
                string pathToFile = Path.Combine(Constants.ConfigFilePath, Constants.ConfigFileName);
                if (File.Exists(pathToFile))
                {
                    string contents = File.ReadAllText(pathToFile);
                    Configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(contents);
                    ConfigurationLoaded?.Invoke(this, Configuration);
                }
                else
                {
                    Configuration = new Configuration();
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                App.Log.Error("ReadConfig", ex);
            }
        }

        public void SaveConfig()
        {
            try
            {
                string pathToFile = Path.Combine(Constants.ConfigFilePath, Constants.ConfigFileName);
                string contents = Newtonsoft.Json.JsonConvert.SerializeObject(Configuration, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(pathToFile, contents);
            }
            catch (Exception ex)
            {
                App.Log.Error("SaveConfig", ex);
            }
        }

        public void StartThreads()
        {
            try
            {
                OscManager = new OSC();
                BHapticsMaanger = new BHaptics();
                VrcState = new VrcState();

                Exit = false;
                TaskOSC = Task.Factory.StartNew(() => OscManager.RunThread(), ThreadsCancel.Token);
                TaskBHaptics = Task.Factory.StartNew(() => BHapticsMaanger.RunThread(), ThreadsCancel.Token);
            }
            catch (Exception ex)
            {
                App.Log.Error("StartThreads", ex);
            }
        }

        public void StopThreads()
        {
            try
            {
                Exit = true;
                ThreadsCancel.Cancel();
            }
            catch (Exception ex)
            {
                App.Log.Error("StopThreads", ex);
            }
        }

        public static void InvokeIfNecessary(Action action)
        {
            try
            {
                if (Thread.CurrentThread == Application.Current?.Dispatcher?.Thread)
                    action();
                else
                {
                    Application.Current?.Dispatcher?.Invoke(action);
                }
            }
            catch (Exception ex)
            {
                App.Log.Error("InvokeIfNecessary", ex);
            }
        }
    }
}
