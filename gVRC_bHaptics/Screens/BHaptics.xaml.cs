using gVRC_bHaptics.Classes;
using gVRC_bHaptics.Classes.Configuration;
using gVRC_bHaptics.Modules;
using gVRC_bHaptics.ViewModels;
using log4net;
using System;
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
        private readonly ILog Log = LogManager.GetLogger(typeof(gVRC_bHaptics.Screens.BHaptics));
        readonly OscHapticsViewModel ConnectionInfo = new OscHapticsViewModel();
        private bool flagLoading;

        public BHaptics()
        {
            InitializeComponent();

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error("Loaded", ex);
            }

        }

        private void Refresh()
        {
            try
            {
                OscManager_ConnectionStateChanged(Common.Instance.OscManager.Connected);
                BHapticsMaanger_HapticsConnectionChanged(Common.Instance.BHapticsMaanger.Connected);
                ShowConfig(Common.Instance.Configuration);
            }
            catch (Exception ex)
            {
                Log.Error("Refresh", ex);
            }
        }

        private void ShowConfig(Configuration config)
        {
            try
            {
                TextPortIn.Text = (config?.VRCPortIn).GetValueOrDefault(9001).ToString();
                TextPortOut.Text = (config?.VRCPortOut).GetValueOrDefault(9002).ToString();

                SliderVest.Value = (config?.BHaptics?.VestMult).GetValueOrDefault(100);
                SliderHead.Value = (config?.BHaptics?.HeadMult).GetValueOrDefault(100);

                CheckVestToggle.IsChecked = (config?.BHaptics?.VestEnabled).GetValueOrDefault();
                CheckHeadToggle.IsChecked = (config?.BHaptics?.HeadEnabled).GetValueOrDefault();
                CheckHapticsAFK.IsChecked = (config?.BHaptics?.DisableOnAFK).GetValueOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error("ShowConfig", ex);
            }
        }

        private void UpdateConnectionText(Label lbl, ConnectionState state)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error("UpdateConnectionText", ex);
            }
        }

        private void SaveConfigFromScreen()
        {
            try
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
            catch (Exception ex)
            {
                Log.Error("SaveConfigFromScreen", ex);
            }

        }

        private void Instance_ConfigurationLoaded(object sender, Configuration config)
        {
            try
            {
                Common.InvokeIfNecessary(() => ShowConfig(config));
            }
            catch (Exception ex)
            {
                Log.Error("ConfigurationLoaded", ex);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                if (Common.Instance?.Configuration != null)
                {
                    SaveConfigFromScreen();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Application_Exit", ex);
            }
        }

        private void OscManager_ConnectionStateChanged(bool connected)
        {
            try
            {
                Common.InvokeIfNecessary(() =>
                {
                    ConnectionInfo.VrcOsc.Connected = connected;
                    UpdateConnectionText(LabelOscConnection, ConnectionInfo.VrcOsc);
                });
            }
            catch (Exception ex)
            {
                Log.Error("OscManager_ConnectionStateChanged", ex);
            }
        }

        private void BHapticsMaanger_HapticsConnectionChanged(bool connected)
        {
            try
            {
                Common.InvokeIfNecessary(() =>
                {
                    ConnectionInfo.BhapticsPlayer.Connected = connected;
                    UpdateConnectionText(LabelBHapticsConnection, ConnectionInfo.BhapticsPlayer);
                });
            }
            catch (Exception ex)
            {
                Log.Error("BHapticsMaanger_HapticsConnectionChanged", ex);
            }

        }

        private void SliderHead_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (LabelHeadValue == null) return;

                LabelHeadValue.Content = e.NewValue.ToString("n0");

                if (Common.Instance?.Configuration?.BHaptics == null) return;
                Common.Instance.Configuration.BHaptics.HeadMult = (int)e.NewValue;
            }
            catch (Exception ex)
            {
                Log.Error("SliderHead_ValueChanged", ex);
            }
        }

        private void SliderVest_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (LabelVestValue == null) return;

                LabelVestValue.Content = e.NewValue.ToString("n0");

                if (Common.Instance?.Configuration?.BHaptics == null) return;
                Common.Instance.Configuration.BHaptics.VestMult = (int)e.NewValue;
            }
            catch (Exception ex)
            {
                Log.Error("SliderVest_ValueChanged", ex);
            }
        }

        private void TextPortIn_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error("TextPortIn_TextChanged", ex);
            }
        }

        private void TextPortOut_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error("TextPortIn_TextChanged", ex);
            }
        }

        private void ButtonVestTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Common.Instance?.BHapticsMaanger?.VestTest();

            }
            catch (Exception ex)
            {
                Log.Error("ButtonVestTest_Click", ex);
            }
        }

        private void ButtonHeadTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Common.Instance?.BHapticsMaanger?.HeadTest();

            }
            catch (Exception ex)
            {
                Log.Error("ButtonHeadTest_Click", ex);
            }            
        }

        private void ButtonTestOSC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Common.Instance.CommnadQueue.Enqueue(new OscCommand
                {
                    Type = OscCommandType.ChatSend,
                    Value = new object[] { "Hello world!", true }
                });
            }
            catch (Exception ex)
            {
                Log.Error("ButtonTestOSC_Click", ex);
            }
        }

        private void CheckHeadToggle_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Common.Instance?.Configuration?.BHaptics == null) return;
                Common.Instance.Configuration.BHaptics.HeadEnabled = (CheckHeadToggle.IsChecked).GetValueOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error("CheckHeadToggle_CheckedChanged", ex);
            }
        }

        private void CheckVestToggle_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Common.Instance?.Configuration?.BHaptics == null) return;
                Common.Instance.Configuration.BHaptics.VestEnabled = (CheckVestToggle.IsChecked).GetValueOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error("CheckHeadToggle_CheckedChanged", ex);
            }
        }

        private void CheckHapticsAFK_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Common.Instance?.Configuration?.BHaptics == null) return;
                Common.Instance.Configuration.BHaptics.DisableOnAFK = (CheckHapticsAFK.IsChecked).GetValueOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error("CheckHapticsAFK_CheckedChanged", ex);
            }

        }
    }
}
