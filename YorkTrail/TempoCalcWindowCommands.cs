using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var window = (TempoCalcWindow)parameter;
            var vm = (TempoCalcWindowViewModel)window.DataContext;
            var mwvm = vm.MainWindowViewModel;

            var sw = new Stopwatch();
            sw.Start();
            // レイテンシを考慮して-30する
            ulong time = mwvm.Time - 30;
            // 時間取得の時間も考慮する
            time -= (ulong)sw.ElapsedMilliseconds;
            sw.Stop();
            vm.StartTime = time;
            //Debug.WriteLine(sw.ElapsedMilliseconds);
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
            var window = (TempoCalcWindow)parameter;
            var vm = (TempoCalcWindowViewModel)window.DataContext;
            var mwvm = vm.MainWindowViewModel;

            var sw = new Stopwatch();
            sw.Start();
            // レイテンシを考慮して-30する
            ulong time = mwvm.Time - 30;
            // 時間取得の時間も考慮する
            time -= (ulong)sw.ElapsedMilliseconds;
            sw.Stop();
            vm.EndTime = time;
        }
    }
}
