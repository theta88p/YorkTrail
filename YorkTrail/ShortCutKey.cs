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
