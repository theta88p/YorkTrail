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
    }
}
