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
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow(MainWindowViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SoundTouchSettings_SourceUpdated(object sender, RoutedEventArgs e)
        {
            var vm = (MainWindowViewModel)this.DataContext;
            vm.Core.SetSoundTouchParam(vm.Settings.SoundTouchSequenceMS, vm.Settings.SoundTouchSeekWindowMS, vm.Settings.SoundTouchOverlapMS);
        }
    }
}
