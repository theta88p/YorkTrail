using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace YorkTrail
{
    public class Settings : INotifyPropertyChanged
    {
        public Settings()
        {}

        public float Volume { get; set; } = 1.0f;
        public int DeviceIndex { get; set; } = 0;
        public string DeviceName { get; set; }
        public ObservableCollection<string> RecentFiles { get; set; } = new ObservableCollection<string>();
        public double WindowHeight { get; set; }
        public int MenuToolBarBand { get; set; }
        public int MenuToolBarBandIndex { get; set; }
        public double MenuToolBarWidth { get; set; }
        public int IconToolBarBand { get; set; }
        public int IconToolBarBandIndex { get; set; }
        public double IconToolBarWidth { get; set; }
        private bool _alwaysOnTop = false;
        public bool AlwaysOnTop {
            get { return _alwaysOnTop; }
            set { _alwaysOnTop = value;
                RaisePropertyChanged(nameof(AlwaysOnTop));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private const string xmlPath = @".\Settings.xml";
        public static Settings ReadSettingsFromFile()
        {
            Settings settings;
            if (File.Exists(xmlPath))
            {
                // デシリアライズする
                var xmlSerializer = new XmlSerializer(typeof(Settings));
                var xmlSettings = new System.Xml.XmlReaderSettings()
                {
                    CheckCharacters = false,
                };
                using (var streamReader = new StreamReader(xmlPath, Encoding.UTF8))
                using (var xmlReader = System.Xml.XmlReader.Create(streamReader, xmlSettings))
                {
                    settings = (Settings)xmlSerializer.Deserialize(xmlReader);
                }
            }
            else
            {
                settings = null;
            }
            return settings;
        }

        public static void WriteSettingsToFile(Settings settings)
        {
            // シリアライズする
            var xmlSerializer1 = new XmlSerializer(typeof(Settings));
            using (var streamWriter = new StreamWriter(xmlPath, false, Encoding.UTF8))
            {
                xmlSerializer1.Serialize(streamWriter, settings);
                streamWriter.Flush();
            }
        }


    }
}
