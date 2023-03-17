/*
    YorkTrail
    Copyright (C) 2021 theta

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
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
            KeyBinds = DefaultKey.GetKeyBinds();
            KeyBindsSerializeable = new Dictionary<string, string>();
            MarkerList = new ObservableCollection<double>();
        }

        public bool IsInitialized { get; set; } = false;

        [DataMember]
        public float Volume { get; set; } = 1.0f;
        [DataMember]
        public int DeviceIndex { get; set; } = 0;
        [DataMember]
        public string DeviceName { get; set; } = "";
        [DataMember]
        public ObservableCollection<string> RecentFiles { get; set; } = new ObservableCollection<string>();
        [DataMember]
        public ObservableCollection<FilterPreset> FilterPresets { get; set; } = new ObservableCollection<FilterPreset>();
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
        public Dictionary<CommandName, ShortCutKey> KeyBinds { get; set; }
        [DataMember]
        public Dictionary<string, string> KeyBindsSerializeable { get; set; }
        [DataMember]
        public double WindowTop { get; set; }
        [DataMember]
        public double WindowLeft { get; set; }

        private bool _alwaysOnTop;
        [DataMember]
        public bool AlwaysOnTop {
            get { return _alwaysOnTop; }
            set {
                _alwaysOnTop = value;
                RaisePropertyChanged(nameof(AlwaysOnTop));
            }
        }

        private bool _showTimeAtMeasure;
        [DataMember]
        public bool ShowTimeAtMeasure {
            get { return _showTimeAtMeasure; }
            set {
                _showTimeAtMeasure = value;
                RaisePropertyChanged(nameof(ShowTimeAtMeasure));
            }
        }

        private bool _snapToTick;
        [DataMember]
        public bool SnapToTick
        {
            get { return _snapToTick; }
            set {
                _snapToTick = value;
                RaisePropertyChanged(nameof(SnapToTick));
            }
        }

        private bool _isSliderLinked;
        [DataMember]
        public bool IsSliderLinked
        {
            get { return _isSliderLinked; }
            set {
                _isSliderLinked = value;
                RaisePropertyChanged(nameof(IsSliderLinked));
            }
        }

        [DataMember]
        public int SkipLengthMS { get; set; } = 2000;
        [DataMember]
        public int SoundTouchSequenceMS { get; set; } = 80;
        [DataMember]
        public int SoundTouchSeekWindowMS { get; set; } = 30;
        [DataMember]
        public int SoundTouchOverlapMS { get; set; } = 8;
        [DataMember]
        public StretchMethod StretchMethod { get; set; } = StretchMethod.RubberBand;
        [DataMember]
        public bool ShowWaveForm { get; set; } = true;
        [DataMember]
        public bool IsStemWindowExpanded { get; set; } = false;

        // 状態保存のためのプロパティ
        [DataMember]
        public bool RestoreLastState { get; set; } = false;
        [DataMember]
        public string FilePath { get; set; } = "";
        [DataMember]
        public double Position { get; set; }
        [DataMember]
        public double StartPosition { get; set; }
        [DataMember]
        public double EndPosition { get; set; }
        [DataMember]
        public Channels Channels { get; set; }
        [DataMember]
        public float Pitch { get; set; }
        [DataMember]
        public float Ratio { get; set; }
        [DataMember]
        public bool IsBypass { get; set; }
        [DataMember]
        public bool IsLoop { get; set; }
        [DataMember]
        public bool LpfEnabled { get; set; }
        [DataMember]
        public bool HpfEnabled { get; set; }
        [DataMember]
        public bool BpfEnabled { get; set; }
        [DataMember]
        public float LpfFreq { get; set; }
        [DataMember]
        public float HpfFreq { get; set; }
        [DataMember]
        public float BpfFreq { get; set; }
        [DataMember]
        public float Tempo { get; set; }
        [DataMember]
        public int MeasureOffset { get; set; }
        [DataMember]
        public int TimeSignature { get; set; }
        [DataMember]
        public double SeekBarMinimum { get; set; }
        [DataMember]
        public double SeekBarMaximum { get; set; }
        [DataMember]
        public int ZoomMultiplier { get; set; }
        [DataMember]
        public ObservableCollection<double> MarkerList { get; set; }
        [DataMember]
        public bool IsStemPlaying { get; set; }
        [DataMember]
        public float VocalsVolume { get; set; }
        [DataMember]
        public float DrumsVolume { get; set; }
        [DataMember]
        public float BassVolume { get; set; }
        [DataMember]
        public float PianoVolume { get; set; }
        [DataMember]
        public float OtherVolume { get; set; }
        [DataMember]
        public bool VocalsMute { get; set; }
        [DataMember]
        public bool DrumsMute { get; set; }
        [DataMember]
        public bool BassMute { get; set; }
        [DataMember]
        public bool PianoMute { get; set; }
        [DataMember]
        public bool OtherMute { get; set; }
        [DataMember]
        public bool VocalsSolo { get; set; }
        [DataMember]
        public bool DrumsSolo { get; set; }
        [DataMember]
        public bool BassSolo { get; set; }
        [DataMember]
        public bool PianoSolo { get; set; }
        [DataMember]
        public bool OtherSolo { get; set; }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private const string xmlPath = @".\Settings.xml";
        public static Settings? ReadSettingsFromFile()
        {
            Settings? settings = null;

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
                        settings = (Settings?)serializer.ReadObject(xmlReader);
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show("設定ファイルの読み込みに失敗しました\n\n" + e.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (settings != null)
                {
                    settings.KeyBinds = DefaultKey.GetKeyBinds();

                    foreach (var kb in settings.KeyBindsSerializeable)
                    {
                        CommandName cmd;
                        if (Enum.TryParse(kb.Key, out cmd) && Enum.IsDefined(typeof(CommandName), cmd))
                        {
                            if (settings.KeyBinds.ContainsKey(cmd))
                            {
                                settings.KeyBinds[cmd] = ShortCutKey.ConvertFromString(kb.Value);
                            }
                        }

                    }
                    // アップデートした時nullになるのでここで初期化
                    if (settings.FilterPresets == null)
                    {
                        settings.FilterPresets = new ObservableCollection<FilterPreset>();
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
            settings.KeyBindsSerializeable = KeyBindsToSerializeable(settings.KeyBinds);

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

        private static Dictionary<string, string> KeyBindsToSerializeable(Dictionary<CommandName, ShortCutKey> dic)
        {
            var ret = new Dictionary<string, string>();

            foreach (var kb in dic)
            {
                ret.Add(kb.Key.ToString(), ShortCutKey.ConvertToString(kb.Value));
            }
            return ret;
        }

        public System.Collections.ICollection GetKeyBindings()
        {
            var collection = new List<KeyBinding>();
            foreach (var binds in KeyBinds)
            {
                var cn = binds.Key;
                var cmd = DefaultKey.KeyCommands[cn].Command;
                var param = DefaultKey.KeyCommands[cn].CommandParameter;
                collection.Add(new KeyBinding() { Command = cmd, CommandParameter = param, Key = KeyBinds[cn].Key, Modifiers = KeyBinds[cn].Modifiers });
            }
            return collection;
        }

    }
}
