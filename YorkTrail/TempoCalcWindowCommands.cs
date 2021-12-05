using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YorkTrail
{
    public class InputStartTimeButtonCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public virtual void Execute(object parameter)
        {
            var window = parameter as TempoCalcWindow;
            var vm = window?.DataContext as TempoCalcWindowViewModel;
            var mwvm = vm?.MainWindowViewModel;
            // レイテンシを考慮して-50する
            vm.StartTime = mwvm.Time - 50;
        }
    }
    public class InputEndTimeButtonCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public virtual void Execute(object parameter)
        {
            var window = parameter as TempoCalcWindow;
            var vm = window?.DataContext as TempoCalcWindowViewModel;
            var mwvm = vm?.MainWindowViewModel;
            vm.EndTime = mwvm.Time - 50;
        }
    }
}
