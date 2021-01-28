using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics;

namespace YorkTrail
{    public class LogScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] minmax = ((string)parameter).Split("-");
            double min = double.Parse(minmax[0]);
            double max = double.Parse(minmax[1]);
            double x = (double)value;
            double y = Math.Pow((x - min) / (max - min), 1.0 / 3.0);
            Debug.WriteLine(x + "/" + y);
            return y;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] minmax = ((string)parameter).Split("-");
            double min = double.Parse(minmax[0]);
            double max = double.Parse(minmax[1]);
            double x = (double)value;
            double y = (max - min) * Math.Pow(x, 3.0) + min;
            Debug.WriteLine(x + "/" + y);
            return y;
        }
    }
}
