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
    /// TempoCalc.xaml の相互作用ロジック
    /// </summary>
    public partial class TempoCalcWindow : Window
    {
        public TempoCalcWindow(MainWindowViewModel vm)
        {
            InitializeComponent();
            ((TempoCalcWindowViewModel)this.DataContext).MainWindowViewModel = vm;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TempoCalcWindowViewModel)this.DataContext).TextBox_TextChanged(sender, e);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((TempoCalcWindowViewModel)this.DataContext).ComboBox_SelectionChanged(sender, e);
        }
    }
}
