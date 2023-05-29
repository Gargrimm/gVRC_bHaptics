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
        private readonly LogProxy Log = LogProxy.GetLogger(typeof(gVRC_bHaptics.Screens.BHaptics), () => Common.Instance.Configuration.Logs.App);
        private bool FlagLoading;
        private ConfigurationViewModel ViewModel = new ConfigurationViewModel(Common.Instance.Configuration);

        public BHaptics()
        {
            this.DataContext = ViewModel;
            InitializeComponent();

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DesignerProperties.GetIsInDesignMode(this)) return;

                FlagLoading = true;

                Common.Instance.BHapticsMaanger.HapticsConnectionChanged += BHapticsMaanger_HapticsConnectionChanged;
                Common.Instance.OscManager.ConnectionStateChanged += OscManager_ConnectionStateChanged;

                Refresh();

                FlagLoading = false;
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
            }
            catch (Exception ex)
            {
                Log.Error("Refresh", ex);
            }
        }

        private void OscManager_ConnectionStateChanged(bool connected)
        {
            try
            {
                ViewModel.StateVrcOSC = connected;
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
                ViewModel.StateHaptics = connected;
            }
            catch (Exception ex)
            {
                Log.Error("BHapticsMaanger_HapticsConnectionChanged", ex);
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
                    if (!FlagLoading)
                    {
                        Common.Instance.OscManager?.Reconnect();
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

                    if (!FlagLoading)
                    {
                        Common.Instance.OscManager?.Reconnect();
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
    }
}
