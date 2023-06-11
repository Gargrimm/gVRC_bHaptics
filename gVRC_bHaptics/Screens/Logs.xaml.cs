using gVRC_bHaptics.Classes;
using gVRC_bHaptics.Modules;
using gVRC_bHaptics.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gVRC_bHaptics.Screens
{
    /// <summary>
    /// Lógica de interacción para Logs.xaml
    /// </summary>
    public partial class Logs : UserControl
    {
        private readonly LogProxy Log = LogProxy.GetLogger(typeof(gVRC_bHaptics.Screens.Logs), () => Common.Instance.Configuration.Logs.App);

        private LogsViewModel ViewModel = new LogsViewModel(Common.Instance.Configuration);

        public Logs()
        {
            this.DataContext = ViewModel;
            InitializeComponent();
        }

        private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = Constants.AppFolder,
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }
    }
}
