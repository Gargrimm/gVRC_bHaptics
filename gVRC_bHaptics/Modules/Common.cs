using gVRC_bHaptics.Classes;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using gVRC_bHaptics.Classes.Configuration;
using System.Windows;

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

        private CancellationTokenSource ThreadsCancel = null;

        public Task TaskOSC = null;
        public Task TaskBHaptics = null;

        public bool Exit;
        public ConcurrentQueue<OscCommand> CommnadQueue = new ConcurrentQueue<OscCommand>();

        public delegate void AvatarParameterChangedEventHandler(string address, string value);
        public event AvatarParameterChangedEventHandler AvatarParameterEvent;

        public delegate void ConfigurationLoadedEventHandler(object sender, Configuration config);
        public event ConfigurationLoadedEventHandler ConfigurationLoaded;

        public CultureInfo CultureUS = new CultureInfo("en-US");
        public CultureInfo CultureES = new CultureInfo("es-ES");
        public Logger GeneralLog = new Logger();
        public Logger OSCLog = new Logger();

        public OSC OscManager { get; private set; }
        public BHaptics BHapticsMaanger { get; private set; }
        public Configuration Configuration { get; private set; }

        private readonly string ConfigFilePath = ".";
        private readonly string ConfigFileName = "config.json";

        public void OnAvatarParameterEvent(string address, string value)
        {

            AvatarParameterEvent?.Invoke(address, value);
        }

        public void Init()
        {
            ReadConfig();

            Exit = false;
            ThreadsCancel = new CancellationTokenSource();
        }

        public void Shutdown()
        {
            Exit = true;
            StopThreads();
            SaveConfig();
        }

        public void ReadConfig()
        {
            try
            {
                string pathToFile = Path.Combine(ConfigFilePath, ConfigFileName);
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
            catch (Exception)
            {
            }
        }

        public void SaveConfig()
        {
            try
            {
                string pathToFile = Path.Combine(ConfigFilePath, ConfigFileName);
                string contents = Newtonsoft.Json.JsonConvert.SerializeObject(Configuration, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(pathToFile, contents);
            }
            catch(Exception) { }
        }

        public void StartThreads()
        {
            OscManager = new OSC(this);
            BHapticsMaanger = new BHaptics(this);

            Exit = false;
            TaskOSC = Task.Factory.StartNew(() => OscManager.RunThread(), ThreadsCancel.Token);
            TaskBHaptics = Task.Factory.StartNew(() => BHapticsMaanger.RunThread(), ThreadsCancel.Token);
        }

        public void StopThreads()
        {
            Exit = true;
            ThreadsCancel.Cancel();
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
            catch (Exception)
            {
            }
        }
    }
}
