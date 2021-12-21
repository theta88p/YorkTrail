/*
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
