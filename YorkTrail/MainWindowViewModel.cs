﻿/*
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Timers;
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Xml.Linq;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Windows.Data;
using System.Reflection;

namespace YorkTrail
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            applicationName = Assembly.GetExecutingAssembly().GetName().Name ?? "YorkTrail";
            playerTask = new Task(() => { });
            _windowTitle = applicationName;
            _statusText = "";
            _startPosition = 0.0;
            _endPosition = 1.0;
            _timeSignature = 4;
            _isStemSeparated = false;
            _isStemPlaying = false;
            _vocalsVolume = 1.0f;
            _drumsVolume = 1.0f;
            _bassVolume = 1.0f;
            _pianoVolume = 1.0f;
            _otherVolume = 1.0f;
            _timeDisplayOpacity = 1.0;
            _seekbarMinimum = 0.0;
            _seekbarMaximum = 1.0;
            FilePath = "";
            MarkerList = new ObservableCollection<double>();
            VolumeList = new ObservableCollection<float>();
            BindingOperations.EnableCollectionSynchronization(VolumeList, _lockObject);

            DeviceList = Core.GetPlaybackDeviceList();
            Settings = Settings.ReadSettingsFromFile();

            if (Settings != null)
            {
                // 無条件で復元する
                Volume = Settings.Volume;
                StretchMethod = Settings.StretchMethod;
                Core.SetSoundTouchParam(Settings.SoundTouchSequenceMS, Settings.SoundTouchSeekWindowMS, Settings.SoundTouchOverlapMS);

                // デバイス構成が前回と違っていたら復元しない
                if (Settings.DeviceName == DeviceList[Settings.DeviceIndex])
                {
                    PlaybackDevice = Settings.DeviceIndex;
                }
            }
            else
            {
                Settings = new Settings();
                Settings.IsInitialized = true;
            }

            Core.NotifyTimeChanged += () => {
                RaisePropertyChanged(nameof(TimeString));
                RaisePropertyChanged(nameof(Position));
                RaisePropertyChanged(nameof(RMSL));
                RaisePropertyChanged(nameof(RMSR));
            };

            BlinkTimer = new Timer(800);
            BlinkTimer.Elapsed += (sender, e) =>
            {
                if (TimeDisplayOpacity == 1.0)
                {
                    TimeDisplayOpacity = 0.1;
                }
                else if (TimeDisplayOpacity == 0.1)
                {
                    TimeDisplayOpacity = 1.0;
                }
            };
        }

        private string applicationName;

        public Timer BlinkTimer;
        private Task playerTask;
        //private Stopwatch _sw;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string FilePath { get; set; }
        public int ZoomMultiplier { get; set; }
        public ObservableCollection<double> MarkerList { get; set; }
        public ObservableCollection<float> VolumeList { get; set; }
        private object _lockObject = new object();

        private string _windowTitle;
        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                RaisePropertyChanged(nameof(WindowTitle));
            }
        }

        public ulong Time
        {
            get { return Core.GetTime(); }
        }

        public string TimeString {
            get {
                long cur = (long)Core.GetTime();
                if (Settings?.ShowTimeAtMeasure ?? false)
                {
                    long offsetCur = cur - MeasureOffset;
                    float msOfBeat = 60000.0f / Tempo;
                    if (msOfBeat <= 0 || TimeSignature <= 0)
                        return "0000:00:00";
                    float msOfMeasure = msOfBeat * TimeSignature;
                    int measure = (int)(offsetCur / msOfMeasure);
                    int beat = (offsetCur < 0) ? (int)(offsetCur / msOfBeat % TimeSignature) : (int)(offsetCur / msOfBeat % TimeSignature + 1);
                    int ms = (int)(cur % msOfBeat / 10);
                    ms = ms % 100;
                    return string.Format(@"{0,3:0000}:{1,2:00}:{2,2:00}", measure, beat, ms);
                }
                else
                {
                    return TimeSpan.FromMilliseconds(cur).ToString(@"hh\:mm\:ss\.ff");
                }
            }
        }

        public ulong TotalMilliSeconds
        { 
            get { return Core.GetTotalMilliSeconds(); }
        }

        public double Position
        {
            get { return Core.GetPosition(); }
            set {
                Core.SetPosition(value);
                RaisePropertyChanged(nameof(Position));
            }
        }

        public int PlaybackDevice
        {
            get { return Core.GetPlaybackDevice(); }
            set {
                if (Settings != null)
                {
                    Settings.DeviceIndex = value;
                    Settings.DeviceName = DeviceList[value];
                }
                SetPlaybackDevice(value);
                RaisePropertyChanged(nameof(PlaybackDevice));
            }
        }

        private string _statusText;
        public string StatusText
        {
            get { return _statusText; }
            set {
                _statusText = value;
                RaisePropertyChanged(nameof(StatusText));
            }
        }
        
        private double _startPosition;
        public double StartPosition
        {
            get { return _startPosition; }
            set {
                _startPosition = value;
                Core.SetStartPosition(value);
                RaisePropertyChanged(nameof(StartPosition));
            }
        }
        
        private double _endPosition;
        public double EndPosition
        {
            get { return _endPosition; }
            set {
                _endPosition = value;
                Core.SetEndPosition(value);
                RaisePropertyChanged(nameof(EndPosition));
            }
        }

        public Channels Channels
        {
            get { return Core.GetChannels(); }
            set {
                Core.SetChannels(value);
                RaisePropertyChanged(nameof(Channels));
            }
        }
        public float Pitch
        {
            get { return Core.GetPitch(); }
            set {
                if (value > 0.0f)
                {
                    Core.SetPitch(value);
                    RaisePropertyChanged(nameof(Pitch));
                }
            }
        }
        
        public float Ratio
        {
            get { return Core.GetRatio(); }
            set {
                if (value > 0.0f)
                {
                    Core.SetRatio(value);
                    RaisePropertyChanged(nameof(Ratio));
                }
            }
        }

        public bool IsBypass
        {
            get { return Core.GetBypass(); }
            set {
                Core.SetBypass(value);
                RaisePropertyChanged(nameof(IsBypass));
            }
        }
        public bool IsLoop
        {
            get { return Core.GetLoop(); }
            set {
                Core.SetLoop(value);
                RaisePropertyChanged(nameof(IsLoop));
            }
        }
        
        public float Volume
        {
            get { return Core.GetVolume(); }
            set {
                if (Settings != null)
                {
                    Settings.Volume = value;
                }
                Core.SetVolume(value);
            }
        }

        private float _vocalsVolume;
        public float VocalsVolume
        {
            get {  return  _vocalsVolume; }
            set {
                _vocalsVolume = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(VocalsVolume));
            }
        }

        private float _drumsVolume;
        public float DrumsVolume
        {
            get { return _drumsVolume; }
            set
            {
                _drumsVolume = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(DrumsVolume));
            }
        }

        private float _bassVolume;
        public float BassVolume
        {
            get { return _bassVolume; }
            set
            {
                _bassVolume = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(BassVolume));
            }
        }

        private float _pianoVolume;
        public float PianoVolume
        {
            get { return _pianoVolume; }
            set
            {
                _pianoVolume = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(PianoVolume));
            }
        }

        private float _otherVolume;
        public float OtherVolume
        {
            get { return _otherVolume; }
            set
            {
                _otherVolume = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(OtherVolume));
            }
        }

        private bool _vocalsMute;
        public bool VocalsMute
        {
            get { return _vocalsMute; }
            set
            {
                _vocalsMute = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(VocalsMute));
            }
        }

        private bool _drumsMute;
        public bool DrumsMute
        {
            get { return _drumsMute; }
            set
            {
                _drumsMute = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(DrumsMute));
            }
        }

        private bool _bassMute;
        public bool BassMute
        {
            get { return _bassMute; }
            set
            {
                _bassMute = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(BassMute));
            }
        }

        private bool _pianoMute;
        public bool PianoMute
        {
            get { return _pianoMute; }
            set
            {
                _pianoMute = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(PianoMute));
            }
        }

        private bool _otherMute;
        public bool OtherMute
        {
            get { return _otherMute; }
            set
            {
                _otherMute = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(OtherMute));
            }
        }

        private bool _vocalsSolo;
        public bool VocalsSolo
        {
            get { return _vocalsSolo; }
            set
            {
                _vocalsSolo = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(VocalsSolo));
            }
        }

        private bool _drumsSolo;
        public bool DrumsSolo
        {
            get { return _drumsSolo; }
            set
            {
                _drumsSolo = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(DrumsSolo));
            }
        }

        private bool _bassSolo;
        public bool BassSolo
        {
            get { return _bassSolo; }
            set
            {
                _bassSolo = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(BassSolo));
            }
        }

        private bool _pianoSolo;
        public bool PianoSolo
        {
            get { return _pianoSolo; }
            set
            {
                _pianoSolo = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(PianoSolo));
            }
        }

        private bool _otherSolo;
        public bool OtherSolo
        {
            get { return _otherSolo; }
            set
            {
                _otherSolo = value;
                SetStemVolumes();
                RaisePropertyChanged(nameof(OtherSolo));
            }
        }

        public StretchMethod StretchMethod
        {
            get { return Core.GetStretchMethod(); }
            set {
                if (Settings != null)
                {
                    Settings.StretchMethod = value;
                }
                Core.SetStretchMethod(value);
            }
        }

        private float _tempo;
        public float Tempo
        {
            get { return _tempo; }
            set {
                _tempo = value;
                RaisePropertyChanged(nameof(Tempo));
            }
        }

        private int _measureOffset;
        public int MeasureOffset
        {
            get { return _measureOffset; }
            set {
                _measureOffset = value;
                RaisePropertyChanged(nameof(MeasureOffset));
            }
        }

        private int _timeSignature;
        public int TimeSignature
        {
            get { return _timeSignature; }
            set
            {
                _timeSignature = value;
                RaisePropertyChanged(nameof(TimeSignature));
            }
        }

        private bool _isStemSeparated;
        public bool IsStemSeparated
        {
            get { return _isStemSeparated; }
            set
            {
                _isStemSeparated = value;
                RaisePropertyChanged(nameof(IsStemSeparated));
            }
        }

        private bool _isStemSeparating;
        public bool IsStemSeparating
        {
            get { return _isStemSeparating; }
            set
            {
                _isStemSeparating = value;
                RaisePropertyChanged(nameof(IsStemSeparating));
            }
        }

        private double _separateProgress;
        public double SeparateProgress
        {
            get { return _separateProgress; }
            set
            {
                _separateProgress = value;
                RaisePropertyChanged(nameof(SeparateProgress));
            }
        }

        private bool _isStemPlaying;
        public bool IsStemPlaying
        {
            get { return _isStemPlaying; }
            set
            {
                _isStemPlaying = value;
                RaisePropertyChanged(nameof(IsStemPlaying));
            }
        }

        private double _timeDisplayOpacity;
        public double TimeDisplayOpacity
        {
            get { return _timeDisplayOpacity; }
            set
            {
                _timeDisplayOpacity = value;
                RaisePropertyChanged(nameof(TimeDisplayOpacity));
            }
        }

        private double _seekbarMinimum;
        public double SeekbarMinimum
        {
            get { return _seekbarMinimum; }
            set
            {
                _seekbarMinimum = value;
                RaisePropertyChanged(nameof(SeekbarMinimum));
            }
        }

        private double _seekbarMaximum;
        public double SeekbarMaximum
        {
            get { return _seekbarMaximum; }
            set
            {
                _seekbarMaximum = value;
                RaisePropertyChanged(nameof(SeekbarMaximum));
            }
        }

        public YorkTrailCore Core { get; private set; } = YorkTrailHandleHolder.GetYorkTrailCore();
        public float RMSL { get { return Core.GetRmsL(); } }
        public float RMSR { get { return Core.GetRmsR(); } }
        public bool LpfEnabled { get { return Core.GetLpfEnabled(); } set { Core.SetLpfEnabled(value); RaisePropertyChanged(nameof(LpfEnabled)); } }
        public bool HpfEnabled { get { return Core.GetHpfEnabled(); } set { Core.SetHpfEnabled(value); RaisePropertyChanged(nameof(HpfEnabled)); } }
        public bool BpfEnabled { get { return Core.GetBpfEnabled(); }  set { Core.SetBpfEnabled(value); RaisePropertyChanged(nameof(BpfEnabled)); } }
        public float LpfFreq { get { return Core.GetLpfFreq(); } set { Core.SetLpfFreq(value); RaisePropertyChanged(nameof(LpfFreq)); } }
        public float HpfFreq { get { return Core.GetHpfFreq(); } set { Core.SetHpfFreq(value); RaisePropertyChanged(nameof(HpfFreq)); } }
        public float BpfFreq { get { return Core.GetBpfFreq(); } set { Core.SetBpfFreq(value); RaisePropertyChanged(nameof(BpfFreq)); } }
        public bool IsFileLoaded { get { return Core.IsFileLoaded(); } }
        public State State { get { return Core.GetState(); } }
        public List<string> DeviceList { get; private set; }
        public Settings? Settings { get; private set; }

        public MainWindow? Window { get; set; }
        public TempoCalcWindow? TempoCalcWindow { get; set; }

        public PlayCommand PlayCommand { get; private set; } = (PlayCommand)CommandCollection.Get(nameof(PlayCommand));
        public StopCommand StopCommand { get; private set; } = (StopCommand)CommandCollection.Get(nameof(StopCommand));
        public PauseCommand PauseCommand { get; private set; } = (PauseCommand)CommandCollection.Get(nameof(PauseCommand));
        public FFCommand FFCommand { get; private set; } = (FFCommand)CommandCollection.Get(nameof(FFCommand));
        public FRCommand FRCommand { get; private set; } = (FRCommand)CommandCollection.Get(nameof(FRCommand));
        public ToPrevMarkerCommand ToPrevMarkerCommand { get; private set; } = (ToPrevMarkerCommand)CommandCollection.Get(nameof(ToPrevMarkerCommand));
        public ToNextMarkerCommand ToNextMarkerCommand { get; private set; } = (ToNextMarkerCommand)CommandCollection.Get(nameof(ToNextMarkerCommand));
        public ZoomInCommand ZoomInCommand { get; private set; } = (ZoomInCommand)CommandCollection.Get(nameof(ZoomInCommand));
        public ZoomOutCommand ZoomOutCommand { get; private set; } = (ZoomOutCommand)CommandCollection.Get(nameof(ZoomOutCommand));
        public LpfOnCommand LpfOnCommand { get; private set; } = (LpfOnCommand)CommandCollection.Get(nameof(LpfOnCommand));
        public HpfOnCommand HpfOnCommand { get; private set; } = (HpfOnCommand)CommandCollection.Get(nameof(HpfOnCommand));
        public BpfOnCommand BpfOnCommand { get; private set; } = (BpfOnCommand)CommandCollection.Get(nameof(BpfOnCommand));
        public BypassCommand BypassCommand { get; private set; } = (BypassCommand)CommandCollection.Get(nameof(BypassCommand));
        public FileOpenCommand FileOpenCommand { get; private set; } = (FileOpenCommand)CommandCollection.Get(nameof(FileOpenCommand));
        public OpenTempoCalcWindowCommand OpenTempoCalcWindowCommand { get; private set; } = (OpenTempoCalcWindowCommand)CommandCollection.Get(nameof(OpenTempoCalcWindowCommand));
        public FileCloseCommand FileCloseCommand { get; private set; } = (FileCloseCommand)CommandCollection.Get(nameof(FileCloseCommand));
        public ExitCommand ExitCommand { get; private set; } = (ExitCommand)CommandCollection.Get(nameof(ExitCommand));
        public SelectionResetCommand SelectionResetCommand { get; private set; } = (SelectionResetCommand)CommandCollection.Get(nameof(SelectionResetCommand));
        public ShowAboutCommand ShowAboutCommand { get; private set; } = (ShowAboutCommand)CommandCollection.Get(nameof(ShowAboutCommand));
        public CurrentToStartPositionCommand CurrentToStartPositionCommand { get; private set; } = (CurrentToStartPositionCommand)CommandCollection.Get(nameof(CurrentToStartPositionCommand));
        public CurrentToEndPositionCommand CurrentToEndPositionCommand { get; private set; } = (CurrentToEndPositionCommand)CommandCollection.Get(nameof(CurrentToEndPositionCommand));
        public OpenKeyCustomizeCommand OpenKeyCustomizeCommand { get; private set; } = (OpenKeyCustomizeCommand)CommandCollection.Get(nameof(OpenKeyCustomizeCommand));
        public OpenSettingWindowCommand OpenSettingWindowCommand { get; private set; } = (OpenSettingWindowCommand)CommandCollection.Get(nameof(OpenSettingWindowCommand));
        public OpenAddFilterPresetWindowCommand OpenAddFilterPresetWindowCommand { get; private set; } = (OpenAddFilterPresetWindowCommand)CommandCollection.Get(nameof(OpenAddFilterPresetWindowCommand));
        public FilterPresetDeleteCommand FilterPresetDeleteCommand { get; private set; } = (FilterPresetDeleteCommand)CommandCollection.Get(nameof(FilterPresetDeleteCommand));
        public FilterPresetMoveUpCommand FilterPresetMoveUpCommand { get; private set; } = (FilterPresetMoveUpCommand)CommandCollection.Get(nameof(FilterPresetMoveUpCommand));
        public FilterPresetMoveDownCommand FilterPresetMoveDownCommand { get; private set; } = (FilterPresetMoveDownCommand)CommandCollection.Get(nameof(FilterPresetMoveDownCommand));
        public FilterPresetRenameCommand FilterPresetRenameCommand { get; private set; } = (FilterPresetRenameCommand)CommandCollection.Get(nameof(FilterPresetRenameCommand));
        public AddMarkerCommand AddMarkerCommand { get; private set; } = (AddMarkerCommand)CommandCollection.Get(nameof(AddMarkerCommand));
        public ClearMarkerCommand ClearMarkerCommand { get; private set; } = (ClearMarkerCommand)CommandCollection.Get(nameof(ClearMarkerCommand));
        public StemSeparateCommand StemSeparateCommand { get; private set; } = (StemSeparateCommand)CommandCollection.Get(nameof(StemSeparateCommand));
        public DeleteStemFilesCommand DeleteStemFilesCommand { get; private set; } = (DeleteStemFilesCommand)CommandCollection.Get(nameof(DeleteStemFilesCommand));
        public CancelProcessingCommand CancelProcessingCommand { get; private set; } = (CancelProcessingCommand)CommandCollection.Get(nameof(CancelProcessingCommand));
        public SwitchDecoderToSourceCommand SwitchDecoderToSourceCommand { get; private set; } = (SwitchDecoderToSourceCommand)CommandCollection.Get(nameof(SwitchDecoderToSourceCommand));
        public SwitchDecoderToStemsCommand SwitchDecoderToStemsCommand { get; private set; } = (SwitchDecoderToStemsCommand)CommandCollection.Get(nameof(SwitchDecoderToStemsCommand));
        public ImportStemsCommand ImportStemsCommand { get; private set; } = (ImportStemsCommand)CommandCollection.Get(nameof(ImportStemsCommand));

        public async void SetPlaybackDevice(int index)
        {
            await Task.Run(() => { Core.SetPlaybackDevice(index); });
        }

        public void DisplayUpdate()
        {
            RaisePropertyChanged(nameof(TimeString));
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(RMSL));
            RaisePropertyChanged(nameof(RMSR));
        }

        public void SetKeyBinds()
        {
            Window?.InputBindings.Clear();
            Window?.InputBindings.AddRange(Settings?.GetKeyBindings());
        }

        public void SaveWindowSettings()
        {
            if (Settings != null && Window != null)
            {
                Settings.WindowTop = Window.Top;
                Settings.WindowLeft = Window.Left;
                Settings.WindowHeight = Window.ActualHeight;
                Settings.MenuToolBarBand = Window.MenuToolBar.Band;
                Settings.MenuToolBarBandIndex = Window.MenuToolBar.BandIndex;
                Settings.MenuToolBarWidth = Window.MenuToolBar.ActualWidth;
                Settings.IconToolBarBand = Window.IconToolBar.Band;
                Settings.IconToolBarBandIndex = Window.IconToolBar.BandIndex;
                Settings.IconToolBarWidth = Window.IconToolBar.ActualWidth;
                Settings.IsStemWindowExpanded = Window.StemWindowExpander.IsExpanded;
            }
        }

        public void RestoreWindowSettings()
        {
            if (Settings != null && Window != null)
            {
                Window.Top = Settings.WindowTop;
                Window.Left = Settings.WindowLeft;
                Window.Height = Settings.WindowHeight;
                Window.MenuToolBar.Band = Settings.MenuToolBarBand;
                Window.MenuToolBar.BandIndex = Settings.MenuToolBarBandIndex;
                Window.MenuToolBar.Width = Settings.MenuToolBarWidth;
                Window.IconToolBar.Band = Settings.IconToolBarBand;
                Window.IconToolBar.BandIndex = Settings.IconToolBarBandIndex;
                Window.IconToolBar.Width = Settings.IconToolBarWidth;
                Window.StemWindowExpander.IsExpanded = Settings.IsStemWindowExpanded;
            }
        }

        public void SaveState()
        {
            if (Settings != null)
            {
                Settings.FilePath = FilePath;
                Settings.Position = Position;
                Settings.StartPosition = StartPosition;
                Settings.EndPosition = EndPosition;
                Settings.Channels = Channels;
                Settings.Pitch = Pitch;
                Settings.Ratio = Ratio;
                Settings.IsBypass = IsBypass;
                Settings.IsLoop = IsLoop;
                Settings.LpfEnabled = LpfEnabled;
                Settings.HpfEnabled = HpfEnabled;
                Settings.BpfEnabled = BpfEnabled;
                Settings.LpfFreq = LpfFreq;
                Settings.HpfFreq = HpfFreq;
                Settings.BpfFreq = BpfFreq;
                Settings.Tempo = Tempo;
                Settings.MeasureOffset = MeasureOffset;
                Settings.TimeSignature = TimeSignature;
                Settings.SeekbarMinimum = SeekbarMinimum;
                Settings.SeekbarMaximum = SeekbarMaximum;
                Settings.ZoomMultiplier = ZoomMultiplier;
                Settings.MarkerList = MarkerList;
                Settings.IsStemPlaying = IsStemPlaying;
                Settings.VocalsVolume = VocalsVolume;
                Settings.DrumsVolume = DrumsVolume;
                Settings.BassVolume = BassVolume;
                Settings.PianoVolume = PianoVolume;
                Settings.OtherVolume = OtherVolume;
                Settings.VocalsMute = VocalsMute;
                Settings.DrumsMute = DrumsMute;
                Settings.BassMute = BassMute;
                Settings.PianoMute = PianoMute;
                Settings.OtherMute = OtherMute;
                Settings.VocalsSolo = VocalsSolo;
                Settings.DrumsSolo = DrumsSolo;
                Settings.BassSolo = BassSolo;
                Settings.PianoSolo = PianoSolo;
                Settings.OtherSolo = OtherSolo;
            }
        }

        public async void ResotreState()
        {
            if (Settings != null && Window != null)
            {
                if (Settings.FilePath != "" && await FileOpen(Settings.FilePath))
                {
                    Position = Settings.Position;
                    StartPosition = Settings.StartPosition;
                    EndPosition = Settings.EndPosition;
                    Tempo = Settings.Tempo;
                    MeasureOffset = Settings.MeasureOffset;
                    TimeSignature = (Settings.TimeSignature == 0) ? 4 : Settings.TimeSignature;
                    SeekbarMinimum = Settings.SeekbarMinimum;
                    SeekbarMaximum = Settings.SeekbarMaximum;
                    ZoomMultiplier = Settings.ZoomMultiplier;
                    StaticMethods.ShallowCopy(Settings.MarkerList, MarkerList);
                    IsStemPlaying = Settings.IsStemPlaying;
                    if (IsStemPlaying)
                        await SwitchDecoderToStems();
                    VocalsVolume = Settings.VocalsVolume;
                    DrumsVolume = Settings.DrumsVolume;
                    BassVolume = Settings.BassVolume;
                    PianoVolume = Settings.PianoVolume;
                    OtherVolume = Settings.OtherVolume;
                    VocalsMute = Settings.VocalsMute;
                    DrumsMute = Settings.DrumsMute;
                    BassMute = Settings.BassMute;
                    PianoMute = Settings.PianoMute;
                    OtherMute = Settings.OtherMute;
                    VocalsSolo = Settings.VocalsSolo;
                    DrumsSolo = Settings.DrumsSolo;
                    BassSolo = Settings.BassSolo;
                    PianoSolo = Settings.PianoSolo;
                    OtherSolo = Settings.OtherSolo;
                    RaisePropertyChanged(nameof(Time));
                }
                Channels = Settings.Channels;
                Pitch = (Settings.Pitch == 0) ? 1 : Settings.Pitch;
                Ratio = (Settings.Ratio == 0) ? 1 : Settings.Ratio;
                IsBypass = Settings.IsBypass;
                IsLoop = Settings.IsLoop;
                LpfEnabled = Settings.LpfEnabled;
                HpfEnabled = Settings.HpfEnabled;
                BpfEnabled = Settings.BpfEnabled;
                LpfFreq = Settings.LpfFreq;
                HpfFreq = Settings.HpfFreq;
                BpfFreq = Settings.BpfFreq;
            }
        }

        public void AddFilterPreset(string name)
        {
            var p = new FilterPreset(name, LpfEnabled, HpfEnabled, BpfEnabled, LpfFreq, HpfFreq, BpfFreq);
            Settings?.FilterPresets.Add(p);
        }

        public async Task<bool> FileOpen(string path)
        {
            if (File.Exists(path))
            {
                FileType type;
                string ext = Path.GetExtension(path);
                ext = ext.ToLower();

                using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] buff = new byte[4];
                    fs.Read(buff, 0, 4);
                    string head = Encoding.ASCII.GetString(buff);

                    switch (head)
                    {
                        case "RIFF":
                            type = FileType.Wav;
                            break;
                        case "TAG":
                        case "ID3":
                            type = FileType.Mp3;
                            break;
                        case "fLaC":
                            type = FileType.Flac;
                            break;
                        default:
                            type = FileType.Unknown;
                            break;
                    }

                    if (type == FileType.Unknown)
                    {
                        switch(ext)
                        {
                            case ".wav":
                                type = FileType.Wav;
                                break;
                            case ".mp3":
                                type = FileType.Mp3;
                                break;
                            case ".flac":
                                type = FileType.Flac;
                                break;
                            default:
                                type = FileType.Unknown;
                                break;
                        }
                    }
                }

                if (type == FileType.Unknown)
                {
                    MessageBox.Show("未対応のファイル形式です", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }

                if (Settings != null)
                {
                    if (Settings.RecentFiles.Contains(path))
                    {
                        Settings.RecentFiles.Remove(path);
                        Settings.RecentFiles.Insert(0, path);
                    }
                    else
                    {
                        Settings.RecentFiles.Insert(0, path);
                        if (Settings.RecentFiles.Count > 10)
                        {
                            Settings.RecentFiles.RemoveAt(10);
                        }
                    }
                }

                if (IsStemSeparated)
                {
                    IsStemSeparated = false;
                    Core.StemFilesClose();
                }

                await FileClose();

                if (!Core.FileOpen(path, type))
                {
                    MessageBox.Show("ファイルを開けませんでした\r\n" + path, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    await FileClose();
                    return false;
                }

                WindowTitle = applicationName + " - " + Path.GetFileName(path);
                StatusText = Core.GetFileInfo();
                RaisePropertyChanged(nameof(IsFileLoaded));
                RaisePropertyChanged(nameof(TotalMilliSeconds));
                FilePath = path;

                if (Settings != null && Settings.ShowWaveForm)
                {
                    VolumeList.Clear();
                    Task.Run(GetVolumeList);
                }
                else
                {
                    VolumeList.Clear();
                }

                var dir = GetStemDir(path);
                if (FindStemFiles(dir))
                {
                    if (Core.StemFilesOpen(dir))
                    {
                        IsStemSeparated = true;
                    }
                    else
                    {
                        var mbres = MessageBox.Show("不完全なStemファイルが見つかりました。削除しますか？", "Error", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (mbres == MessageBoxResult.Yes)
                        {
                            await DeleteStemFiles();
                        }
                    }
                }
                return true;
            }
            else
            {
                MessageBox.Show("ファイルが存在しません\r\n" + path, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (Settings?.RecentFiles.Contains(path) ?? false)
                {
                    Settings.RecentFiles.Remove(path);
                }
                return false;
            }
        }

        public void GetVolumeList()
        {
            int threadCount = 8;
            int listCount = 160;
            var lists = new List<List<float>>(threadCount);
            
            var tasks = new List<Task>();
            var action = new Action<object?>((object? x) =>
            {
                if (x != null) lists[(int)x] = Core.GetVolumeList((int)x * listCount / threadCount, listCount / threadCount, threadCount);
            });
            for (int i = 0; i < threadCount; i++)
            {
                lists.Add(new List<float>());
            }
            for (int i = 0; i < threadCount; i++)
            {
                tasks.Add(Task.Factory.StartNew(action, i));
            }
            Task.WaitAll(tasks.ToArray());

            for (int i = 0; i < threadCount; i++)
            {
                for (int j = 0; j < listCount / threadCount; j++)
                {
                    VolumeList.Add(lists[i][j]);
                }
            }
        }

        public async Task FileClose()
        {
            if (IsFileLoaded)
            {
                Stop();
                IsStemSeparated = false;
                IsStemPlaying = false;
                await SwitchDecoderToSource();
                Position = 0;
                FilePath = "";
                SelectionResetCommand.Execute(null);
                ClearMarkerCommand.Execute(null);
                StatusText = "";
                VolumeList.Clear();
                WindowTitle = applicationName;
                await Task.Run(Core.FileClose);
                CommandBase.RaiseCanExecuteChanged();
            }
        }

        public async void FileDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (await FileOpen(files[0]))
                    Play();
            }
            e.Handled = true;
        }

        public void StemFilesDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (FilePath == "")
                {
                    MessageBox.Show("先にファイルを読み込んでください", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                string dir = "";
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files[0] != null)
                {
                    if (File.GetAttributes(files[0]).HasFlag(FileAttributes.Directory))
                    {
                        dir = files[0];
                    }
                    else
                    {
                        var d = Path.GetDirectoryName(files[0]);
                        if (d != null)
                        {
                            dir = d;
                        }
                    }
                }

                if (dir != "")
                {
                    ImportStems(dir);
                }
            }
            e.Handled = true;
        }

        public void Play()
        {
            if (IsFileLoaded)
            {
                if (State == State.Pausing)
                {
                    Pause();
                }
                else
                {
                    Position = StartPosition;

                    if (State == State.Stopping)
                        playerTask.Wait();

                    if (State == State.Stopped)
                    {
                        playerTask = Task.Run(Core.Start);
                    }
                }
            }
        }

        public void Pause()
        {
            if (IsFileLoaded)
            {
                if (State == State.Playing)
                {
                    Core.Pause();
                    BlinkTimer.Start();
                }
                else if (State == State.Pausing)
                {
                    BlinkTimer.Stop();
                    TimeDisplayOpacity = 1.0;
                    playerTask.Wait();
                    playerTask = Task.Run(Core.Start);
                }
            }
        }

        public void Stop()
        {
            if (IsFileLoaded)
            {
                BlinkTimer.Stop();
                TimeDisplayOpacity = 1.0;
                Core.Stop();
                if (playerTask.Status == TaskStatus.Running)
                    playerTask.Wait();

                Core.ResetRMS();
            }
        }

        public void SeekRelative(int ms)
        {
            if (IsFileLoaded)
            {
                Core.SeekRelative(ms);

                if (State == State.Pausing)
                {
                    Play();
                }
                else if (State == State.Stopped)
                {
                    StartPosition = Position;
                    DisplayUpdate();
                }
            }
        }

        public string GetStemDir(string path)
        {
            var name = Path.GetFileName(path);
            var mydoc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dir = path.Replace("\\", "_").Replace(":", "_");
            return Path.Combine(mydoc, "YorkTrail", "stem", dir);
        }

        public bool FindStemFiles(string dir)
        {
            var files = new List<string> { "vocals.flac", "drums.flac", "bass.flac", "piano.flac", "other.flac" };

            foreach (var file in files)
            {
                var path = Path.Combine(dir, file);

                if (!File.Exists(path))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task DeleteStemFiles()
        {
            IsStemSeparated = false;
            IsStemPlaying = false;
            await SwitchDecoderToSource();
            Core.StemFilesClose();

            var dir = GetStemDir(FilePath);
            if (FindStemFiles(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (IOException e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void SeparateStem()
        {
            if (IsFileLoaded)
            {
                var dir = GetStemDir(FilePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var tsource = new CancellationTokenSource();
                var token = tsource.Token;

                var processTask = Task.Run(() =>
                {
                    IsStemSeparating = true;
                    if (Core.SeparateStem(dir))
                    {
                        if (!FindStemFiles(dir))
                        {
                            return;
                        }
                        Core.StemFilesOpen(dir);
                        IsStemSeparated = true;
                        IsStemPlaying = true;
                        SwitchDecoderToStems();
                    }
                    else
                    {
                        tsource.Cancel();
                    }
                    IsStemSeparating = false;
                });

                var progressTask = Task.Run(() =>
                {
                    while (!processTask.IsCompleted || !token.IsCancellationRequested)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            SeparateProgress = Core.GetProgress();
                        }));
                        Thread.Sleep(100);
                    }

                    if (token.IsCancellationRequested)
                    {
                        DeleteStemFiles();
                    }
                });
            }
        }

        public void GetStemVolumes(out float vo, out float dr, out float bs, out float pn, out float other)
        {
            if (VocalsSolo || DrumsSolo || BassSolo || PianoSolo || OtherSolo)
            {
                vo = (VocalsSolo) ? ((VocalsMute) ? 0.0f : VocalsVolume) : 0.0f;
                dr = (DrumsSolo) ? ((DrumsMute) ? 0.0f : DrumsVolume) : 0.0f;
                bs = (BassSolo) ? ((BassMute) ? 0.0f : BassVolume) : 0.0f;
                pn = (PianoSolo) ? ((PianoMute) ? 0.0f : PianoVolume) : 0.0f;
                other = (OtherSolo) ? ((OtherMute) ? 0.0f : OtherVolume) : 0.0f;
            }
            else
            { 
                vo = (VocalsMute) ? 0.0f : VocalsVolume;
                dr = (DrumsMute) ? 0.0f : DrumsVolume;
                bs = (BassMute) ? 0.0f : BassVolume;
                pn = (PianoMute) ? 0.0f : PianoVolume;
                other = (OtherMute) ? 0.0f : OtherVolume;
            }
        }

        public void SetStemVolumes()
        {
            GetStemVolumes(out float vo, out float dr, out float bs, out float pn, out float other);
            Core.SetStemVolumes(vo, dr, bs, pn, other);
        }

        public async Task SwitchDecoderToSource()
        {
            await Task.Run(Core.SwitchDecoderToSource);
        }

        public async Task SwitchDecoderToStems()
        {
            await Task.Run(Core.SwitchDecoderToStems);
        }
        
        public async void ImportStems(string inputDir)
        {
            if (inputDir == null)
            {
                return;
            }
            if (!File.Exists(Path.Combine(inputDir, "vocals.wav")))
            {
                MessageBox.Show("Stemファイルが見つかりません", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (IsStemSeparated)
            {
                var res = MessageBox.Show("Stemファイルが存在します。上書きしてもよろしいですか？", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Cancel)
                {
                    return;
                }
                await DeleteStemFiles();
            }

            var outputDir = GetStemDir(FilePath);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var processTask = Task.Run(() =>
            {
                IsStemSeparating = true;
                var tasks = new List<Task<bool>>();
                var names = new[] { "vocals", "drums", "bass", "piano", "other" };
                foreach (var name in names)
                {
                    var inputFileName = Path.Combine(inputDir, name + ".wav");
                    var outputFileName = Path.Combine(outputDir, name + ".flac");
                    if (File.Exists(inputFileName))
                    {
                        var t = Task.Run(() => Core.TranscodeToFlac(inputFileName, outputFileName));
                        tasks.Add(t);
                    }
                    else
                    {
                        var t = Task.Run(() => Core.NullOutputToFlac(outputFileName));
                        tasks.Add(t);
                    }
                }
                Task.WaitAll(tasks.ToArray());
                if (tasks.All(x => x.Result))
                {
                    Core.StemFilesOpen(outputDir);
                    IsStemSeparating = false;
                    IsStemSeparated = true;
                    IsStemPlaying = true;
                    SwitchDecoderToStems();
                }
                else
                {
                    IsStemSeparating = false;
                    DeleteStemFiles();
                }
            });

            var progressTask = Task.Run(() =>
            {
                while (!processTask.IsCompleted)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        SeparateProgress = Core.GetProgress();
                    }));
                    Thread.Sleep(100);
                }
            });
        }

        internal void RangeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsFileLoaded)
            {
                if (State == State.Pausing)
                {
                    Play();
                }
                Position = StartPosition;
            }
        }

        internal void RangeSlider_LowerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsFileLoaded)
            {
                //Position = Window.SeekBar.LowerValue;
            }
            else
            {
                StartPosition = 0;
            }
        }
        
        internal void RangeSlider_LowerSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (IsFileLoaded)
            {
                if (State == State.Pausing)
                {
                    Play();
                }
                Position = StartPosition;
            }
        }

        internal void SeekBar_DisplayValueTickBarMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsFileLoaded)
            {
                Position = StartPosition;
            }
        }


        internal async void RecentFile_Clicked(object sender, ExecutedRoutedEventArgs e)
        {
            string path = (string)e.Parameter;
            if (await FileOpen(path))
            {
                Play();
            }
        }
        
        internal void PlaybackDevice_Clicked(object sender, ExecutedRoutedEventArgs e)
        {
            string device = (string)e.Parameter;
            for (int i = 0; i < DeviceList.Count; i++)
            {
                if (DeviceList[i] == device)
                {
                    PlaybackDevice = i;
                }
            }
        }

        internal void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            if (Settings != null && !Settings.IsInitialized)
            {
                RestoreWindowSettings();

                // オプションで指定されていたら復元する
                // ファイルを開く時ウィンドウタイトルを設定するためLoadedの後じゃないとダメ
                if (Settings.RestoreLastState)
                {
                    ResotreState();
                }
            }

            SetKeyBinds();
        }

        internal void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (State == State.Playing)
            {
                Stop();
            }

            if (BlinkTimer.Enabled)
            {
                BlinkTimer.Stop();
            }

            SaveState();
            SaveWindowSettings();
            if (Settings != null)
            {
                Settings.WriteSettingsToFile(Settings);
            }
        }

        internal void FilterPreset_Clicked(object sender, ExecutedRoutedEventArgs e)
        {
            FilterPreset fp = (FilterPreset)e.Parameter;
            LpfEnabled = fp.LpfEnabled;
            HpfEnabled = fp.HpfEnbled;
            BpfEnabled = fp.BpfEnabled;
            LpfFreq = fp.LpfFreq;
            HpfFreq = fp.HpfFreq;
            BpfFreq = fp.BpfFreq;
        }

        internal void PitchStackPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Window != null)
            {
                Window.PitchSliderPopup.IsOpen = true;
            }
        }

        internal void RatioStackPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Window != null)
            {
                Window.RatioSliderPopup.IsOpen = true;
            }
        }
    }
}
