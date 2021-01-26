using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace YorkTrail
{
    public class TimeValidationRule : ValidationRule
    {
        private static readonly Regex regex = new Regex(@"^\d\d:\d\d:\d\d\.\d\d$");

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!regex.IsMatch((string)value))
            {
                return new ValidationResult(false, "入力形式が不正です");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
