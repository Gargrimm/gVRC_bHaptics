using gVRC_bHaptics.Classes;
using gVRC_bHaptics.Modules;
using log4net;
using System.Windows;

namespace gVRC_bHaptics
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly LogProxy Log = LogProxy.GetLogger(typeof(App), () => Common.Instance.Configuration.Logs.App);

        public App()
        {
            Common.Instance.ReadConfig();
            Exit += Application_Exit;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Common.Instance.SaveConfig();
        }
    }
}
