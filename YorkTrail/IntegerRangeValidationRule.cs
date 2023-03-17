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
            double res = 0;
            Min = (NoMin) ? int.MinValue : Min;
            Max = (NoMax) ? int.MaxValue : Max;

            if (double.TryParse((string)value, out res) && res >= Min && res <= Max)
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
