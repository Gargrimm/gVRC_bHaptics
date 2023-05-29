using gVRC_bHaptics.Classes.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gVRC_bHaptics.ViewModels
{
    internal class LogsViewModel: ViewModelBase
    {
        private Configuration model;

        public LogsViewModel(Configuration model)
        {
            this.model = model;
        }

        public bool App
        {
            get => model.Logs.App;
            set
            {
                model.Logs.App = value;
                NotifyPropertyChanged();
            }
        }

        public bool OSC
        {
            get => model.Logs.Osc;
            set
            {
                model.Logs.Osc = value;
                NotifyPropertyChanged();
            }
        }

        public bool HapticsOSC
        {
            get => model.Logs.HapticsOSC;
            set
            {
                model.Logs.HapticsOSC = value;
                NotifyPropertyChanged();
            }
        }

        public bool HapticsValues
        {
            get => model.Logs.HapticsValues;
            set
            {
                model.Logs.HapticsValues = value;
                NotifyPropertyChanged();
            }
        }
    }
}
