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
            this.DeviceList = Core.GetPlaybackDeviceList();
            this.Settings = Settings.ReadSettingsFromFile();

            if (Settings != null)
            {
                // 無条件で復元する
                this.Volume = Settings.Volume;
                Core.SetSoundTouchParam(Settings.SoundTouchSequenceMS, Settings.SoundTouchSeekWindowMS, Settings.SoundTouchOverlapMS);

                // デバイス構成が前回と違っていたら復元しない
                if (Settings.DeviceName == DeviceList[Settings.DeviceIndex])
                {
                    this.PlaybackDevice = Settings.DeviceIndex;
                }
            }

            // Positionはイベント駆動
            Core.NotifyTimeChanged += () => {
                RaisePropertyChanged(nameof(Time));
                RaisePropertyChanged(nameof(Position));
                RaisePropertyChanged(nameof(RMSL));
                RaisePropertyChanged(nameof(RMSR));
            };

            BlinkTimer = new Timer(800);
            BlinkTimer.Elapsed += (sender, e) =>
            {
                this.Window.TimeDisplay.Dispatcher.Invoke(() =>
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
            /*
            //時間はVMで補完しながら駆動する
            _sw = new Stopwatch();
            _timer = new Timer(50);
            _timer.Elapsed += (sender, e) => {
                if (Player.GetState() == State.Playing)
                {
                    long t = Player.Time;
                    if (t != _playerTime)
                    {
                        _playerTime = t;
                        _curTime = t;
                        _sw.Restart();
                    }
                    else
                    {
                        if (!_sw.IsRunning)
                        {
                            _sw.Start();
                        }

                        _curTime = _playerTime + (long)(_sw.ElapsedMilliseconds * Player.Rate);
                    }
                    if (Player.GetState() == State.Playing)
                    {
                        NotifyPropertyChanged(nameof(Time));
                    }
                }
                else
                {
                    if (_sw.IsRunning)
                    {
                        _sw.Reset();
                    }
                }
            };
            _timer.Start();
            */
        }

        private string applicationName = "YorkTrail";


        public Timer BlinkTimer;
        //private Stopwatch _sw;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string FilePath { get; set; }
        private bool _isZooming;
        public bool IsZooming
        {
            get { return _isZooming; }
            set
            {
                _isZooming = value;
                if (_isZooming)
                {
                    this.Window.ProgressBar.Minimum = this.StartPosition;
                    this.Window.ProgressBar.Maximum = this.EndPosition;
                    this.Window.RangeSlider.Minimum = this.StartPosition;
                    this.Window.RangeSlider.Maximum = this.EndPosition;
                }
                else
                {
                    this.Window.ProgressBar.Minimum = 0.0;
                    this.Window.ProgressBar.Maximum = 1.0;
                    this.Window.RangeSlider.Minimum = 0.0;
                    this.Window.RangeSlider.Maximum = 1.0;
                }
                RaisePropertyChanged(nameof(IsZooming));
            }
        }
        public ulong Time {
            get { return Core.GetTime(); }
        }
        public ulong TotalMilliSeconds { 
            get { return Core.GetTotalMilliSeconds(); }
        }
        public float Position {
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
        public float StartPosition {
            get { return Core.startPos; }
            set {
                Core.startPos = value;
                RaisePropertyChanged(nameof(StartPosition));
            }
        }
        public float EndPosition {
            get { return Core.endPos; }
            set {
                Core.endPos = value;
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

        public PlayCommand PlayCommand { get; private set; } = new PlayCommand();
        public StopCommand StopCommand { get; private set; } = new StopCommand();
        public PauseCommand PauseCommand { get; private set; } = new PauseCommand();
        public FFCommand FFCommand { get; private set; } = new FFCommand();
        public FRCommand FRCommand { get; private set; } = new FRCommand();
        public ToStartCommand ToStartCommand { get; private set; } = new ToStartCommand();
        public ToEndCommand ToEndCommand { get; private set; } = new ToEndCommand();
        public ZoomCommand ZoomCommand { get; private set; } = new ZoomCommand();
        public LpfOnCommand LpfOnCommand { get; private set; } = new LpfOnCommand();
        public HpfOnCommand HpfOnCommand { get; private set; } = new HpfOnCommand();
        public BpfOnCommand BpfOnCommand { get; private set; } = new BpfOnCommand();
        public BypassCommand BypassCommand { get; private set; } = new BypassCommand();
        public FileOpenCommand FileOpenCommand { get; private set; } = new FileOpenCommand();
        public OpenTempoCalcWindowCommand OpenTempoCalcWindowCommand { get; private set; } = new OpenTempoCalcWindowCommand();
        public FileCloseCommand FileCloseCommand { get; private set; } = new FileCloseCommand();
        public ExitCommand ExitCommand { get; private set; } = new ExitCommand();
        public SelectionResetCommand SelectionResetCommand { get; private set; } = new SelectionResetCommand();
        public ShowAboutCommand ShowAboutCommand { get; private set; } = new ShowAboutCommand();
        public CurrentToStartPositionCommand CurrentToStartPositionCommand { get; private set; } = new CurrentToStartPositionCommand();
        public CurrentToEndPositionCommand CurrentToEndPositionCommand { get; private set; } = new CurrentToEndPositionCommand();
        public StereoCommand StereoCommand { get; private set; } = new StereoCommand();
        public MonoCommand MonoCommand { get; private set; } = new MonoCommand();
        public LOnlyCommand LOnlyCommand { get; private set; } = new LOnlyCommand();
        public ROnlyCommand ROnlyCommand { get; private set; } = new ROnlyCommand();
        public LMinusRCommand LMinusRCommand { get; private set; } = new LMinusRCommand();
        public PitchQuadCommand PitchQuadCommand { get; private set; } = new PitchQuadCommand();
        public PitchDoubleCommand PitchDoubleCommand { get; private set; } = new PitchDoubleCommand();
        public PitchNormalCommand PitchNormalCommand { get; private set; } = new PitchNormalCommand();
        public PitchHalfCommand PitchHalfCommand { get; private set; } = new PitchHalfCommand();
        public TempoDoubleCommand TempoDoubleCommand { get; private set; } = new TempoDoubleCommand();
        public TempoNormalCommand TempoNormalCommand { get; private set; } = new TempoNormalCommand();
        public TempoHalfCommand TempoHalfCommand { get; private set; } = new TempoHalfCommand();
        public TempoOneThirdCommand TempoOneThirdCommand { get; private set; } = new TempoOneThirdCommand();
        public TempoQuarterCommand TempoQuarterCommand { get; private set; } = new TempoQuarterCommand();
        public LoopCommand LoopCommand { get; private set; } = new LoopCommand();
        public AlwaysOnTopCommand AlwaysOnTopCommand { get; private set; } = new AlwaysOnTopCommand();
        public OpenKeyCustomizeCommand OpenKeyCustomizeCommand { get; private set; } = new OpenKeyCustomizeCommand();
        public OpenSettingWindowCommand OpenSettingWindowCommand { get; private set; } = new OpenSettingWindowCommand();
        public OpenAddFilterPresetWindowCommand OpenAddFilterPresetWindowCommand { get; private set; } = new OpenAddFilterPresetWindowCommand();
        public FilterPresetDeleteCommand FilterPresetDeleteCommand { get; private set; } = new FilterPresetDeleteCommand();
        public FilterPresetMoveUpCommand FilterPresetMoveUpCommand { get; private set; } = new FilterPresetMoveUpCommand();
        public FilterPresetMoveDownCommand FilterPresetMoveDownCommand { get; private set; } = new FilterPresetMoveDownCommand();
        public FilterPresetRenameCommand filterPresetRenameCommand { get; private set; } = new FilterPresetRenameCommand();

        public void DisplayUpdate()
        {
            RaisePropertyChanged(nameof(Time));
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(RMSL));
            RaisePropertyChanged(nameof(RMSR));
        }

        public void SetKeyBinds()
        {
            Window.InputBindings.Clear();

            Window.InputBindings.Add(new KeyBinding() { Command = PlayCommand, CommandParameter = Window, Key = Settings.KeyBinds["Play"].Key, Modifiers = Settings.KeyBinds["Play"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = PauseCommand, CommandParameter = Window, Key = Settings.KeyBinds["Pause"].Key, Modifiers = Settings.KeyBinds["Pause"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = StopCommand, CommandParameter = Window, Key = Settings.KeyBinds["Stop"].Key, Modifiers = Settings.KeyBinds["Stop"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = FFCommand, CommandParameter = Window, Key = Settings.KeyBinds["FF"].Key, Modifiers = Settings.KeyBinds["FF"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = FRCommand, CommandParameter = Window, Key = Settings.KeyBinds["FR"].Key, Modifiers = Settings.KeyBinds["FR"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = ToStartCommand, CommandParameter = Window, Key = Settings.KeyBinds["ToStart"].Key, Modifiers = Settings.KeyBinds["ToStart"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = ToEndCommand, CommandParameter = Window, Key = Settings.KeyBinds["ToEnd"].Key, Modifiers = Settings.KeyBinds["ToEnd"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = CurrentToStartPositionCommand, CommandParameter = Window, Key = Settings.KeyBinds["CurrentToStartPosition"].Key, Modifiers = Settings.KeyBinds["CurrentToStartPosition"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = CurrentToEndPositionCommand, CommandParameter = Window, Key = Settings.KeyBinds["CurrentToEndPosition"].Key, Modifiers = Settings.KeyBinds["CurrentToEndPosition"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = PlayCommand, CommandParameter = Window, Key = Settings.KeyBinds["CurrentToStartPosition"].Key, Modifiers = Settings.KeyBinds["CurrentToStartPosition"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = StereoCommand, CommandParameter = Window, Key = Settings.KeyBinds["Stereo"].Key, Modifiers = Settings.KeyBinds["Stereo"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = MonoCommand, CommandParameter = Window, Key = Settings.KeyBinds["Mono"].Key, Modifiers = Settings.KeyBinds["Mono"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = LOnlyCommand, CommandParameter = Window, Key = Settings.KeyBinds["LOnly"].Key, Modifiers = Settings.KeyBinds["LOnly"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = ROnlyCommand, CommandParameter = Window, Key = Settings.KeyBinds["ROnly"].Key, Modifiers = Settings.KeyBinds["ROnly"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = LMinusRCommand, CommandParameter = Window, Key = Settings.KeyBinds["LMinusR"].Key, Modifiers = Settings.KeyBinds["LMinusR"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = LpfOnCommand, CommandParameter = Window, Key = Settings.KeyBinds["LpfOn"].Key, Modifiers = Settings.KeyBinds["LpfOn"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = HpfOnCommand, CommandParameter = Window, Key = Settings.KeyBinds["HpfOn"].Key, Modifiers = Settings.KeyBinds["HpfOn"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = BpfOnCommand, CommandParameter = Window, Key = Settings.KeyBinds["BpfOn"].Key, Modifiers = Settings.KeyBinds["BpfOn"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = PitchQuadCommand, CommandParameter = Window, Key = Settings.KeyBinds["PitchQuad"].Key, Modifiers = Settings.KeyBinds["PitchQuad"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = PitchDoubleCommand, CommandParameter = Window, Key = Settings.KeyBinds["PitchDouble"].Key, Modifiers = Settings.KeyBinds["PitchDouble"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = PitchNormalCommand, CommandParameter = Window, Key = Settings.KeyBinds["PitchNormal"].Key, Modifiers = Settings.KeyBinds["PitchNormal"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = PitchHalfCommand, CommandParameter = Window, Key = Settings.KeyBinds["PitchHalf"].Key, Modifiers = Settings.KeyBinds["PitchHalf"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = TempoDoubleCommand, CommandParameter = Window, Key = Settings.KeyBinds["TempoDouble"].Key, Modifiers = Settings.KeyBinds["TempoDouble"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = TempoNormalCommand, CommandParameter = Window, Key = Settings.KeyBinds["TempoNormal"].Key, Modifiers = Settings.KeyBinds["TempoNormal"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = TempoHalfCommand, CommandParameter = Window, Key = Settings.KeyBinds["TempoHalf"].Key, Modifiers = Settings.KeyBinds["TempoHalf"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = TempoOneThirdCommand, CommandParameter = Window, Key = Settings.KeyBinds["TempoOneThird"].Key, Modifiers = Settings.KeyBinds["TempoOneThird"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = TempoQuarterCommand, CommandParameter = Window, Key = Settings.KeyBinds["TempoQuarter"].Key, Modifiers = Settings.KeyBinds["TempoQuarter"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = BypassCommand, CommandParameter = Window, Key = Settings.KeyBinds["Bypass"].Key, Modifiers = Settings.KeyBinds["Bypass"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = FileOpenCommand, CommandParameter = Window, Key = Settings.KeyBinds["FileOpen"].Key, Modifiers = Settings.KeyBinds["FileOpen"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = LoopCommand, CommandParameter = Window, Key = Settings.KeyBinds["Loop"].Key, Modifiers = Settings.KeyBinds["Loop"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = SelectionResetCommand, CommandParameter = Window, Key = Settings.KeyBinds["SelectionReset"].Key, Modifiers = Settings.KeyBinds["SelectionReset"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = ZoomCommand, CommandParameter = Window, Key = Settings.KeyBinds["Zoom"].Key, Modifiers = Settings.KeyBinds["Zoom"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = OpenTempoCalcWindowCommand, CommandParameter = Window, Key = Settings.KeyBinds["OpenTempoCalcWindow"].Key, Modifiers = Settings.KeyBinds["OpenTempoCalcWindow"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = AlwaysOnTopCommand, CommandParameter = Window, Key = Settings.KeyBinds["AlwaysOnTop"].Key, Modifiers = Settings.KeyBinds["AlwaysOnTop"].Modifiers });
            Window.InputBindings.Add(new KeyBinding() { Command = ExitCommand, CommandParameter = Window, Key = Settings.KeyBinds["Exit"].Key, Modifiers = Settings.KeyBinds["Exit"].Modifiers });
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
            Settings.FilePath = this.FilePath;
            Settings.Position = this.Position;
            Settings.StartPosition = this.StartPosition;
            Settings.EndPosition = this.EndPosition;
            Settings.Channels = this.Channels;
            Settings.Pitch = this.Pitch;
            Settings.Rate = this.Rate;
            Settings.IsBypass = this.IsBypass;
            Settings.IsLoop = this.IsLoop;
            Settings.LpfEnabled = this.LpfEnabled;
            Settings.HpfEnabled = this.HpfEnabled;
            Settings.BpfEnabled = this.BpfEnabled;
            Settings.LpfFreq = this.LpfFreq;
            Settings.HpfFreq = this.HpfFreq;
            Settings.BpfFreq = this.BpfFreq;
            Settings.IsZooming = this.IsZooming;
        }

        public void ResotreState()
        {
            if (Settings.FilePath != null)
            {
                FileOpen(Settings.FilePath, true);
            }
            this.Position = Settings.Position;
            RaisePropertyChanged(nameof(Time));
            this.StartPosition = Settings.StartPosition;
            Window.RangeSlider.LowerValue = this.StartPosition;
            this.EndPosition = Settings.EndPosition;
            Window.RangeSlider.UpperValue = this.EndPosition;
            this.Channels = Settings.Channels;
            this.Pitch = Settings.Pitch;
            this.Rate = Settings.Rate;
            this.IsBypass = Settings.IsBypass;
            this.IsLoop = Settings.IsLoop;
            this.LpfEnabled = Settings.LpfEnabled;
            this.HpfEnabled = Settings.HpfEnabled;
            this.BpfEnabled = Settings.BpfEnabled;
            this.LpfFreq = Settings.LpfFreq;
            this.HpfFreq = Settings.HpfFreq;
            this.BpfFreq = Settings.BpfFreq;
            this.IsZooming = Settings.IsZooming;
        }

        public void AddFilterPreset(string name)
        {
            var p = new FilterPreset(name, this.LpfEnabled, this.HpfEnabled, this.BpfEnabled, this.LpfFreq, this.HpfFreq, this.BpfFreq);
            this.Settings.FilterPresets.Add(p);
        }

        public void FileOpen(string path, bool restored)
        {
            if (File.Exists(path))
            {
                this.FilePath = path;
                string ext = Path.GetExtension(path);
                ext = ext.ToLower();
                if (ext == ".wav" || ext == ".mp3" || ext == ".flac")
                {
                    Window.Title = applicationName + " - " + Path.GetFileName(path);
                    SelectionResetCommand.Execute(this.Window);
                    // 一時停止状態の解除
                    StopCommand.Execute(Window);
                    //BlinkTimer.Stop();
                    //Window.TimeDisplay.Opacity = 1.0;

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
                        if (!Core.FileOpen(path))
                        {
                            tsource.Cancel();
                        }
                    })
                    .ContinueWith((task) =>
                    {
                        this.StatusText = Core.GetFileInfo();
                        RaisePropertyChanged(nameof(TotalMilliSeconds));
                    }, token);

                    if (!restored)
                    {
                        t.ContinueWith((task) => { Core.Start(); }, token);
                    }

                    try
                    {
                        t.Wait(token);
                    }
                    catch(OperationCanceledException)
                    {
                        MessageBox.Show("ファイルを開けませんでした", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        FileClose();
                    }
                }
                else
                {
                    MessageBox.Show("未対応のファイル形式です", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("ファイルが存在しません", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (Settings.RecentFiles.Contains(path))
                {
                    Settings.RecentFiles.Remove(path);
                }
            }
        }

        public void FileClose()
        {
            StopCommand.Execute(Window);
            Core.FileClose();
            this.FilePath = null;
            SelectionResetCommand.Execute(Window);
            Window.Title = applicationName;
            this.StatusText = "";
        }

        public void FileDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                FileOpen(files[0], false);
            }
            e.Handled = true;
        }

        public void RangeSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Core.IsFileLoaded())
            {
                if (this.Core.GetState() == State.Pausing)
                {
                    this.BlinkTimer.Stop();
                    Window.TimeDisplay.Opacity = 1.0;
                    this.Core.Start();
                }
                RangeSlider rs = (RangeSlider)sender;
                Point p = e.GetPosition(rs);
                //double pos = (p.X / rs.Width * (1.0 - rs.Minimum) + rs.Minimum) * rs.Maximum;
                double pos = ((rs.Maximum - rs.Minimum) * p.X / rs.Width) + rs.Minimum;
                //rs.LowerValue = pos;
                this.Position = (float)pos;
                this.StartPosition = (float)pos;
            }
        }

        public void RangeSlider_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RangeSlider rs = (RangeSlider)sender;
            Point p = e.GetPosition(rs);
            double pos = ((rs.Maximum - rs.Minimum) * p.X / rs.Width) + rs.Minimum;
            //rs.UpperValue = pos;
            this.EndPosition = (float)pos;
        }

        public void RangeSlider_LowerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Core.IsFileLoaded())
            {
                ((RangeSlider)sender).LowerValue = 0;
            }
        }
        
        public void RangeSlider_LowerSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (!Core.IsFileLoaded())
            {
                Slider rs = (Slider)sender;
                float value = (float)rs.Value;
                this.StartPosition = value;
            }
            else
            {
                if (this.Core.GetState() == State.Pausing)
                {
                    this.BlinkTimer.Stop();
                    Window.TimeDisplay.Opacity = 1.0;
                    this.Core.Start();
                }
                Slider rs = (Slider)sender;
                float value = (float)rs.Value;
                this.Position = value;
                this.StartPosition = value;
            }
        }
        
        public void RangeSlider_UpperSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            Slider rs = (Slider)sender;
            float value = (float)rs.Value;
            this.EndPosition = value;
        }
        
        public void RecentFile_Clicked(object sender, ExecutedRoutedEventArgs e)
        {
            string path = (string)e.Parameter;
            FileOpen(path, false);
        }
        
        public void PlaybackDevice_Clicked(object sender, ExecutedRoutedEventArgs e)
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

        public void MainWindow_Loaded(object sender, EventArgs e)
        {
            if (Settings != null)
            {
                RestoreWindowSettings();

                // オプションで指定されていたら復元する
                // ファイルを開く時ウィンドウタイトルを設定するためLoadedの後じゃないとダメ
                if (Settings.RestoreLastState)
                {
                    ResotreState();
                }
            }
            else
            {
                // null判定するから全部終わった後じゃないとダメ
                this.Settings = new Settings();
            }

            SetKeyBinds();
        }

        public void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Core.GetState() == State.Playing)
            {
                this.Core.Stop();
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
            this.LpfEnabled = fp.LpfEnabled;
            this.HpfEnabled = fp.HpfEnbled;
            this.BpfEnabled = fp.BpfEnabled;
            this.LpfFreq = fp.LpfFreq;
            this.HpfFreq = fp.HpfFreq;
            this.BpfFreq = fp.BpfFreq;
        }
    }
}
