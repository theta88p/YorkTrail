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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YorkTrail
{
    public class FilterPreset : INotifyPropertyChanged
    {
        public FilterPreset()
        {
        }

        public FilterPreset(string name, bool lpfEnabled, bool hpfEnbaled, bool bpfEnabled, float lpfFreq, float hpfFreq, float bpfFreq)
        {
            this.Name = name;
            this.LpfEnabled = lpfEnabled;
            this.HpfEnbled = hpfEnbaled;
            this.BpfEnabled = bpfEnabled;
            this.LpfFreq = lpfFreq;
            this.HpfFreq = hpfFreq;
            this.BpfFreq = bpfFreq;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _name;
        public string Name {
            get { return _name; }
            set {
                _name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }
        public bool LpfEnabled { get; set; }
        public bool HpfEnbled { get; set; }
        public bool BpfEnabled { get; set; }
        public float LpfFreq { get; set; }
        public float HpfFreq { get; set; }
        public float BpfFreq { get; set; }
    }
}
