using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

namespace YorkTrail
{
    [DataContract]
    public class Settings : INotifyPropertyChanged
    {
        public Settings()
        {
            SetDefaultKeyBinds();
        }
        [DataMember]
        public float Volume { get; set; } = 1.0f;
        [DataMember]
        public int DeviceIndex { get; set; } = 0;
        [DataMember]
        public string DeviceName { get; set; }
        [DataMember]
        public ObservableCollection<string> RecentFiles { get; set; } = new ObservableCollection<string>();
        [DataMember]
        public double WindowHeight { get; set; }
        [DataMember]
        public int MenuToolBarBand { get; set; }
        [DataMember]
        public int MenuToolBarBandIndex { get; set; }
        [DataMember]
        public double MenuToolBarWidth { get; set; }
        [DataMember]
        public int IconToolBarBand { get; set; }
        [DataMember]
        public int IconToolBarBandIndex { get; set; }
        [DataMember]
        public double IconToolBarWidth { get; set; }
        public Dictionary<string, KeyGesture> KeyBinds { get; set; }
        [DataMember]
        public Dictionary<string, string> keyBindsSerializeable;

        [DataMember]
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
            Settings settings = null;
            if (File.Exists(xmlPath))
            {
                // デシリアライズする
                var serializer = new DataContractSerializer(typeof(Settings));
                var xmlSettings = new XmlReaderSettings()
                {
                    CheckCharacters = false,
                };

                try
                {
                    using (var streamReader = new StreamReader(xmlPath, Encoding.UTF8))
                    using (var xmlReader = XmlReader.Create(streamReader, xmlSettings))
                    {
                        settings = (Settings)serializer.ReadObject(xmlReader);
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show("設定ファイルの読み込みに失敗しました\n\n" + e.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (settings != null)
                {
                    var kgvs = new KeyGestureValueSerializer();
                    settings.SetDefaultKeyBinds();

                    foreach (var kb in settings.keyBindsSerializeable)
                    {
                        if (kgvs.CanConvertFromString(kb.Value, null))
                        {
                            if (settings.KeyBinds.ContainsKey(kb.Key))
                            {
                                settings.KeyBinds[kb.Key] = (KeyGesture)kgvs.ConvertFromString(kb.Value, null);
                            }
                        }
                    }
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
            var kgvs = new KeyGestureValueSerializer();
            settings.keyBindsSerializeable = new Dictionary<string, string>();

            foreach(var kb in settings.KeyBinds)
            {
                if (kgvs.CanConvertToString(kb.Value, null))
                {
                    settings.keyBindsSerializeable.Add(kb.Key, kgvs.ConvertToString(kb.Value, null));
                }
            }
            // シリアライズする
            var serializer = new DataContractSerializer(typeof(Settings));
            var set = new XmlWriterSettings();
            set.CheckCharacters = false;
            set.Encoding = Encoding.UTF8;
            set.Indent = true;

            try
            {
                using (var xmlWriter = XmlWriter.Create(xmlPath, set))
                {
                    serializer.WriteObject(xmlWriter, settings);
                    xmlWriter.Flush();
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("設定ファイルの書き込みに失敗しました\n\n" + e.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetDefaultKeyBinds()
        {
            var dic = new Dictionary<string, KeyGesture>() {
                { "Play", new KeyGesture(Key.Space, ModifierKeys.None) },
                { "Pause", new KeyGesture(Key.Enter, ModifierKeys.None) },
                { "Stop", new KeyGesture(Key.Escape, ModifierKeys.None) },
                { "FF", new KeyGesture(Key.Right, ModifierKeys.None) },
                { "FR", new KeyGesture(Key.Left, ModifierKeys.None) },
                { "ToStart", new KeyGesture(Key.Left, ModifierKeys.Shift) },
                { "ToEnd", new KeyGesture(Key.Right, ModifierKeys.Shift) }
            };

            this.KeyBinds = dic;
        }
    }
}
