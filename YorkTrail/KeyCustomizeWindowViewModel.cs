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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YorkTrail
{
    public class KeyCustomizeWindowViewModel
    {
        public KeyCustomizeWindowViewModel()
        {
            KeyList = new List<Key>();
            foreach(var k in Enum.GetValues(typeof(Key)))
            {
                KeyList.Add((Key)k);
            }
        }

        public MainWindowViewModel MainWindowViewModel { get; set; }
        public List<Key> KeyList { get; set; }

        public void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = (ListBox)sender;
            lb.ScrollIntoView(lb.SelectedItem);
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            MainWindowViewModel.SetKeyBinds();
        }
    }
}
