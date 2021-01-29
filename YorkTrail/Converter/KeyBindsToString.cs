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
            var dic = value as Dictionary<string, ShortCutKey>;
            ShortCutKey kg;
            string ret = "";

            if (dic.TryGetValue(parameter as string, out kg))
            {
                ret = ShortCutKey.ConvertToString(kg);
            }
            return ret.Replace("Control", "Ctrl");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
