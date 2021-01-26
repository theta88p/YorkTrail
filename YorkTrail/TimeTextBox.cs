using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace YorkTrail
{
    public class TimeTextBox : TextBox
    {
        private static readonly Regex regex = new Regex(@"^\d\d:\d\d:\d\d\.\d\d$");

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!regex.IsMatch(this.Text))
                e.Handled = true;
            base.OnPreviewTextInput(e);
        }
    }
}
