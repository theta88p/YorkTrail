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
                this.Volume = Settings.Volume;

                // デバイス構成が前回と違っていたら復元しない
                if (Settings.DeviceName == DeviceList[Settings.DeviceIndex])
                {
                    PlaybackDevice = Settings.DeviceIndex;
                }
            }

            //Positionはイベント駆動
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

        public bool IsZooming { get; set; }
        public ulong Time {
            get { return Core.GetTime(); }
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
        public bool UseLpf { get { return Core.useLpf; } set { Core.useLpf = value; } }
        public bool UseHpf { get { return Core.useHpf; } set { Core.useHpf = value; } }
        public bool UseBpf { get { return Core.useBpf; } set { Core.useBpf = value; } }
        public float LpfFreq { set { Core.SetLPF(value); } }
        public float HpfFreq { set { Core.SetHPF(value); } }
        public float BpfFreq { set { Core.SetBPF(value); } }
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
        public ZoomResetCommand ZoomResetCommand { get; private set; } = new ZoomResetCommand();
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

        public void DisplayUpdate()
        {
            RaisePropertyChanged(nameof(Time));
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(RMSL));
            RaisePropertyChanged(nameof(RMSR));
        }

        public void CollectWindowSettings()
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

        public void SetPath(string path)
        {
            string ext = System.IO.Path.GetExtension(path);
            ext = ext.ToLower();
            if (ext == ".wav" || ext == ".mp3" || ext == ".flac")
            {
                Window.Title = applicationName + " - " + Path.GetFileName(path);
                this.ZoomResetCommand.Execute(null);
                this.SelectionResetCommand.Execute(this.Window);

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
                Task.Run(() => { Core.SetPath(path); })
                    .ContinueWith((task) => { this.StatusText = Core.GetFileInfo(); })
                    .ContinueWith((task) => { Core.Start(); });
            }
            else
            {
                MessageBox.Show("未対応のファイル形式です", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void FileClose()
        {
            StopCommand.Execute(Window);
            Core.FileClose();
            SelectionResetCommand.Execute(Window);
            Window.Title = applicationName;
            this.StatusText = "";
        }

        public void FileDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                SetPath(files[0]);
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
                rs.LowerValue = pos;
                this.Position = (float)pos;
                this.StartPosition = (float)pos;

            }
        }
        public void RangeSlider_LowerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Core.IsFileLoaded())
            {
                ((RangeSlider)sender).LowerValue = 0;
            }
        }
        public void RangeSlider_UpperValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.EndPosition = (float)e.NewValue;
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
            SetPath(path);
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
        public void mainWindow_Loaded(object sender, EventArgs e)
        {
            RestoreWindowSettings();
            this.Settings ??= new Settings();
        }
        public void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CollectWindowSettings();
            Settings.WriteSettingsToFile(Settings);
        }
    }
}
