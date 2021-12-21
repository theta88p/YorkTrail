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
using System.Windows.Input;

namespace YorkTrail
{
    public class ShortCutKey
    {
        public ShortCutKey()
        {

        }

        public ShortCutKey(Key key)
        {
            this.Key = key;
        }

        public ShortCutKey(Key key, ModifierKeys mod)
        {
            this.Key = key;
            this.Modifiers = mod;
        }

        public Key Key { get; set; } = Key.None;
        public ModifierKeys Modifiers { get; set; } = ModifierKeys.None;

        public static string ConvertToString(ShortCutKey key)
        {
            if (key.Modifiers == ModifierKeys.None)
            {
                return key.Key.ToString();
            }
            else
            {
                return key.Modifiers.ToString() + "+" + key.Key.ToString();
            }
        }

        public static ShortCutKey ConvertFromString(string value)
        {
            var kgvs = new KeyGestureConverter();
            var sv = value.Split('+');
            string key = "Control+" + sv.Last<string>();
            if (sv.Length > 1)
            {
                string mod = sv.First<string>().Replace(", ", "+") + "+F1";
                var kk = (KeyGesture)kgvs.ConvertFromString(key);
                var kg = (KeyGesture)kgvs.ConvertFromString(mod);
                return new ShortCutKey(kk.Key, kg.Modifiers);
            }
            else
            {
                var kk = (KeyGesture)kgvs.ConvertFromString(key);
                return new ShortCutKey(kk.Key, ModifierKeys.None);
            }
        }
    }
}
