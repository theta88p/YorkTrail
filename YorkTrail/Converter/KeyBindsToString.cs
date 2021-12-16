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
    class KeyBindsToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dic = (Dictionary<CommandName, ShortCutKey>)value;
            CommandName cmd;
            ShortCutKey key;
            string ret = "";

            if (Enum.TryParse((string)parameter, out cmd))
            {
                if (dic.TryGetValue(cmd, out key))
                {
                    ret = ShortCutKey.ConvertToString(key);
                }
            }
            return ret.Replace("Control", "Ctrl");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
