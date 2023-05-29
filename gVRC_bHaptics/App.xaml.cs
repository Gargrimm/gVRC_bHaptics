//using REghZyFramework.Themes;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.SessionState;
using System.Windows;

namespace gVRC_bHaptics
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly ILog Log = LogManager.GetLogger(typeof(App));

        public App()
        {
            //ThemesController.SetTheme(ThemesController.ThemeTypes.ColourfulDark);
        }
    }
}
