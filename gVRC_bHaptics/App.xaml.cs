using gVRC_bHaptics.Classes;
using gVRC_bHaptics.Modules;
using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;
using System.Windows;

namespace gVRC_bHaptics
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static LogProxy Log { get; private set; }

        public App()
        {
            LoadLog4NetConfig();
            Log = LogProxy.GetLogger(typeof(App), () => Common.Instance.Configuration.Logs.App);
            Common.Instance.ReadConfig();
            Exit += Application_Exit;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Common.Instance.SaveConfig();
        }

        private void LoadLog4NetConfig()
        {
            string resourceName = "gVRC_bHaptics.log4net.config";

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                XmlConfigurator.Configure(stream);
            }
        }
    }
}
