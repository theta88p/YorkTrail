using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public TempoCalcWindow Window { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }

        private float _tempo;
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
        public int Measure { get; set; } = 8;
        public uint MeasureTime { get; set; } = 1;


        public InputStartTimeButtonCommand InputStartTimeButtonCommand { get; private set; } = new InputStartTimeButtonCommand();
        public InputEndTimeButtonCommand InputEndTimeButtonCommand { get; private set; } = new InputEndTimeButtonCommand();

        private void CalcTempo()
        {
            double tempo = 60.0f / ((float)(EndTime - StartTime) / 1000) * Measure * TimeSignature;
            Tempo = (float)Math.Round(tempo, 2);
        }

        internal void TempoOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Tempo > 0 && !float.IsInfinity(Tempo))
            {
                MeasureTime = (uint)(60000.0f / Tempo * TimeSignature);
                //EndTime = StartTime + MeasureTime * (uint)Measure;

                if (MainWindowViewModel != null)
                {
                    MainWindowViewModel.Tempo = Tempo;
                    MainWindowViewModel.MeasureOffset = (int)(StartTime % MeasureTime);
                }
            }
        }

        internal void StartTimeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MainWindowViewModel != null && MeasureTime > 0)
                MainWindowViewModel.MeasureOffset = (int)(StartTime % MeasureTime);

            if (StartTime < EndTime)
                CalcTempo();
        }

        internal void EndTimeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StartTime < EndTime)
                CalcTempo();
        }

        public void MeasureInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcTempo();
        }

        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbox = (ComboBox)sender;
            TimeSignature = (int)cbox.SelectedValue;

            if (MainWindowViewModel != null)
                MainWindowViewModel.TimeSignature = TimeSignature;

            CalcTempo();
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Window.Visibility = Visibility.Collapsed;
        }
    }
}
