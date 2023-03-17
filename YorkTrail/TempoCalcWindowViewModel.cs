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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Timer = System.Timers.Timer;

namespace YorkTrail
{
    public class TempoCalcWindowViewModel : INotifyPropertyChanged
    {
        public TempoCalcWindowViewModel()
        {
            autocalcTimer.Elapsed += (sender, e) =>
            {
                Window?.TempoOutput.Dispatcher.Invoke(() =>
                {
                    Window.TempoOutput.Text = MainWindowViewModel?.Core.GetBPM().ToString("#.##");
                });
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public TempoCalcWindow? Window { get; set; }
        public MainWindowViewModel? MainWindowViewModel { get; set; }

        private Timer autocalcTimer = new Timer(500);

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
        public float MeasureTime { get; set; } = 1;
        private bool _isAutoCalc = false;
        public bool IsAutoCalc
        {
            get { return _isAutoCalc; }
            set {
                _isAutoCalc = value;
                if (value)
                {
                    autocalcTimer.Start();
                }
                else
                {
                    autocalcTimer.Stop();
                }
            }
        }

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
                MeasureTime = 60000.0f / Tempo * TimeSignature;
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
            if (Window != null)
            {
                e.Cancel = true;
                Window.Visibility = Visibility.Collapsed;
            }
        }
    }
}
