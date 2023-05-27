using gVRC_bHaptics.Modules;
using gVRC_bHaptics.ViewModels;
using MicaWPF.Controls;
using System;
using System.Collections.Generic;
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

namespace gVRC_bHaptics
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MicaWindow
    {
        AppViewModel viewModel;

        public MainWindow()
        {
            viewModel = new AppViewModel();
            this.DataContext = viewModel;

            InitializeComponent();

            Common.Instance.GeneralLog.Dispatcher = this.Dispatcher;
            Common.Instance.OSCLog.Dispatcher = this.Dispatcher;
            Common.Instance.Init();
            Common.Instance.StartThreads();

            Application.Current.Exit += Application_Exit;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Common.Instance.Shutdown();
        }
    }
}
