using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace YorkTrail
{
    public static class CommandCollection
    {
        private static readonly ReadOnlyDictionary<string, CommandBase> Commands = new ReadOnlyDictionary<string, CommandBase>(new Dictionary<string, CommandBase>
        {
            { nameof(PlayCommand), new PlayCommand() },
            { nameof(StopCommand), new StopCommand() },
            { nameof(PauseCommand), new PauseCommand() },
            { nameof(FFCommand), new FFCommand() },
            { nameof(FRCommand), new FRCommand() },
            { nameof(ToPrevMarkerCommand), new ToPrevMarkerCommand() },
            { nameof(ToNextMarkerCommand), new ToNextMarkerCommand() },
            { nameof(ChannelCommand), new ChannelCommand() },
            { nameof(PitchCommand), new PitchCommand() },
            { nameof(TempoCommand), new TempoCommand() },
            { nameof(ZoomInCommand), new ZoomInCommand() },
            { nameof(ZoomOutCommand), new ZoomOutCommand() },
            { nameof(LpfOnCommand), new LpfOnCommand() },
            { nameof(HpfOnCommand), new HpfOnCommand() },
            { nameof(BpfOnCommand), new BpfOnCommand() },
            { nameof(BypassCommand), new BypassCommand() },
            { nameof(LoopCommand), new LoopCommand() },
            { nameof(AlwaysOnTopCommand), new AlwaysOnTopCommand() },
            { nameof(ShowTimeAtMeasureCommand), new ShowTimeAtMeasureCommand() },
            { nameof(SnapToTickCommand), new SnapToTickCommand() },
            { nameof(OpenTempoCalcWindowCommand), new OpenTempoCalcWindowCommand() },
            { nameof(FileOpenCommand), new FileOpenCommand() },
            { nameof(FileCloseCommand), new FileCloseCommand() },
            { nameof(ExitCommand), new ExitCommand() },
            { nameof(ShowAboutCommand), new ShowAboutCommand() },
            { nameof(OpenKeyCustomizeCommand), new OpenKeyCustomizeCommand() },
            { nameof(OpenSettingWindowCommand), new OpenSettingWindowCommand() },
            { nameof(SelectionResetCommand), new SelectionResetCommand() },
            { nameof(CurrentToStartPositionCommand), new CurrentToStartPositionCommand() },
            { nameof(CurrentToEndPositionCommand), new CurrentToEndPositionCommand() },
            { nameof(OpenAddFilterPresetWindowCommand), new OpenAddFilterPresetWindowCommand() },
            { nameof(FilterPresetDeleteCommand), new FilterPresetDeleteCommand() },
            { nameof(FilterPresetRenameCommand), new FilterPresetRenameCommand() },
            { nameof(FilterPresetMoveUpCommand), new FilterPresetMoveUpCommand() },
            { nameof(FilterPresetMoveDownCommand), new FilterPresetMoveDownCommand() },
            { nameof(AddMarkerCommand), new AddMarkerCommand() },
            { nameof(ClearMarkerCommand), new ClearMarkerCommand() },
            { nameof(LinkSlidersCommand), new LinkSlidersCommand() },
        });

        public static CommandBase Get(string cmd)
        {
            return Commands[cmd];
        }

        public static void SetWindowInstance(MainWindow w)
        {
            foreach (var c in Commands)
            {
                var cmd = c.Value;
                cmd.Window = w;
                cmd.ViewModel = (MainWindowViewModel)w.DataContext;
            }
        }
    }

    public class CommandBase : ICommand
    {
        public MainWindow Window { get; set; }
        public MainWindowViewModel ViewModel { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public virtual void Execute(object parameter)
        {
            // 何もしない
        }
    }

    public class PlayCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.Play();
        }
    }

    public class StopCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.Stop();
        }
    }

    public class PauseCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.Pause();
        }
    }

    public class FFCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.SeekRelative(ViewModel.Settings.SkipLengthMS);
        }
    }

    public class FRCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.SeekRelative(-1 * ViewModel.Settings.SkipLengthMS);
        }
    }

    public class ToPrevMarkerCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = Window;
            var vm = ViewModel;
            double value = StaticMethods.GetClosePointInList(vm.MarkerList, vm.Position, vm.TotalMilliSeconds, true);
            value = Math.Max(window.SeekBar.Minimum, value);
            value = Math.Min(window.SeekBar.Maximum, value);

            if (vm.Core.GetState() == State.Pausing)
            {
                vm.Play();
            }
            // 開始終了をリンクしている場合UIと内部で不整合が起きるためこうする
            window.SeekBar.LowerSlider.Value = value;
            vm.Position = value;
        }
    }

    public class ToNextMarkerCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = Window;
            var vm = ViewModel;
            double value = StaticMethods.GetClosePointInList(vm.MarkerList, vm.Position, vm.TotalMilliSeconds, false);
            value = Math.Max(window.SeekBar.Minimum, value);
            value = Math.Min(window.SeekBar.Maximum, value);

            if (vm.Core.GetState() == State.Pausing)
            {
                vm.Stop();
            }
            window.SeekBar.LowerSlider.Value = value;
            vm.Position = value;
        }
    }

    public class ChannelCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var ch = (Channels)parameter;
            ViewModel.Channels = ch;
        }
    }

    public class PitchCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var p = (float)parameter;
            ViewModel.Pitch = p;
        }
    }

    public class TempoCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var r = (float)parameter;
            ViewModel.Ratio = r;
        }
    }

    public class ZoomInCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = Window;
            var vm = ViewModel;
            int initial = (int)Math.Pow(2, (int)((vm.EndPosition - vm.StartPosition) * 4));
            initial = Math.Max(initial, 4);
            vm.ZoomMultiplier = Math.Max(initial, vm.ZoomMultiplier * 2);
            vm.ZoomMultiplier = Math.Min(vm.ZoomMultiplier, 1024);
            window.SeekBar.Minimum = Math.Max(0, vm.StartPosition - (vm.EndPosition - vm.StartPosition) / (vm.ZoomMultiplier * (vm.EndPosition - vm.StartPosition)));
            window.SeekBar.Maximum = Math.Min(1, vm.EndPosition + (vm.EndPosition - vm.StartPosition) / (vm.ZoomMultiplier * (vm.EndPosition - vm.StartPosition)));
        }
    }

    public class ZoomOutCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = Window;
            var vm = ViewModel;
            int initial = (int)Math.Pow(2, (int)((vm.EndPosition - vm.StartPosition) * 4));
            initial = Math.Max(initial, 4);
            vm.ZoomMultiplier = (vm.ZoomMultiplier <= initial) ? 0 : vm.ZoomMultiplier / 2;
            window.SeekBar.Minimum = Math.Max(0, vm.StartPosition - (vm.EndPosition - vm.StartPosition) / (vm.ZoomMultiplier * (vm.EndPosition - vm.StartPosition)));
            window.SeekBar.Maximum = Math.Min(1, vm.EndPosition + (vm.EndPosition - vm.StartPosition) / (vm.ZoomMultiplier * (vm.EndPosition - vm.StartPosition)));
        }
    }

    public class LpfOnCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.LpfEnabled = Window.LpfOnButton.IsChecked ?? false;
        }
    }

    public class HpfOnCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.LpfEnabled = Window.HpfOnButton.IsChecked ?? false;
        }
    }

    public class BpfOnCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.LpfEnabled = Window.BpfOnButton.IsChecked ?? false;
        }
    }
    
    public class BypassCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.IsBypass = !ViewModel.IsBypass;
        }
    }

    public class LoopCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.IsLoop = !ViewModel.IsLoop;
        }
    }
    public class AlwaysOnTopCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.Settings.AlwaysOnTop = !ViewModel.Settings.AlwaysOnTop;
        }
    }
    public class ShowTimeAtMeasureCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.Settings.ShowTimeAtMeasure = !ViewModel.Settings.ShowTimeAtMeasure;
        }
    }

    public class SnapToTickCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.Settings.SnapToTick = !ViewModel.Settings.SnapToTick;
        }
    }

    public class OpenTempoCalcWindowCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = Window;
            var vm = ViewModel;

            if (vm.TempoCalcWindow == null || !vm.TempoCalcWindow.IsLoaded)
            {
                var tcwindow = new TempoCalcWindow(vm);
                vm.TempoCalcWindow = tcwindow;
                var tcwvm = (TempoCalcWindowViewModel)tcwindow.DataContext;
                tcwvm.StartTime = (ulong)vm.MeasureOffset;
                tcwvm.Tempo = vm.Tempo;
                tcwindow.ShowActivated = true;
                tcwindow.Owner = window;
                tcwindow.Show();
            }
            else if (vm.TempoCalcWindow.Visibility == Visibility.Collapsed)
            {
                vm.TempoCalcWindow.Visibility = Visibility.Visible;
            }
            else
            {
                vm.TempoCalcWindow.Visibility = Visibility.Collapsed;
            }
        }
    }
    public class FileOpenCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var vm = ViewModel;
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "音声ファイル (*.wav;*.mp3;*.flac)|*.wav;*.mp3;*.flac";
            if (ofd.ShowDialog() == true)
            {
                if (vm.FileOpen(ofd.FileName))
                    vm.Play();
            }
        }
    }

    public class FileCloseCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return ViewModel?.Core.IsFileLoaded() ?? false;
        }

        public override void Execute(object parameter)
        {
            ViewModel.FileClose();
        }
    }

    public class ExitCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            Window.Close();
        }
    }

    public class ShowAboutCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = Window;
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowActivated = true;
            aboutWindow.Topmost = window.Topmost;
            aboutWindow.Owner = window;
            aboutWindow.ShowDialog();
        }
    }

    public class OpenKeyCustomizeCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = Window;
            var vm = ViewModel;
            var kcwindow = new KeyCustomizeWindow(vm);
            kcwindow.ShowActivated = true;
            kcwindow.Owner = window;
            kcwindow.ShowDialog();
        }
    }

    public class OpenSettingWindowCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var sw = new SettingWindow(ViewModel);
            sw.ShowActivated = true;
            sw.Owner = Window;
            sw.ShowDialog();
        }
    }

    public class SelectionResetCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            Window.SeekBar.Minimum = 0.0;
            Window.SeekBar.Maximum = 1.0;
            ViewModel.StartPosition = 0.0;
            ViewModel.EndPosition = 1.0;
        }
    }

    public class CurrentToStartPositionCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            // ValueChangedの中で値を変更しているので、バインドしているプロパティを直に変更すると
            // 呼び出し順の関係上、開始終了をリンクしているとき不整合が起きる
            Window.SeekBar.LowerSlider.Value = ViewModel.Position;
        }
    }

    public class CurrentToEndPositionCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            Window.SeekBar.UpperSlider.Value = ViewModel.Position;
        }
    }

    public class OpenAddFilterPresetWindowCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var afw = new PresetNameInputWindow(ViewModel);
            afw.ShowActivated = true;
            afw.Owner = Window;
            bool dr = (bool)afw.ShowDialog();
            if (dr)
            {
                string name = afw.PresetNameTextBox.Text;
                ViewModel.AddFilterPreset(name);
            }
        }
    }

    public class FilterPresetDeleteCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = parameter as SettingWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            var lb = (ListBox)window.FilterPresetsListBox;
            var fp = (FilterPreset)lb.SelectedItem;
            vm.Settings.FilterPresets.Remove(fp);
        }
    }

    public class FilterPresetRenameCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = parameter as SettingWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            var lb = (ListBox)window.FilterPresetsListBox;
            var fp = (FilterPreset)lb.SelectedItem;
            var afw = new PresetNameInputWindow(vm);
            afw.ShowActivated = true;
            afw.Owner = window;
            bool dr = (bool)afw.ShowDialog();
            if (dr)
            {
                string name = afw.PresetNameTextBox.Text;
                fp.Name = name;
            }
        }
    }

    public class FilterPresetMoveUpCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = parameter as SettingWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            var lb = (ListBox)window.FilterPresetsListBox;
            var fp = (FilterPreset)lb.SelectedItem;
            int i = vm.Settings.FilterPresets.IndexOf(fp);
            if (i > 0)
            {
                vm.Settings.FilterPresets.Move(i, i - 1);
            }
        }
    }

    public class FilterPresetMoveDownCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = parameter as SettingWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            var lb = (ListBox)window.FilterPresetsListBox;
            var fp = (FilterPreset)lb.SelectedItem;
            int i = vm.Settings.FilterPresets.IndexOf(fp);
            if (i < vm.Settings.FilterPresets.Count - 1)
            {
                vm.Settings.FilterPresets.Move(i, i + 1);
            }
        }
    }

    public class AddMarkerCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            var window = Window;
            var vm = ViewModel;

            if (vm.MarkerList.Count == 0)
            {
                vm.MarkerList.Add(vm.Position);
            }
            else
            {
                for (int i = 0; i < vm.MarkerList.Count; i++)
                {
                    if (vm.MarkerList[i] == vm.Position)
                    {
                        vm.MarkerList.Remove(vm.Position);
                        return;
                    }
                    else if (vm.MarkerList[i] > vm.Position)
                    {
                        vm.MarkerList.Insert(i, vm.Position);
                        return;
                    }
                    else if (i == vm.MarkerList.Count - 1)
                    {
                        vm.MarkerList.Add(vm.Position);
                        return;
                    }
                }
            }
        }
    }

    public class ClearMarkerCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.MarkerList.Clear();
        }
    }

    public class LinkSlidersCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            ViewModel.Settings.IsSliderLinked = !ViewModel.Settings.IsSliderLinked;
        }
    }
}
