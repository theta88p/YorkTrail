using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace YorkTrail
{
    public class TempoCalcWindowViewModel : INotifyPropertyChanged
    {
        public TempoCalcWindowViewModel()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private float _tempo;
        public MainWindowViewModel MainWindowViewModel { get; set; }
        public float Tempo {
            get { return _tempo; }
            set {
                _tempo = value;
                RaisePropertyChanged(nameof(Tempo));
            }
        }
        private ulong _startTime = 0;
        public ulong StartTime {
            get { return _startTime; }
            set {
                _startTime = value;
                RaisePropertyChanged(nameof(StartTime));
            }
        }
        private ulong _endTime = 0;
        public ulong EndTime
        {
            get { return _endTime; }
            set {
                _endTime = value;
                RaisePropertyChanged(nameof(EndTime));
            }
        }
        public int TimeSignature { get; set; } = 4;
        public int Bar { get; set; } = 8;

        public InputStartTimeButtonCommand InputStartTimeButtonCommand { get; private set; } = new InputStartTimeButtonCommand();
        public InputEndTimeButtonCommand InputEndTimeButtonCommand { get; private set; } = new InputEndTimeButtonCommand();

        public void CalcTempo()
        {
            double tempo = 60.0f / ((float)(EndTime - StartTime) / 1000) * Bar * TimeSignature;
            Tempo = (float)Math.Round(tempo, 2);
        }

        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcTempo();
        }

        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalcTempo();
        }
    }
}
