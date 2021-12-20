using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YorkTrail
{
    public static class EnumNameDictionary
    {
        public static ReadOnlyDictionary<StretchMethod, string> StretchMethodDictionary
            = new ReadOnlyDictionary<StretchMethod, string>(new Dictionary<StretchMethod, string>()
        {
            { StretchMethod.SoundTouch, "SoundTouch" },
            { StretchMethod.RubberBand, "Rubber Band" }
        });
    }
}
