using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YorkTrail
{
    public static class StaticCommands
    {
        public static RoutedCommand OpenRecentFile { get; private set; }
        public static RoutedCommand PlaybackDeviceSelect { get; private set; }
        public static RoutedCommand SetFilterPreset { get; private set; }

        static StaticCommands()
        {
            StaticCommands.OpenRecentFile = new RoutedCommand();
            StaticCommands.PlaybackDeviceSelect = new RoutedCommand();
            StaticCommands.SetFilterPreset = new RoutedCommand();
        }
    }
}
