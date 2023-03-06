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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace YorkTrail
{
    public class WaveFormViewer : Border
    {
        public WaveFormViewer()
        {
            SnapsToDevicePixels = true;
            
            var unplayedColor1 = Color.FromRgb(180, 180, 180);
            var unplayedColor2 = Color.FromRgb(90, 90, 90);
            var playedColor1 = Color.FromRgb(100, 210, 255);
            var playedColor2 = Color.FromRgb(100, 120, 205);

            fgpen.Thickness = 0;
            var grad1 = new LinearGradientBrush();
            grad1.StartPoint = new Point(0, 0);
            grad1.EndPoint = new Point(0, 1);
            grad1.GradientStops.Add(new GradientStop(unplayedColor1, 0.0));
            grad1.GradientStops.Add(new GradientStop(unplayedColor2, 1.0));
            fgpen.Brush = grad1;

            playedpen.Thickness = 0;
            var grad2 = new LinearGradientBrush();
            grad2.StartPoint = new Point(0, 0);
            grad2.EndPoint = new Point(0, 1);
            grad2.GradientStops.Add(new GradientStop(playedColor1, 0.0));
            grad2.GradientStops.Add(new GradientStop(playedColor2, 1.0));
            playedpen.Brush = grad2;

            stop1.Offset = 0.0;
            stop2.Offset = 1.0;

            playingpen.Thickness = 0;
            var grad3 = new LinearGradientBrush();
            grad3.StartPoint = new Point(0, 0);
            grad3.EndPoint = new Point(0, 1);
            grad3.GradientStops.Add(stop1);
            grad3.GradientStops.Add(stop2);
            playingpen.Brush = grad3;

            anime1.From = unplayedColor1;
            anime1.To = playedColor1;
            anime1.Duration = TimeSpan.FromMilliseconds(500);
            
            anime2.From = unplayedColor2;
            anime2.To = playedColor2;
            anime2.Duration = TimeSpan.FromMilliseconds(500);
        }

        private Pen fgpen = new Pen();
        private Pen playedpen = new Pen();
        private Pen playingpen = new Pen();
        private GradientStop stop1 = new GradientStop();
        private GradientStop stop2 = new GradientStop();
        private ColorAnimation anime1 = new ColorAnimation();
        private ColorAnimation anime2 = new ColorAnimation();
        private int CurrentBar { get; set; }
        private int PrevBar { get; set; }

        public double Position
        {
            get { return (double)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(double), typeof(WaveFormViewer), new FrameworkPropertyMetadata(0d, OnPositionPropertyChanged));

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(WaveFormViewer), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnPropertyChanged));
        
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(WaveFormViewer), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, OnPropertyChanged));

        public List<float> VolumeList
        {
            get { return (List<float>)GetValue(VolumeListProperty); }
            set { SetValue(VolumeListProperty, value); }
        }

        public static readonly DependencyProperty VolumeListProperty =
            DependencyProperty.Register("VolumeList", typeof(List<float>), typeof(WaveFormViewer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPropertyChanged));

        private static void OnPositionPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctrl = (WaveFormViewer)obj;
            ctrl.CurrentBar = (int)Math.Ceiling(ctrl.VolumeList.Count * ctrl.Position);
            if (ctrl.CurrentBar != ctrl.PrevBar)
            {
                ctrl.PrevBar = ctrl.CurrentBar;
                ctrl.InvalidateVisual();
            }
        }

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctrl = (WaveFormViewer)obj;
            ctrl.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // 透明に塗らないとクリック出来ない
            var bgpen = new Pen();
            bgpen.Thickness = 0;
            bgpen.Brush = Brushes.Transparent;
            dc.DrawRectangle(bgpen.Brush, bgpen, new Rect(0, 0, ActualWidth, ActualHeight));

            if (VolumeList.Count > 0)
            {
                // State復元時にVolumeList取得前にPositionが設定されてしまうのでここで再計算する
                CurrentBar = (int)Math.Ceiling(VolumeList.Count * Position);

                for (var i = 0; i < VolumeList.Count; i++)
                {
                    var barWidth = ActualWidth / VolumeList.Count / (Maximum - Minimum);
                    var barHeight = ActualHeight * (1 + VolumeList[i] / 70);
                    var x = (double)i / VolumeList.Count / (Maximum - Minimum) * ActualWidth - ActualWidth / (Maximum - Minimum) * Minimum;
                    var y = ActualHeight - barHeight;

                    if (x < 0 && x + barWidth > 0)
                    {
                        barWidth += x;
                        x = 0;
                    }
                    else if (x < ActualWidth && x + barWidth > ActualWidth)
                    {
                        barWidth -= x + barWidth - ActualWidth;
                    }
                    else if (x < 0 || x > ActualWidth)
                    {
                        continue;
                    }

                    if (i == CurrentBar - 1)
                    {
                        dc.DrawRectangle(playingpen.Brush, null, new Rect(x, y, barWidth * 0.9, barHeight));
                        stop1.BeginAnimation(GradientStop.ColorProperty, anime1);
                        stop2.BeginAnimation(GradientStop.ColorProperty, anime2);
                    }
                    else if (i < CurrentBar - 1)
                    {
                        dc.DrawRectangle(playedpen.Brush, null, new Rect(x, y, barWidth * 0.9, barHeight));
                    }
                    else
                    {
                        dc.DrawRectangle(fgpen.Brush, null, new Rect(x, y, barWidth * 0.9, barHeight));
                    }
                }
            }
        }
    }
}
