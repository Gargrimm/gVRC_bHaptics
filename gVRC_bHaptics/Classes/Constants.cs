using System;
using System.IO;

namespace gVRC_bHaptics.Classes
{
    internal class Constants
    {
        public static string AppName { get; } = "VRC OSC for bHaptics";
        public static string AppShortName { get; } = "gVRC_bHaptics";
        public static string Version { get; } = "v0.2";
        public static string Title { get; } = $"{AppName} {Version}";
        public static string AppFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppShortName);
        public static string ConfigFilePath { get; } = AppFolder;
        public static string ConfigFileName { get; } = "config.json";
    }
}
