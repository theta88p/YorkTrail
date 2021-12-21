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
