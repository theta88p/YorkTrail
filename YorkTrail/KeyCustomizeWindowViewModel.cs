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
            var vm = MainWindowViewModel;

            foreach (InputBinding ib in vm.Window.InputBindings)
            {
                foreach(var kb in vm.Settings.KeyBinds)
                {
                    if (ib.Command.ToString() == kb.Key)
                    {
                        vm.Window.InputBindings.Remove(ib);
                        vm.Window.InputBindings.Add(new KeyBinding(ib.Command, kb.Value.Key, kb.Value.Modifiers));
                    }
                }
            }
        }
    }
}
