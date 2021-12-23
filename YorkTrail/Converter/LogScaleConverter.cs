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
            double minv = Math.Log(double.Parse(minmax[0]));
            double maxv = Math.Log(double.Parse(minmax[1]));
            float p = (float)value;
            double res = (Math.Log(p) - minv) / (maxv - minv);
            Debug.WriteLine("LogScaleConverter Convert: " + p + "/" + res);
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] minmax = ((string)parameter).Split("-");
            double minv = Math.Log(double.Parse(minmax[0]));
            double maxv = Math.Log(double.Parse(minmax[1]));
            double p = (double)value;
            double res = Math.Exp(minv + (maxv - minv) * p);
            Debug.WriteLine("LogScaleConverter ConvertBack:" + p + "/" + res);
            return (float)res;
        }
    }
}
