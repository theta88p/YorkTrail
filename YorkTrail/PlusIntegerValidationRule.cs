using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace YorkTrail
{
    public class PlusIntegerValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int res = 0;

            if (int.TryParse((string)value, out res) && res >= 0)
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
