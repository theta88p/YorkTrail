using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Controls;

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
            vm.BlinkTimer.Stop();
            window.TimeDisplay.Opacity = 1.0;
            vm.Core.Start();
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
            vm.BlinkTimer.Stop();
            window.TimeDisplay.Opacity = 1.0;
            vm.Core.Stop();
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
            if (vm.Core.GetState() == State.Playing)
            {
                vm.BlinkTimer.Start();
            }
            else if (vm.Core.GetState() == State.Pausing)
            {
                vm.BlinkTimer.Stop();
                window.TimeDisplay.Opacity = 1.0;
            }
            vm.Core.Pause();
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
            if (vm.Core.GetState() == State.Pausing)
            {
                vm.BlinkTimer.Stop();
                window.TimeDisplay.Opacity = 1.0;
                vm.Core.Start();
            }
            vm.Core.SeekRelative(vm.Settings.SkipLengthMS);
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
            if (vm.Core.GetState() == State.Pausing)
            {
                vm.BlinkTimer.Stop();
                window.TimeDisplay.Opacity = 1.0;
                vm.Core.Start();
            }
            vm.Core.SeekRelative(-1 * vm.Settings.SkipLengthMS);
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
                vm.BlinkTimer.Stop();
                window.TimeDisplay.Opacity = 1.0;
                vm.Core.Start();
            }
            vm.Core.SetPosition(0.0f);
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
                vm.BlinkTimer.Stop();
                window.TimeDisplay.Opacity = 1.0;
                vm.Core.Start();
            }
            vm.Core.SetPosition(1.0f);
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

    public class ZoomCommand : ICommand
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
            vm.IsZooming = !vm.IsZooming;
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
                tcwindow.ShowActivated = true;
                tcwindow.Owner = window;
                tcwindow.Show();
            }
            else
            {
                vm.TempoCalcWindow.Close();
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
                vm.FileOpen(ofd.FileName, false);
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
            vm.IsZooming = false;

            vm.StartPosition = 0.0f;
            vm.EndPosition = 1.0f;
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
            vm.StartPosition = vm.Position;
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
            vm.EndPosition = vm.Position;
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
