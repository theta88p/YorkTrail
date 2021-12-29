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
using System.IO;

namespace YorkTrail
{
    public class PathShortenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = (string)value;
            string? root = Path.GetPathRoot(path);
            string? dir = Path.GetDirectoryName(path);
            if (dir != null)
            {
                string[] dirs = dir.Split("\\");
                string newpath = Path.GetFileName(path);
                for (int i = dirs.Length - 1; i >= 0; i--)
                {
                    if (newpath.Length > 50)
                    {
                        newpath = root + "...\\" + newpath;
                        break;
                    }
                    newpath = dirs[i] + "\\" + newpath;
                }
                return newpath;
            }
            return path;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}