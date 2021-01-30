using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace YorkTrail
{
    class ModifiersToBooleanConverter : IValueConverter
    {
        public bool CtrlSelected { get; set; }
        public bool AltSelected { get; set; }
        public bool ShiftSelected { get; set; }
        public bool WindowsSelected { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)parameter == "Ctrl")
            {
                bool ret = ((ModifierKeys)value & ModifierKeys.Control) == ModifierKeys.Control;
                this.CtrlSelected = ret;
                return ret;
            }
            else if ((string)parameter == "Alt")
            {
                bool ret = ((ModifierKeys)value & ModifierKeys.Alt) == ModifierKeys.Alt;
                this.AltSelected = ret;
                return ret;
            }
            else if ((string)parameter == "Shift")
            {
                bool ret = ((ModifierKeys)value & ModifierKeys.Shift) == ModifierKeys.Shift;
                this.ShiftSelected = ret;
                return ret;
            }
            else if ((string)parameter == "Windows")
            {
                bool ret = ((ModifierKeys)value & ModifierKeys.Windows) == ModifierKeys.Windows;
                this.WindowsSelected = ret;
                return ret;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)parameter == "Ctrl")
            {
                this.CtrlSelected = (bool)value;
            }
            else if ((string)parameter == "Alt")
            {
                this.AltSelected = (bool)value;
            }
            else if ((string)parameter == "Shift")
            {
                this.ShiftSelected = (bool)value;
            }
            else if ((string)parameter == "Windows")
            {
                this.WindowsSelected = (bool)value;
            }

            var ret = ModifierKeys.None;

            if (this.CtrlSelected)
            {
                ret |= ModifierKeys.Control;
            }
            else
            {
                ret = ret & ~ModifierKeys.Control;
            }

            if (this.AltSelected)
            {
                ret |= ModifierKeys.Alt;
            }
            else
            {
                ret = ret & ~ModifierKeys.Alt;
            }

            if (this.ShiftSelected)
            {
                ret |= ModifierKeys.Shift;
            }
            else
            {
                ret = ret & ~ModifierKeys.Shift;
            }

            if (this.WindowsSelected)
            {
                ret |= ModifierKeys.Windows;
            }
            else
            {
                ret = ret & ~ModifierKeys.Windows;
            }

            return ret;
        }
    }
}
