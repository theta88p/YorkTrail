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
            float min = float.Parse(minmax[0]);
            float max = float.Parse(minmax[1]);
            float x = (float)value;
            double y = Math.Pow((x - min) / (max - min), 1.0 / 3.0);
            Debug.WriteLine(x + "/" + y);
            return (float)y;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] minmax = ((string)parameter).Split("-");
            float min = float.Parse(minmax[0]);
            float max = float.Parse(minmax[1]);
            float x = (float)(double)value;
            float y = (float)((max - min) * Math.Pow(x, 3.0) + min);
            Debug.WriteLine(x + "/" + y);
            return y;
        }
    }
}
