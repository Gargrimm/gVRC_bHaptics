using gVRC_bHaptics.Classes;
using gVRC_bHaptics.Classes.Configuration;
using gVRC_bHaptics.Modules;
using gVRC_bHaptics.ViewModels;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace gVRC_bHaptics.Screens
{
    /// <summary>
    /// Lógica de interacción para BHaptics.xaml
    /// </summary>
    public partial class BHaptics : UserControl
    {
        readonly OscHapticsViewModel ConnectionInfo = new OscHapticsViewModel();
        private bool flagLoading;

        public BHaptics()
        {
            InitializeComponent();

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            flagLoading = true;

            if (DesignerProperties.GetIsInDesignMode(this)) return;

            Common.Instance.BHapticsMaanger.HapticsConnectionChanged += BHapticsMaanger_HapticsConnectionChanged;
            Common.Instance.OscManager.ConnectionStateChanged += OscManager_ConnectionStateChanged;
            Common.Instance.ConfigurationLoaded += Instance_ConfigurationLoaded;
            Application.Current.Exit += Application_Exit;

            Refresh();

            flagLoading = false;
        }

        private void Refresh()
        {
            OscManager_ConnectionStateChanged(Common.Instance.OscManager.Connected);
            BHapticsMaanger_HapticsConnectionChanged(Common.Instance.BHapticsMaanger.Connected);
            ShowConfig(Common.Instance.Configuration);
        }

        private void ShowConfig(Configuration config)
        {
            TextPortIn.Text = (config?.VRCPortIn).GetValueOrDefault(9001).ToString();
            TextPortOut.Text = (config?.VRCPortOut).GetValueOrDefault(9002).ToString();

            SliderVest.Value = (config?.BHaptics?.VestMult).GetValueOrDefault(100);
            SliderHead.Value = (config?.BHaptics?.HeadMult).GetValueOrDefault(100);

            CheckVestToggle.IsChecked = (config?.BHaptics?.VestEnabled).GetValueOrDefault();
            CheckHeadToggle.IsChecked = (config?.BHaptics?.HeadEnabled).GetValueOrDefault();
            CheckHapticsAFK.IsChecked = (config?.BHaptics?.DisableOnAFK).GetValueOrDefault();
        }

        private void UpdateConnectionText(Label lbl, ConnectionState state)
        {
            lbl.Content = state.ConnectionText;
            //if (state.Connected)
            //{
            //    lbl.SetValue(Label.ForegroundProperty, Brushes.Green);
            //    //lbl.Style = LabelConnectedStyle;
            //}
            //else
            //{
            //    lbl.SetValue(Label.ForegroundProperty, Brushes.Red);
            //    //lbl.Style = LabelDisconnectedStyle;
            //}
        }

        private void SaveConfigFromScreen()
        {
            var config = Common.Instance.Configuration;

            int portInVal = config.VRCPortIn;
            int.TryParse(TextPortIn.Text, out portInVal);

            int portOutVal = config.VRCPortOut;
            int.TryParse(TextPortOut.Text, out portOutVal);

            config.VRCPortIn = portInVal;
            config.VRCPortOut = portOutVal;

            if (config.BHaptics == null) config.BHaptics = new BHapticsConfig();
            config.BHaptics.VestMult = (int)SliderVest.Value;
            config.BHaptics.HeadMult = (int)SliderHead.Value;

            config.BHaptics.VestEnabled = (CheckVestToggle.IsChecked).GetValueOrDefault();
            config.BHaptics.HeadEnabled = (CheckHeadToggle.IsChecked).GetValueOrDefault();
            config.BHaptics.DisableOnAFK = (CheckHapticsAFK.IsChecked).GetValueOrDefault();

            Common.Instance.SaveConfig();
        }

        private void Instance_ConfigurationLoaded(object sender, Configuration config)
        {
            Common.InvokeIfNecessary(() => ShowConfig(config));
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (Common.Instance.Configuration != null)
            {
                SaveConfigFromScreen();
            }
        }

        private void OscManager_ConnectionStateChanged(bool connected)
        {
            Common.InvokeIfNecessary(() =>
            {
                ConnectionInfo.VrcOsc.Connected = connected;
                UpdateConnectionText(LabelOscConnection, ConnectionInfo.VrcOsc);
            });
        }

        private void BHapticsMaanger_HapticsConnectionChanged(bool connected)
        {
            Common.InvokeIfNecessary(() =>
            {
                ConnectionInfo.BhapticsPlayer.Connected = connected;
                UpdateConnectionText(LabelBHapticsConnection, ConnectionInfo.BhapticsPlayer);
            });
        }

        private void SliderHead_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LabelHeadValue == null) return;

            LabelHeadValue.Content = e.NewValue.ToString("n0");

            if (Common.Instance?.Configuration?.BHaptics == null) return;
            Common.Instance.Configuration.BHaptics.HeadMult = (int)e.NewValue;
        }

        private void SliderVest_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LabelVestValue == null) return;

            LabelVestValue.Content = e.NewValue.ToString("n0");

            if (Common.Instance?.Configuration?.BHaptics == null) return;
            Common.Instance.Configuration.BHaptics.VestMult = (int)e.NewValue;
        }

        private void TextPortIn_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(TextPortIn.Text, out _))
            {
                TextPortIn.BorderBrush = Brushes.Red;
            }
            else
            {
                TextPortIn.BorderBrush = null;
                if (!flagLoading)
                {
                    SaveConfigFromScreen();
                    Common.Instance.OscManager.Reconnect();
                }
            }
        }

        private void TextPortOut_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(TextPortOut.Text, out _))
            {
                TextPortOut.BorderBrush = Brushes.Red;
            }
            else
            {
                TextPortOut.BorderBrush = null;

                if (!flagLoading)
                {
                    SaveConfigFromScreen();
                    Common.Instance.OscManager.Reconnect();
                }
            }
        }

        private void ButtonVestTest_Click(object sender, RoutedEventArgs e)
        {
            Common.Instance?.BHapticsMaanger?.VestTest();
        }

        private void ButtonHeadTest_Click(object sender, RoutedEventArgs e)
        {
            Common.Instance?.BHapticsMaanger?.HeadTest();
        }

        private void ButtonTestOSC_Click(object sender, RoutedEventArgs e)
        {
            Common.Instance.CommnadQueue.Enqueue(new OscCommand
            {
                Type = OscCommandType.ChatSend,
                Value = new object[] { "Hello world!", true }
            });
        }

        private void CheckHeadToggle_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Common.Instance?.Configuration?.BHaptics == null) return;
            Common.Instance.Configuration.BHaptics.HeadEnabled = (CheckHeadToggle.IsChecked).GetValueOrDefault();
        }

        private void CheckVestToggle_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Common.Instance?.Configuration?.BHaptics == null) return;
            Common.Instance.Configuration.BHaptics.VestEnabled = (CheckVestToggle.IsChecked).GetValueOrDefault();
        }

        private void CheckHapticsAFK_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Common.Instance?.Configuration?.BHaptics == null) return;
            Common.Instance.Configuration.BHaptics.DisableOnAFK = (CheckHapticsAFK.IsChecked).GetValueOrDefault();
        }
    }
}
