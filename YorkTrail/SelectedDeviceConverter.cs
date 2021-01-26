using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace YorkTrail
{
    public class SelectedDeviceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> list = values[0] as List<string>;
            if (list != null)
            {
                int n = (int)values[1];
                string header = values[2] as string;

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == header)
                    {
                        return n == i;
                    }
                }
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
