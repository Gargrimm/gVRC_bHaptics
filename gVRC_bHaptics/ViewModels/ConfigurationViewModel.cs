using gVRC_bHaptics.Classes;
using gVRC_bHaptics.Classes.Configuration;
using System.ComponentModel;
using System.Net.Configuration;
using System.Windows.Navigation;
using Windows.ApplicationModel.VoiceCommands;

namespace gVRC_bHaptics.ViewModels
{
    internal class ConfigurationViewModel : ViewModelBase
    {
        private Configuration model;
        public ConfigurationViewModel(Configuration model)
        {
            this.model = model;
        }

        private bool _stateVrcOSC;
        public bool StateVrcOSC
        {
            get => _stateVrcOSC;
            set
            {
                SetProperty(ref _stateVrcOSC, value);
                StateVrcOscDesc = ConnectionDesc(value);
            }
        }

        private bool _stateHaptics;
        public bool StateHaptics
        {
            get => _stateHaptics;
            set
            {
                SetProperty(ref _stateHaptics, value);
                StateHapticsDesc = ConnectionDesc(value);
            }
        }

        private string _stateVrcOscDesc;
        public string StateVrcOscDesc
        {
            get => _stateVrcOscDesc;
            set => SetProperty(ref _stateVrcOscDesc, value);
        }

        private string _stateHapticsDesc;
        public string StateHapticsDesc
        {
            get => _stateHapticsDesc;
            set => SetProperty(ref _stateHapticsDesc, value);
        }

        public string VrcPortIn
        {
            get => model.VRCPortIn.ToString();
            set
            {
                if (int.TryParse(value, out int parseVal))
                {
                    model.VRCPortIn = parseVal;
                    NotifyPropertyChanged();
                }
            }
        }

        public string VrcPortOut
        {
            get => model.VRCPortOut.ToString();
            set
            {
                if (int.TryParse(value, out int parseVal))
                {
                    model.VRCPortOut = parseVal;
                    NotifyPropertyChanged();
                }
            }
        }

        public float VestMult
        {
            get => (int)(model?.BHaptics?.Vest.Mult).GetValueOrDefault(100);
            set
            {
                model.BHaptics.Vest.Mult = (int)value;
                NotifyPropertyChanged();
            }
        }

        public float HeadMult
        {
            get => (int)(model?.BHaptics?.Head?.Mult).GetValueOrDefault(100);
            set
            {
                model.BHaptics.Head.Mult = (int)value;
                NotifyPropertyChanged();
            }
        }

        public bool VestEnabled
        {
            get => model.BHaptics.Vest.Enabled;
            set
            {
                model.BHaptics.Vest.Enabled = value;
                NotifyPropertyChanged();
            }
        }

        public bool HeadEnabled
        {
            get => model.BHaptics.Head.Enabled;
            set
            {
                model.BHaptics.Head.Enabled = value;
                NotifyPropertyChanged();
            }
        }

        public bool DisableWhenAFK
        {
            get => model.BHaptics.DisableWhenAFK;
            set
            {
                model.BHaptics.DisableWhenAFK = value;
                NotifyPropertyChanged();
            }
        }

        private string ConnectionDesc(bool state)
        {
            return (state) ? "Ready" : "Disconnected";
        }
    }
}
