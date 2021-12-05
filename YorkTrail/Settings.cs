﻿using System;
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

        public bool IsInitialized { get; set; } = false;

        [DataMember]
        public float Volume { get; set; } = 1.0f;
        [DataMember]
        public int DeviceIndex { get; set; } = 0;
        [DataMember]
        public string DeviceName { get; set; }
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
        public Dictionary<string, ShortCutKey> KeyBinds { get; set; }
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
            set
            {
                _snapToTick = value;
                RaisePropertyChanged(nameof(SnapToTick));
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

        // 状態保存のためのプロパティ
        [DataMember]
        public bool RestoreLastState { get; set; } = false;
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public bool IsZooming { get; set; }
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
        public float Rate { get; set; }
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
        public bool IsSliderLinked { get; set; }
        [DataMember]
        public float Tempo { get; set; }
        [DataMember]
        public int MeasureOffset { get; set; }
        [DataMember]
        public int TimeSignature { get; set; } = 4;

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

                settings.SetDefaultKeyBinds();

                foreach (var kb in settings.KeyBindsSerializeable)
                {
                    if (settings.KeyBinds.ContainsKey(kb.Key))
                    {
                        settings.KeyBinds[kb.Key] = ShortCutKey.ConvertFromString(kb.Value);
                    }
                }
                // アップデートした時nullになるのでここで初期化
                if (settings.FilterPresets == null)
                {
                    settings.FilterPresets = new ObservableCollection<FilterPreset>();
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

        private void SetDefaultKeyBinds()
        {
            var dic = new Dictionary<string, ShortCutKey>() {
                { "Play",  new ShortCutKey(Key.Space, ModifierKeys.None) },
                { "Pause", new ShortCutKey(Key.Enter, ModifierKeys.None) },
                { "Stop", new ShortCutKey(Key.Escape, ModifierKeys.None) },
                { "FF", new ShortCutKey(Key.Right, ModifierKeys.None) },
                { "FR", new ShortCutKey(Key.Left, ModifierKeys.None) },
                { "ToStart", new ShortCutKey(Key.Left, ModifierKeys.Shift) },
                { "ToEnd", new ShortCutKey(Key.Right, ModifierKeys.Shift) },
                { "CurrentToStartPosition", new ShortCutKey(Key.F9, ModifierKeys.None) },
                { "CurrentToEndPosition", new ShortCutKey(Key.F10, ModifierKeys.None) },
                { "Stereo", new ShortCutKey(Key.S, ModifierKeys.None) },
                { "Mono", new ShortCutKey(Key.M, ModifierKeys.None) },
                { "LOnly", new ShortCutKey(Key.L, ModifierKeys.None) },
                { "ROnly", new ShortCutKey(Key.R, ModifierKeys.None) },
                { "LMinusR", new ShortCutKey(Key.M, ModifierKeys.Shift) },
                { "LpfOn", new ShortCutKey(Key.D0, ModifierKeys.None) },
                { "HpfOn", new ShortCutKey(Key.D1, ModifierKeys.None) },
                { "BpfOn", new ShortCutKey(Key.D2, ModifierKeys.None) },
                { "PitchQuad", new ShortCutKey(Key.F2, ModifierKeys.Shift) },
                { "PitchDouble", new ShortCutKey(Key.F2, ModifierKeys.None) },
                { "PitchNormal", new ShortCutKey(Key.F3, ModifierKeys.None) },
                { "PitchHalf", new ShortCutKey(Key.F4, ModifierKeys.None) },
                { "TempoDouble", new ShortCutKey(Key.F5, ModifierKeys.None) },
                { "TempoNormal", new ShortCutKey(Key.F6, ModifierKeys.None) },
                { "TempoHalf", new ShortCutKey(Key.F7, ModifierKeys.None) },
                { "TempoOneThird", new ShortCutKey(Key.F8, ModifierKeys.None) },
                { "TempoQuarter", new ShortCutKey(Key.F8, ModifierKeys.Shift) },
                { "Bypass", new ShortCutKey(Key.B, ModifierKeys.None) },
                { "FileOpen", new ShortCutKey(Key.O, ModifierKeys.Control) },
                { "FileClose", new ShortCutKey(Key.None, ModifierKeys.None) },
                { "Loop", new ShortCutKey(Key.L, ModifierKeys.Control) },
                { "SelectionReset", new ShortCutKey(Key.I, ModifierKeys.Control) },
                { "Zoom", new ShortCutKey(Key.X, ModifierKeys.Control) },
                { "OpenTempoCalcWindow", new ShortCutKey(Key.T, ModifierKeys.Alt) },
                { "AlwaysOnTop", new ShortCutKey(Key.T, ModifierKeys.Control) },
                { "ShowTimeAtMeasure", new ShortCutKey(Key.None, ModifierKeys.None) },
                { "SnapToTick", new ShortCutKey(Key.None, ModifierKeys.None) },
                { "Exit", new ShortCutKey(Key.Q, ModifierKeys.Control) },
            };

            this.KeyBinds = dic;
        }

        private static Dictionary<string, string> KeyBindsToSerializeable(Dictionary<string, ShortCutKey> dic)
        {
            var ret = new Dictionary<string, string>();

            foreach (var kb in dic)
            {
                ret.Add(kb.Key, ShortCutKey.ConvertToString(kb.Value));
            }
            return ret;
        }
    }
}
