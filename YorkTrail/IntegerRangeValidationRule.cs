using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace YorkTrail
{
    public class IntegerRangeValidationRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public bool NoMax { get; set; }
        public bool NoMin { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int res = 0;
            Min = (NoMin) ? int.MinValue : Min;
            Max = (NoMax) ? int.MaxValue : Max;

            if (int.TryParse((string)value, out res) && res >= Min && res <= Max)
            {
                return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, "入力形式が不正です");
            }
        }
    }
}
