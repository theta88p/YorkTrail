using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;

namespace YorkTrail
{
    public class EffectCommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            var window = parameter as MainWindow;
            return !window?.BypassButton.IsChecked ?? true;
        }
        public virtual void Execute(object parameter)
        {
            // 何もしない
        }
    }

    public class PlayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Play();
        }
    }
    public class StopCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Stop();
        }
    }
    public class PauseCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Pause();
        }
    }
    public class FFCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.SeekRelative(vm.Settings.SkipLengthMS);
        }
    }
    public class FRCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.SeekRelative(-1 * vm.Settings.SkipLengthMS);
        }
    }
    public class ToStartCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            if (vm.Core.GetState() == State.Pausing)
            {
                vm.Play();
            }
            else if (vm.Core.GetState() == State.Stopped)
            {
                // 開始終了をリンクしている場合UIと内部で不整合が起きるためこうする
                window.SeekBar.LowerSlider.Value = 0.0;
            }
            vm.Position = 0.0;
        }
    }
    public class ToEndCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            if (vm.Core.GetState() == State.Pausing)
            {
                vm.Stop();
            }
            else if (vm.Core.GetState() == State.Stopped)
            {
                window.SeekBar.LowerSlider.Value = 1.0;
            }
            vm.Position = 1.0;
        }
    }
    public class StereoCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Channels = Channels.Stereo;
        }
    }
    public class MonoCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Channels = Channels.LPlusR;
        }
    }
    public class LOnlyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Channels = Channels.L;
        }
    }
    public class ROnlyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Channels = Channels.R;
        }
    }
    public class LMinusRCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Channels = Channels.LMinusR;
        }
    }
    public class PitchQuadCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Pitch = 4.0f;
        }
    }
    public class PitchDoubleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Pitch = 2.0f;
        }
    }
    public class PitchNormalCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Pitch = 1.0f;
        }
    }
    public class PitchHalfCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Pitch = 0.5f;
        }
    }
    public class TempoDoubleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Rate = 2.0f;
        }
    }
    public class TempoNormalCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Rate = 1.0f;
        }
    }
    public class TempoHalfCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Rate = 0.5f;
        }
    }
    public class TempoOneThirdCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Rate = 0.33f;
        }
    }
    public class TempoQuarterCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Rate = 0.25f;
        }
    }

    public class ZoomInCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            if (vm.ZoomMultiplier < 2048)
            {
                vm.ZoomMultiplier = Math.Max(4, vm.ZoomMultiplier * 2);
            }
            window.SeekBar.Minimum = Math.Max(0, vm.StartPosition - (vm.EndPosition - vm.StartPosition) / (vm.ZoomMultiplier * 0.1));
            window.SeekBar.Maximum = Math.Min(1, vm.EndPosition + (vm.EndPosition - vm.StartPosition) / (vm.ZoomMultiplier * 0.1));
        }
    }

    public class ZoomOutCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.ZoomMultiplier = Math.Max(4, vm.ZoomMultiplier / 2);
            if (vm.ZoomMultiplier > 4)
            {
                window.SeekBar.Minimum = Math.Max(0, vm.StartPosition - (vm.EndPosition - vm.StartPosition) / (vm.ZoomMultiplier * 0.1));
                window.SeekBar.Maximum = Math.Min(1, vm.EndPosition + (vm.EndPosition - vm.StartPosition) / (vm.ZoomMultiplier * 0.1));
            }
            else
            {
                window.SeekBar.Minimum = 0;
                window.SeekBar.Maximum = 1;
            }
        }
    }

    public class LpfOnCommand : EffectCommandBase
    {
        public override void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.LpfEnabled = window?.LpfOnButton.IsChecked ?? false;
        }
    }
    public class HpfOnCommand : EffectCommandBase
    {
        public override void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.HpfEnabled = window?.HpfOnButton.IsChecked ?? false;
        }
    }
    public class BpfOnCommand : EffectCommandBase
    {
        public override void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.BpfEnabled = window?.BpfOnButton.IsChecked ?? false;
        }
    }
    public class BypassCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.IsBypass = !vm.IsBypass;
        }
    }
    public class LoopCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.IsLoop = !vm.IsLoop;
        }
    }
    public class AlwaysOnTopCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Settings.AlwaysOnTop = !vm.Settings.AlwaysOnTop;
        }
    }
    public class ShowTimeAtMeasureCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Settings.ShowTimeAtMeasure = !vm.Settings.ShowTimeAtMeasure;
        }
    }
    public class SnapToTickCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.Settings.SnapToTick = !vm.Settings.SnapToTick;
        }
    }
    public class OpenTempoCalcWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
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
    public class FileOpenCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "音声ファイル (*.wav;*.mp3;*.flac)|*.wav;*.mp3;*.flac";
            if (ofd.ShowDialog() == true)
            {
                if (vm.FileOpen(ofd.FileName))
                    vm.Play();
            }
        }
    }
    public class FileCloseCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            if (vm != null)
            {
                return vm.Core.IsFileLoaded();
            }
            else
            {
                return false;
            }
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.FileClose();
        }
    }
    public class ExitCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            window.Close();
        }
    }

    public class ShowAboutCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowActivated = true;
            aboutWindow.Topmost = window.Topmost;
            aboutWindow.Owner = window;
            aboutWindow.ShowDialog();
        }
    }

    public class OpenKeyCustomizeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            var kcwindow = new KeyCustomizeWindow(vm);
            kcwindow.ShowActivated = true;
            kcwindow.Owner = window;
            kcwindow.ShowDialog();
        }
    }

    public class OpenSettingWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            var sw = new SettingWindow(vm);
            sw.ShowActivated = true;
            sw.Owner = window;
            sw.ShowDialog();
        }
    }

    public class SelectionResetCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            /*
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            if (vm != null)
            {
                return vm.StartPosition != 0.0f || vm.EndPosition != 1.0f;
            }
            else
            {
                return false;
            }
            */
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            window.SeekBar.Minimum = 0.0;
            window.SeekBar.Maximum = 1.0;
            vm.StartPosition = 0.0;
            vm.EndPosition = 1.0;
        }
    }

    public class CurrentToStartPositionCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            // ValueChangedの中で値を変更しているので、バインドしているプロパティを直に変更すると
            // 呼び出し順の関係上、開始終了をリンクしているとき不整合が起きる
            window.SeekBar.LowerSlider.Value = vm.Position;
        }
    }

    public class CurrentToEndPositionCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            window.SeekBar.UpperSlider.Value = vm.Position;
        }
    }

    public class OpenAddFilterPresetWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            var afw = new PresetNameInputWindow(vm);
            afw.ShowActivated = true;
            afw.Owner = window;
            bool dr = (bool)afw.ShowDialog();
            if (dr)
            {
                string name = afw.PresetNameTextBox.Text;
                vm.AddFilterPreset(name);
            }
        }
    }

    public class FilterPresetDeleteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            var window = parameter as SettingWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            var lb = (ListBox)window.FilterPresetsListBox;
            var fp = (FilterPreset)lb.SelectedItem;
            vm.Settings.FilterPresets.Remove(fp);
        }
    }

    public class FilterPresetRenameCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
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

    public class FilterPresetMoveUpCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
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

    public class FilterPresetMoveDownCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
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
}
