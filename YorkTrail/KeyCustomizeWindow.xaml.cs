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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YorkTrail
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class KeyCustomizeWindow : Window
    {
        public KeyCustomizeWindow(MainWindowViewModel vm)
        {
            InitializeComponent();
            ((KeyCustomizeWindowViewModel)this.DataContext).MainWindowViewModel = vm;
            // MainWindowViewModelをセットした後バインドしないと表示されない
            this.KeyBinds.ItemsSource = vm.Settings.KeyBinds;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((KeyCustomizeWindowViewModel)this.DataContext).ListBox_SelectionChanged(sender, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ((KeyCustomizeWindowViewModel)this.DataContext).Window_Closed(sender, e);
        }
    }
}
