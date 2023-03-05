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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace YorkTrail
{
    /// <summary>
    /// RangeSlider.xaml の相互作用ロジック
    /// </summary>
    public partial class RangeSlider : UserControl
    {
        public RangeSlider()
        {
            InitializeComponent();

            this.Loaded += Slider_Loaded;
        }
        /*
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        */
        private double upperLowerDiff;
        private bool isTickBarMouseDown;
        private bool isSliderBackMouseDown;
        private double mouseMoveStartPos;
        private double mouseMoveCurrentPos;

        void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            LowerSlider.ValueChanged += LowerSlider_ValueChanged;
            UpperSlider.ValueChanged += UpperSlider_ValueChanged;
        }

        private void LowerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsSliderLinked && !isTickBarMouseDown)
            {
                UpperSlider.Value = LowerSlider.Value + upperLowerDiff;
                if (UpperSlider.Value >= UpperSlider.Maximum)
                {
                    LowerSlider.Value = UpperSlider.Value - upperLowerDiff;
                }
            }
            else
            {
                UpperSlider.Value = Math.Max(UpperSlider.Value, LowerSlider.Value);
            }

            if (LowerValueChanged != null)
            {
                this.LowerValueChanged(this, e);
            }
        }

        private void UpperSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsSliderLinked && !isTickBarMouseDown)
            {
                LowerSlider.Value = UpperSlider.Value - upperLowerDiff;
                if (LowerSlider.Value <= LowerSlider.Minimum)
                {
                    UpperSlider.Value = LowerSlider.Value + upperLowerDiff;
                }
            }
            else
            {
                LowerSlider.Value = Math.Min(UpperSlider.Value, LowerSlider.Value);
            }

            if (UpperValueChanged != null)
            {
                this.UpperValueChanged(this, e);
            }
        }

        public event RoutedPropertyChangedEventHandler<double>? LowerValueChanged;
        public event RoutedPropertyChangedEventHandler<double>? UpperValueChanged;
        public event DragCompletedEventHandler? LowerSliderDragCompleted;
        public event DragCompletedEventHandler? UpperSliderDragCompleted;
        public event MouseButtonEventHandler? DisplayValueTickBarMouseLeftButtonUp;

        private void DoSnapToTick(Slider slider)
        {
            var tl = DisplayValueTickBar.TickList;

            for (int i = 0; i <= tl.Count; i++)
            {
                double prev;
                double next;

                if (i == 0)
                {
                    prev = Minimum;
                    next = tl[0];
                }
                else if (i == tl.Count)
                {
                    prev = tl[i - 1];
                    next = 1.0;
                }
                else
                {
                    prev = tl[i - 1];
                    next = tl[i];
                }

                if (slider.Value >= prev && slider.Value < (prev + next) / 2)
                {
                    slider.Value = prev;
                    return;
                }
                else if (slider.Value >= (prev + next) / 2 && slider.Value < next)
                {
                    slider.Value = next;
                    return;
                }
            }

        }

        private void LowerSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (SnapToTick)
            {
                DoSnapToTick(LowerSlider);
            }

            if (LowerSliderDragCompleted != null)
            {
                this.LowerSliderDragCompleted(sender, e);
            }
        }

        private void UpperSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (SnapToTick)
            {
                DoSnapToTick(UpperSlider);
            }

            if (UpperSliderDragCompleted != null)
            {
                this.UpperSliderDragCompleted(sender, e);
            }
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isSliderBackMouseDown = false;
            Point p = e.GetPosition(SliderBack);
            LowerSlider.Value = ((Maximum - Minimum) * p.X / SliderBack.ActualWidth) + Minimum;
            if (SnapToTick)
            {
                DoSnapToTick(LowerSlider);
            }
            LowerSlider.IsHitTestVisible = true;
            SliderBack.ReleaseMouseCapture();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isSliderBackMouseDown = true;
            Point p = e.GetPosition(SliderBack);
            LowerSlider.IsHitTestVisible = false;
            LowerSlider.Value = ((Maximum - Minimum) * p.X / SliderBack.ActualWidth) + Minimum;
            SliderBack.CaptureMouse();
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSliderBackMouseDown)
            {
                return;
            }
            Point p = e.GetPosition(SliderBack);
            LowerSlider.Value = ((Maximum - Minimum) * p.X / SliderBack.ActualWidth) + Minimum;
        }

        private void Border_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var b = (Border)sender;
            Point p = e.GetPosition(b);
            UpperSlider.Value = ((Maximum - Minimum) * p.X / b.ActualWidth) + Minimum;
            if (SnapToTick)
            {
                DoSnapToTick(UpperSlider);
            }
        }

        private static void IsSliderLinkedPropertyChanged(DependencyObject dpObj, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                RangeSlider rs = (RangeSlider)dpObj;
                rs.upperLowerDiff = rs.UpperSlider.Value - rs.LowerSlider.Value;
            }
        }

        private void DisplayValueTickBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isTickBarMouseDown = true;
            mouseMoveStartPos = e.GetPosition(DisplayValueTickBar).X;
            DisplayValueTickBar.CaptureMouse();
        }

        private void DoSnapToLowerValue()
        {
            double diff = 0;
            double value = 0;

            var tl = DisplayValueTickBar.TickList;

            for (int i = 0; i <= tl.Count; i++)
            {
                double prev;
                double next;

                if (i == 0)
                {
                    prev = Minimum;
                    next = tl[0];
                }
                else if (i == tl.Count)
                {
                    prev = tl[i - 1];
                    next = 1.0;
                }
                else
                {
                    prev = tl[i - 1];
                    next = tl[i];
                }

                if (LowerSlider.Value >= prev && LowerSlider.Value < (prev + next) / 2)
                {
                    value = prev;
                    break;
                }
                else if (LowerSlider.Value >= (prev + next) / 2 && LowerSlider.Value < next)
                {
                    value = next;
                    break;
                }
            }

            diff = value - LowerSlider.Value;

            if (Minimum + diff >= 0 && Maximum + diff <= 1)
            {
                Minimum += diff;
                Maximum += diff;
                LowerSlider.Value = value;
                UpperSlider.Value += diff;
            }
        }

        private void DisplayValueTickBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (SnapToTick)
            {
                DoSnapToLowerValue();
            }

            if (DisplayValueTickBarMouseLeftButtonUp != null)
            {
                DisplayValueTickBarMouseLeftButtonUp(sender, e);
            }
            DisplayValueTickBar.ReleaseMouseCapture();

            isTickBarMouseDown = false;
        }
        /*
        private void DisplayValueTickBar_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseDown = false;

            if (SnapToTick)
            {
                DoSnapToLowerValue();
            }
        }
        */
        private void DisplayValueTickBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isTickBarMouseDown)
            {
                return;
            }

            mouseMoveCurrentPos = e.GetPosition(DisplayValueTickBar).X;
            double offsetX = mouseMoveCurrentPos - mouseMoveStartPos;
            double offsetValue = (Maximum - Minimum) * offsetX / ActualWidth;

            if (Minimum - offsetValue >= 0 && Maximum - offsetValue <= 1
                && LowerSlider.Value - offsetValue >= 0 && UpperSlider.Value - offsetValue <= 1)
            {
                Minimum -= offsetValue;
                Maximum -= offsetValue;
                LowerSlider.Value -= offsetValue;
                UpperSlider.Value -= offsetValue;
            }

            mouseMoveStartPos = mouseMoveCurrentPos;
        }

        public double Minimum {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double LowerValue {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); } }

        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register("LowerValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double UpperValue {
            get { return (double)GetValue(UpperValueProperty); }
            set { SetValue(UpperValueProperty, value); }
        }

        public static readonly DependencyProperty UpperValueProperty =
            DependencyProperty.Register("UpperValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double Maximum {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d));

        public ulong TotalMilliSeconds
        {
            get { return (ulong)GetValue(TotalMilliSecondsProperty); }
            set { SetValue(TotalMilliSecondsProperty, value); }
        }

        public static readonly DependencyProperty TotalMilliSecondsProperty =
            DependencyProperty.Register("TotalMilliSeconds", typeof(ulong), typeof(RangeSlider), new UIPropertyMetadata(0ul));

        public float Tempo
        {
            get { return (float)GetValue(TempoProperty); }
            set { SetValue(TempoProperty, value); }
        }

        public static readonly DependencyProperty TempoProperty =
            DependencyProperty.Register("Tempo", typeof(float), typeof(RangeSlider), new FrameworkPropertyMetadata(0.0f));

        public int MeasureOffset
        {
            get { return (int)GetValue(MeasureOffsetProperty); }
            set { SetValue(MeasureOffsetProperty, value); }
        }

        public static readonly DependencyProperty MeasureOffsetProperty =
            DependencyProperty.Register("MeasureOffset", typeof(int), typeof(RangeSlider), new FrameworkPropertyMetadata(0));

        public int TimeSignature
        {
            get { return (int)GetValue(TimeSignatureProperty); }
            set { SetValue(TimeSignatureProperty, value); }
        }

        public static readonly DependencyProperty TimeSignatureProperty =
            DependencyProperty.Register("TimeSignature", typeof(int), typeof(RangeSlider), new FrameworkPropertyMetadata(0));

        public bool IsSliderLinked
        {
            get { return (bool)GetValue(IsSliderLinkedProperty); }
            set { SetValue(IsSliderLinkedProperty, value); }
        }

        public static readonly DependencyProperty IsSliderLinkedProperty =
            DependencyProperty.Register("IsSliderLinked", typeof(bool), typeof(RangeSlider), new FrameworkPropertyMetadata(false, IsSliderLinkedPropertyChanged));

        public bool ShowTimeAtMeasure
        {
            get { return (bool)GetValue(ShowTimeAtMeasureProperty); }
            set { SetValue(ShowTimeAtMeasureProperty, value); }
        }

        public static readonly DependencyProperty ShowTimeAtMeasureProperty =
            DependencyProperty.Register("ShowTimeAtMeasure", typeof(bool), typeof(RangeSlider), new FrameworkPropertyMetadata(false));

        public bool SnapToTick
        {
            get { return (bool)GetValue(SnapToTickProperty); }
            set { SetValue(SnapToTickProperty, value); }
        }

        public static readonly DependencyProperty SnapToTickProperty =
            DependencyProperty.Register("SnapToTick", typeof(bool), typeof(RangeSlider), new FrameworkPropertyMetadata(false));

        public ObservableCollection<double> MarkerList
        {
            get { return (ObservableCollection<double>)GetValue(MarkerListProperty); }
            set { SetValue(MarkerListProperty, value); }
        }

        public static readonly DependencyProperty MarkerListProperty =
            DependencyProperty.Register("MarkerList", typeof(ObservableCollection<double>), typeof(RangeSlider), new FrameworkPropertyMetadata(null));

        public List<float> VolumeList
        {
            get { return (List<float>)GetValue(VolumeListProperty); }
            set { SetValue(VolumeListProperty, value); }
        }

        public static readonly DependencyProperty VolumeListProperty =
            DependencyProperty.Register("VolumeList", typeof(List<float>), typeof(RangeSlider), new FrameworkPropertyMetadata(null));

        public double Position
        {
            get { return (double)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(double), typeof(RangeSlider), new FrameworkPropertyMetadata(0.0));

    }
}
