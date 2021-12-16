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

namespace YorkTrail
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            _startPosition = 0.0;
            _endPosition = 1.0;
            _timeSignature = 4;
            MarkerList = new ObservableCollection<double>();

            DeviceList = Core.GetPlaybackDeviceList();
            Settings = Settings.ReadSettingsFromFile();

            if (Settings != null)
            {
                // 無条件で復元する
                Volume = Settings.Volume;
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

            // Positionはイベント駆動
            Core.NotifyTimeChanged += () => {
                RaisePropertyChanged(nameof(TimeString));
                RaisePropertyChanged(nameof(Position));
                RaisePropertyChanged(nameof(RMSL));
                RaisePropertyChanged(nameof(RMSR));
            };

            BlinkTimer = new Timer(800);
            BlinkTimer.Elapsed += (sender, e) =>
            {
                Window.TimeDisplay.Dispatcher.Invoke(() =>
                {
                    double opc = Window.TimeDisplay.Opacity;
                    if (opc == 1.0)
                    {
                        Window.TimeDisplay.Opacity = 0.1;
                    }
                    else if (opc == 0.1)
                    {
                        Window.TimeDisplay.Opacity = 1.0;
                    }
                });
            };
        }

        private const string applicationName = "YorkTrail";

        public Timer BlinkTimer;
        private Task playerTask;
        //private Stopwatch _sw;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string FilePath { get; set; }
        public int ZoomMultiplier { get; set; }
        public ObservableCollection<double> MarkerList { get; set; }

        public ulong Time
        {
            get { return Core.GetTime(); }
        }

        public string TimeString {
            get {
                long cur = (long)Core.GetTime();
                if (Settings.ShowTimeAtMeasure)
                {
                    long offsetCur = cur - MeasureOffset;
                    float msOfBeat = 60000.0f / Tempo;
                    if (msOfBeat <= 0 || TimeSignature <= 0)
                        return "0000:00:00";
                    float msOfMeasure = msOfBeat * TimeSignature;
                    int measure = (int)(offsetCur / msOfMeasure);
                    int beat = (offsetCur < 0) ? (int)(offsetCur / msOfBeat % TimeSignature) : (int)(offsetCur / msOfBeat % TimeSignature + 1);
                    int ms = (int)(cur % msOfBeat / 10);
                    ms = (ms < 1000) ? ms : ms /= 10;
                    return string.Format(@"{0,3:0000}:{1,2:00}:{2,2:00}", measure, beat, ms);
                }
                else
                {
                    return TimeSpan.FromMilliseconds(cur).ToString(@"hh\:mm\:ss\.ff");
                }
            }
        }

        public ulong TotalMilliSeconds { 
            get { return Core.GetTotalMilliSeconds(); }
        }

        public double Position {
            get { return Core.GetPosition(); }
            set {
                Core.SetPosition(value);
                RaisePropertyChanged(nameof(Position));
            }
        }
        public int PlaybackDevice {
            get { return Core.playbackDevice; }
            set {
                Core.playbackDevice = value;
                Settings.DeviceIndex = value;
                Settings.DeviceName = DeviceList[value];
                RaisePropertyChanged(nameof(PlaybackDevice));
            }
        }
        private string _statusText;
        public string StatusText {
            get { return _statusText; }
            set
            {
                _statusText = value;
                RaisePropertyChanged(nameof(StatusText));
            }
        }
        private double _startPosition;
        public double StartPosition {
            get { return _startPosition; }
            set {
                _startPosition = value;
                Core.SetStartPosition(value);
                RaisePropertyChanged(nameof(StartPosition));
            }
        }
        private double _endPosition;
        public double EndPosition {
            get { return _endPosition; }
            set
            {
                _endPosition = value;
                Core.SetEndPosition(value);
                RaisePropertyChanged(nameof(EndPosition));
            }
        }

        public Channels Channels {
            get { return Core.GetChannels(); }
            set {
                Core.SetChannels(value);
                RaisePropertyChanged(nameof(Channels));
            }
        }
        public float Pitch {
            get { return Core.GetPitch(); }
            set {
                Core.SetPitch(value);
                RaisePropertyChanged(nameof(Pitch));
            }
        }
        public float Rate {
            get { return Core.GetRate(); }
            set {
                Core.SetRate(value);
                RaisePropertyChanged(nameof(Rate));
            }
        }
        public bool IsBypass {
            get { return Core.GetBypass(); }
            set {
                Core.SetBypass(value);
                RaisePropertyChanged(nameof(IsBypass));
            }
        }
        public bool IsLoop {
            get { return Core.isLoop; }
            set {
                Core.isLoop = value;
                RaisePropertyChanged(nameof(IsLoop));
            }
        }
        public float Volume {
            get {
                return Settings?.Volume ?? Core.GetVolume();
            }
            set {
                Settings.Volume = value;
                Core.SetVolume(value);
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

        public YorkTrailCore Core { get; private set; } = YorkTrailHandleHolder.hYorkTrailCore;
        public float RMSL { get { return Core.rmsL; } }
        public float RMSR { get { return Core.rmsR; } }
        public bool LpfEnabled { get { return Core.LpfEnabled; } set { Core.LpfEnabled = value; RaisePropertyChanged(nameof(LpfEnabled)); } }
        public bool HpfEnabled { get { return Core.HpfEnabled; } set { Core.HpfEnabled = value; RaisePropertyChanged(nameof(HpfEnabled)); } }
        public bool BpfEnabled { get { return Core.BpfEnabled; } set { Core.BpfEnabled = value; RaisePropertyChanged(nameof(BpfEnabled)); } }
        public float LpfFreq { get { return Core.lpfFreq; } set { Core.SetLPF((float)value); RaisePropertyChanged(nameof(LpfFreq)); } }
        public float HpfFreq { get { return Core.hpfFreq; } set { Core.SetHPF(value); RaisePropertyChanged(nameof(HpfFreq)); } }
        public float BpfFreq { get { return Core.bpfFreq; } set { Core.SetBPF(value); RaisePropertyChanged(nameof(BpfFreq)); } }
        public List<string> DeviceList { get; private set; }
        public Settings Settings { get; private set; }

        public MainWindow Window { get; set; }
        public TempoCalcWindow TempoCalcWindow { get; set; }

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

        public void DisplayUpdate()
        {
            RaisePropertyChanged(nameof(TimeString));
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(RMSL));
            RaisePropertyChanged(nameof(RMSR));
        }

        public void SetKeyBinds()
        {
            Window.InputBindings.Clear();

            foreach(var kb in Settings.KeyBinds)
            {
                Window.InputBindings.Add(Settings.GetKeyBinding(kb.Key));
            }
        }

        public void SaveWindowSettings()
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
        }

        public void RestoreWindowSettings()
        {
            if (Settings != null)
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
            }
        }

        public void SaveState()
        {
            Settings.FilePath = FilePath;
            Settings.Position = Position;
            Settings.StartPosition = StartPosition;
            Settings.EndPosition = EndPosition;
            Settings.Channels = Channels;
            Settings.Pitch = Pitch;
            Settings.Rate = Rate;
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
            Settings.SeekBarMinimum = Window.SeekBar.Minimum;
            Settings.SeekBarMaximum = Window.SeekBar.Maximum;
            Settings.ZoomMultiplier = ZoomMultiplier;
            Settings.MarkerList = MarkerList;
        }

        public void ResotreState()
        {
            if (Settings.FilePath != null)
            {
                if (FileOpen(Settings.FilePath))
                {
                    Position = Settings.Position;
                    StartPosition = Settings.StartPosition;
                    EndPosition = Settings.EndPosition;
                    Tempo = Settings.Tempo;
                    MeasureOffset = Settings.MeasureOffset;
                    TimeSignature = (Settings.TimeSignature == 0) ? 4 : Settings.TimeSignature;
                    Window.SeekBar.Minimum = Settings.SeekBarMinimum;
                    Window.SeekBar.Maximum = Settings.SeekBarMaximum;
                    ZoomMultiplier = Settings.ZoomMultiplier;
                    StaticMethods.ShallowCopy(Settings.MarkerList, MarkerList);
                    RaisePropertyChanged(nameof(Time));
                }
            }
            Channels = Settings.Channels;
            Pitch = Settings.Pitch;
            Rate = Settings.Rate;
            IsBypass = Settings.IsBypass;
            IsLoop = Settings.IsLoop;
            LpfEnabled = Settings.LpfEnabled;
            HpfEnabled = Settings.HpfEnabled;
            BpfEnabled = Settings.BpfEnabled;
            LpfFreq = Settings.LpfFreq;
            HpfFreq = Settings.HpfFreq;
            BpfFreq = Settings.BpfFreq;
        }

        public void AddFilterPreset(string name)
        {
            var p = new FilterPreset(name, LpfEnabled, HpfEnabled, BpfEnabled, LpfFreq, HpfFreq, BpfFreq);
            Settings.FilterPresets.Add(p);
        }

        public bool FileOpen(string path)
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

                Window.Title = applicationName + " - " + Path.GetFileName(path);
                SelectionResetCommand.Execute(Window);
                ClearMarkerCommand.Execute(Window);
                Stop();

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

                System.Threading.CancellationTokenSource tsource = new System.Threading.CancellationTokenSource();
                System.Threading.CancellationToken token = tsource.Token;
                TaskFactory factory = new TaskFactory(token);

                Task t = factory.StartNew(() =>
                {
                    if (!Core.FileOpen(path, type))
                    {
                        tsource.Cancel();
                    }
                })
                .ContinueWith((task) =>
                {
                    StatusText = Core.GetFileInfo();
                    RaisePropertyChanged(nameof(TotalMilliSeconds));
                }, token);

                try
                {
                    t.Wait(token);
                }
                catch(OperationCanceledException)
                {
                    MessageBox.Show("ファイルを開けませんでした\r\n" + path, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    FileClose();
                    return false;
                }

                FilePath = path;
                return true;
            }
            else
            {
                MessageBox.Show("ファイルが存在しません\r\n" + path, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (Settings.RecentFiles.Contains(path))
                {
                    Settings.RecentFiles.Remove(path);
                }
                return false;
            }
        }

        public void FileClose()
        {
            if (Core.IsFileLoaded())
            {
                Stop();
                Core.FileClose();
                FilePath = null;
                SelectionResetCommand.Execute(Window);
                Window.Title = applicationName;
                StatusText = "";
            }
        }

        public void FileDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (FileOpen(files[0]))
                    Play();
            }
            e.Handled = true;
        }

        public void Play()
        {
            if (Core.IsFileLoaded())
            {
                if (Core.GetState() == State.Pausing)
                {
                    Pause();
                }
                else
                {
                    Position = StartPosition;

                    if (Core.GetState() == State.Stopping)
                        playerTask.Wait();

                    if (Core.GetState() == State.Stopped)
                    {
                        playerTask = Task.Run(Core.Start);
                    }
                }
            }
        }

        public void Pause()
        {
            if (Core.IsFileLoaded())
            {
                if (Core.GetState() == State.Playing)
                {
                    Core.Pause();
                    BlinkTimer.Start();
                }
                else if (Core.GetState() == State.Pausing)
                {
                    BlinkTimer.Stop();
                    Window.TimeDisplay.Opacity = 1.0;
                    playerTask.Wait();
                    playerTask = Task.Run(Core.Start);
                }
            }
        }

        public void Stop()
        {
            if (Core.IsFileLoaded())
            {
                BlinkTimer.Stop();
                Window.TimeDisplay.Opacity = 1.0;
                Core.Stop();
                if (playerTask?.Status == TaskStatus.Running)
                    playerTask.Wait();

                Core.ResetRMS();
            }
        }

        public void SeekRelative(int ms)
        {
            if (Core.IsFileLoaded())
            {
                Core.SeekRelative(ms);

                if (Core.GetState() == State.Pausing)
                {
                    Play();
                }
                else if (Core.GetState() == State.Stopped)
                {
                    StartPosition = Position;
                    DisplayUpdate();
                }
            }
        }

        public void RangeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Core.IsFileLoaded())
            {
                if (Core.GetState() == State.Pausing)
                {
                    Play();
                }
                Position = Window.SeekBar.LowerValue;
            }
        }

        public void RangeSlider_LowerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Core.IsFileLoaded())
            {
                //Position = Window.SeekBar.LowerValue;
            }
            else
            {
                Window.SeekBar.LowerValue = 0;
            }
        }
        
        public void RangeSlider_LowerSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (Core.IsFileLoaded())
            {
                if (this.Core.GetState() == State.Pausing)
                {
                    Play();
                }
                Position = Window.SeekBar.LowerValue;
            }
        }

        internal void SeekBar_DisplayValueTickBarMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Core.IsFileLoaded())
            {
                Position = Window.SeekBar.LowerValue;
            }
        }


        public void RecentFile_Clicked(object sender, ExecutedRoutedEventArgs e)
        {
            string path = (string)e.Parameter;
            if (FileOpen(path))
            {
                Play();
            }
        }
        
        public void PlaybackDevice_Clicked(object sender, ExecutedRoutedEventArgs e)
        {
            string device = (string)e.Parameter;
            for (int i = 0; i < DeviceList.Count; i++)
            {
                if (DeviceList[i] == device)
                {
                    if (Core.IsFileLoaded())
                    {
                        Stop();
                        Core.DeviceClose();
                        PlaybackDevice = i;
                        Core.DeviceOpen();
                    }
                    else
                    {
                        PlaybackDevice = i;
                    }
                }
            }
        }

        internal void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            if (!Settings.IsInitialized)
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

        public void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Core.GetState() == State.Playing)
            {
                Stop();
            }

            if (BlinkTimer.Enabled)
            {
                BlinkTimer.Stop();
            }

            SaveState();
            SaveWindowSettings();
            Settings.WriteSettingsToFile(Settings);
        }

        public void FilterPreset_Clicked(object sender, ExecutedRoutedEventArgs e)
        {
            FilterPreset fp = (FilterPreset)e.Parameter;
            LpfEnabled = fp.LpfEnabled;
            HpfEnabled = fp.HpfEnbled;
            BpfEnabled = fp.BpfEnabled;
            LpfFreq = fp.LpfFreq;
            HpfFreq = fp.HpfFreq;
            BpfFreq = fp.BpfFreq;
        }
    }
}
