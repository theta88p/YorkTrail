using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace YorkTrail
{
    class TimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.FromMilliseconds((ulong)value).ToString(@"hh\:mm\:ss\.ff");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan res;
            if (TimeSpan.TryParseExact((string)value, @"hh\:mm\:ss\.ff", null, out res))
            {
                return res.TotalMilliseconds;
            }
            else
            {
                return 0;
            }
        }
    }
}
