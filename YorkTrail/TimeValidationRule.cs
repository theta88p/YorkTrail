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
