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
            var dic = value as Dictionary<string, ShortCutKey>;
            ShortCutKey kg;

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
