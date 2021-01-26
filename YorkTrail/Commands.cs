using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using System.Diagnostics;

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
            vm.Core.SeekRelative(2000);
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
            vm.Core.SeekRelative(-2000);
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
            vm.Core.SetPosition(1.0f);
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
            window.ProgressBar.Minimum = vm.StartPosition;
            window.ProgressBar.Maximum = vm.EndPosition;
            window.RangeSlider.Minimum = vm.StartPosition;
            window.RangeSlider.Maximum = vm.EndPosition;
            vm.IsZooming = true;
        }
    }
    public class ZoomResetCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            /*
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            if (vm != null)
            {
                return vm.IsZooming;
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
            if (window != null)
            {
                window.ProgressBar.Minimum = 0.0f;
                window.ProgressBar.Maximum = 1.0f;
                window.RangeSlider.Minimum = 0.0f;
                window.RangeSlider.Maximum = 1.0f;
                vm.IsZooming = false;
            }
        }
    }
    public class LpfOnCommand : EffectCommandBase
    {
        public override void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.UseLpf = window?.LpfOnButton.IsChecked ?? false;
        }
    }
    public class HpfOnCommand : EffectCommandBase
    {
        public override void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.UseHpf = window?.HpfOnButton.IsChecked ?? false;
        }
    }
    public class BpfOnCommand : EffectCommandBase
    {
        public override void Execute(object parameter)
        {
            var window = parameter as MainWindow;
            var vm = window?.DataContext as MainWindowViewModel;
            vm.UseBpf = window?.BpfOnButton.IsChecked ?? false;
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
            bool check = window?.BypassButton.IsChecked ?? false;
            vm.Core.SetBypass(check);
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
                vm.SetPath(ofd.FileName);
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
            vm.StartPosition = 0.0f;
            vm.EndPosition = 1.0f;
            // Dependency PropertyだとNotifyPropertyChanged呼ばれないので無理やり動かす
            window.RangeSlider.Dispatcher.Invoke(() => { window.RangeSlider.LowerValue = 0.0f; });
            window.RangeSlider.Dispatcher.Invoke(() => { window.RangeSlider.UpperValue = 1.0f; });

        }
    }
}
