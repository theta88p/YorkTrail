using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace YorkTrail
{
    public class SliderIgnoreDelta : Slider
    {
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            //base.OnValueChanged(oldValue, newValue);
        }
    }
}
