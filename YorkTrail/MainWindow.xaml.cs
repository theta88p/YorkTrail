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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace YorkTrail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = (MainWindowViewModel)this.DataContext;
            vm.Window = this;
            CommandCollection.SetWindowInstance(this);
        }
        private void FileDrop(object sender, DragEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).FileDrop(sender, e);
        }
        private void RangeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).RangeSlider_MouseLeftButtonUp(sender, e);
        }
        private void RangeSlider_LowerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((MainWindowViewModel)this.DataContext).RangeSlider_LowerValueChanged(sender, e);
        }
        private void RangeSlider_LowerSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).RangeSlider_LowerSliderDragCompleted(sender, e);
        }
        private void RecentFile_Clicked(object sender, ExecutedRoutedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).RecentFile_Clicked(sender, e);
        }
        private void PlaybackDevice_Clicked(object sender, ExecutedRoutedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).PlaybackDevice_Clicked(sender, e);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).MainWindow_Closing(sender, e);
        }

        private void FilterPreset_Clicked(object sender, ExecutedRoutedEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).FilterPreset_Clicked(sender, e);
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).MainWindow_SourceInitialized(sender, e);
        }

        private void SeekBar_DisplayValueTickBarMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).SeekBar_DisplayValueTickBarMouseLeftButtonUp(sender, e);
        }

        private void PitchStackPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).PitchStackPanel_MouseRightButtonUp(sender, e);
        }

        private void RatioStackPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext).RatioStackPanel_MouseRightButtonUp(sender, e);
        }
    }
}
