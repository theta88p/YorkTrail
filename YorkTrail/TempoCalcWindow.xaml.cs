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
using System.ComponentModel;

namespace YorkTrail
{
    /// <summary>
    /// TempoCalc.xaml の相互作用ロジック
    /// </summary>
    public partial class TempoCalcWindow : Window
    {
        public TempoCalcWindow(MainWindowViewModel vm)
        {
            InitializeComponent();
            ((TempoCalcWindowViewModel)this.DataContext).MainWindowViewModel = vm;
            ((TempoCalcWindowViewModel)this.DataContext).Window = this;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((TempoCalcWindowViewModel)this.DataContext).ComboBox_SelectionChanged(sender, e);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ((TempoCalcWindowViewModel)this.DataContext).Window_Closing(sender, e);
        }

        private void TempoOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TempoCalcWindowViewModel)this.DataContext).TempoOutput_TextChanged(sender, e);
        }

        private void StartTimeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TempoCalcWindowViewModel)this.DataContext).StartTimeInput_TextChanged(sender, e);
        }

        private void EndTimeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TempoCalcWindowViewModel)this.DataContext).MeasureInput_TextChanged(sender, e);
        }

        private void MeasureInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TempoCalcWindowViewModel)this.DataContext).MeasureInput_TextChanged(sender, e);
        }
    }
}
