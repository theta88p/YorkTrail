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
            string root = Path.GetPathRoot(path);
            string[] dirs = Path.GetDirectoryName(path).Split("\\");
            string newpath = Path.GetFileName(path);
            for(int i = dirs.Length - 1; i >= 0; i--)
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}