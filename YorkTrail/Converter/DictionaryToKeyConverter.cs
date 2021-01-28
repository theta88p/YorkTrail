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
    public class DictionaryToKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dic = value as Dictionary<string, KeyGesture>;
            KeyGesture kg;

            if (dic.TryGetValue(parameter as string, out kg))
            {
                if (targetType == typeof(Key))
                {
                    return kg.Key;
                }
                else if (targetType == typeof(ModifierKeys))
                {
                    return kg.Modifiers;
                }
            }

            if (targetType == typeof(Key))
            {
                return Key.None;
            }
            else if (targetType == typeof(ModifierKeys))
            {
                return ModifierKeys.None;
            }
            return Key.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
