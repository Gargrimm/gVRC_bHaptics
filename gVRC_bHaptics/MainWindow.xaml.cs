using gVRC_bHaptics.Modules;
using gVRC_bHaptics.ViewModels;
using MicaWPF.Controls;
using System.Windows;

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
