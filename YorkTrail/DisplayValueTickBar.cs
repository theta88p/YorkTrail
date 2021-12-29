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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace YorkTrail
{
    public class DisplayValueTickBar : TickBar
    {
        public DisplayValueTickBar()
        {
        }

        public List<double> TickList { get; private set; } = new List<double>();

        public ulong TotalMilliSeconds
        {
            get { return (ulong)GetValue(TotalMilliSecondsProperty); }
            set { SetValue(TotalMilliSecondsProperty, value); }
        }

        public static readonly DependencyProperty TotalMilliSecondsProperty =
            DependencyProperty.Register("TotalMilliSeconds", typeof(ulong), typeof(DisplayValueTickBar), new FrameworkPropertyMetadata(0ul, FrameworkPropertyMetadataOptions.AffectsRender));

        public float Tempo
        {
            get { return (float)GetValue(TempoProperty); }
            set { SetValue(TempoProperty, value); }
        }

        public static readonly DependencyProperty TempoProperty =
            DependencyProperty.Register("Tempo", typeof(float), typeof(DisplayValueTickBar), new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.AffectsRender));

        public int MeasureOffset
        {
            get { return (int)GetValue(MeasureOffsetProperty); }
            set { SetValue(MeasureOffsetProperty, value); }
        }

        public static readonly DependencyProperty MeasureOffsetProperty =
            DependencyProperty.Register("MeasureOffset", typeof(int), typeof(DisplayValueTickBar), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        public int TimeSignature
        {
            get { return (int)GetValue(TimeSignatureProperty); }
            set { SetValue(TimeSignatureProperty, value); }
        }

        public static readonly DependencyProperty TimeSignatureProperty =
            DependencyProperty.Register("TimeSignature", typeof(int), typeof(DisplayValueTickBar), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool ShowTimeAtMeasure
        {
            get { return (bool)GetValue(ShowTimeAtMeasureProperty); }
            set { SetValue(ShowTimeAtMeasureProperty, value); }
        }

        public static readonly DependencyProperty ShowTimeAtMeasureProperty =
            DependencyProperty.Register("ShowTimeAtMeasure", typeof(bool), typeof(DisplayValueTickBar), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public ObservableCollection<double> MarkerList
        {
            get { return (ObservableCollection<double>)GetValue(MarkerListProperty); }
            set { SetValue(MarkerListProperty, value); }
        }

        public static readonly DependencyProperty MarkerListProperty =
            DependencyProperty.Register("MarkerList", typeof(ObservableCollection<double>), typeof(DisplayValueTickBar), new FrameworkPropertyMetadata(default(ObservableCollection<double>), FrameworkPropertyMetadataOptions.AffectsRender, OnMarkerListChanged));


        private static void OnMarkerListChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            INotifyCollectionChanged oldList = (INotifyCollectionChanged)args.OldValue;
            INotifyCollectionChanged newList = (INotifyCollectionChanged)args.NewValue;

            var ctrl = (DisplayValueTickBar)obj;
            //If the old list implements the INotifyCollectionChanged interface, then unsubscribe to CollectionChanged events.
            if (oldList != null)
                oldList.CollectionChanged -= ctrl.OnMarkerListCollectionChanged;
            //If the new list implements the INotifyCollectionChanged interface, then subscribe to CollectionChanged events.
            if (newList != null)
                newList.CollectionChanged += ctrl.OnMarkerListCollectionChanged;
        }

        private void OnMarkerListCollectionChanged(object? s, NotifyCollectionChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            // 透明に塗らないとクリック出来ない
            var bgpen = new Pen();
            bgpen.Thickness = 0;
            bgpen.Brush = Brushes.Transparent;
            dc.DrawRectangle(bgpen.Brush, bgpen, new Rect(0, 0, ActualWidth, ActualHeight));

            if (ShowTimeAtMeasure)
            {
                RenderMeasure(dc);
            }
            else
            {
                RenderTime(dc);
            }

            RenderMarker(dc);
        }

        private void GetSpan(ulong selectedLength, out int span, out int vSpan)
        {
            span = 200;
            vSpan = 1000;

            if (selectedLength > 3600000)
            {
                for (int i = 10; i > 0; i--)
                {
                    if ((long)selectedLength > 3600000 * i)
                    {
                        span = 600000 * i;
                        vSpan = 1800000 * i;
                        break;
                    }
                }
            }
            else if (selectedLength > 1800000)
            {
                span = 300000;
                vSpan = 600000;
            }
            else if (selectedLength > 1200000)
            {
                span = 200000;
                vSpan = 600000;
            }
            else if (selectedLength > 600000)
            {
                span = 60000;
                vSpan = 300000;
            }
            else if (selectedLength > 300000)
            {
                span = 60000;
                vSpan = 120000;
            }
            else if (selectedLength > 180000)
            {
                span = 30000;
                vSpan = 60000;
            }
            else if (selectedLength > 120000)
            {
                span = 20000;
                vSpan = 60000;
            }
            else if (selectedLength > 60000)
            {
                span = 10000;
                vSpan = 30000;
            }
            else if (selectedLength > 30000)
            {
                span = 5000;
                vSpan = 15000;
            }
            else if (selectedLength > 20000)
            {
                span = 2000;
                vSpan = 10000;
            }
            else if (selectedLength > 10000)
            {
                span = 1000;
                vSpan = 5000;
            }
            else if (selectedLength > 5000)
            {
                span = 500;
                vSpan = 2000;
            }
            else if (selectedLength > 2000)
            {
                span = 200;
                vSpan = 1000;
            }
            else
            {
                span = 100;
                vSpan = 1000;
            }
        }

        private void GetMeasureSpan(ulong selectedLength, out int span, out int vSpan, out int measureMult)
        {
            float factor = selectedLength * Tempo / 100;
            int measure = (int)(60000.0f / Tempo * TimeSignature);
            int vMult = 1;
            measureMult = 1;

            // ゼロ除算防止
            if (Tempo == 0 || TimeSignature == 0)
            {
                factor = selectedLength / 100;
                measure = 240000;
            }

            if (factor > 48000)
            {
                for (int i = 10; i > 0; i--)
                {
                    int m = 1;
                    for (int j = 1; j < i; j++)
                    {
                        m *= 2;
                    }

                    if (factor > 48000 * m)
                    {
                        measureMult = 2 * m;
                        vMult = 2;
                        break;
                    }
                }
            }
            else if (factor > 24000)
            {
                measureMult = 1;
                vMult = 2;
            }
            else
            {
                measureMult = 1;
                vMult = 1;
            }

            span = measure * measureMult;
            vSpan = measure * measureMult * vMult;
        }

        private void RenderMarker(DrawingContext dc)
        {
            var pen = new Pen();
            pen.Thickness = 0.5;
            pen.Brush = Brushes.Black;

            foreach (var m in MarkerList)
            {
                double pos = (m - Minimum) / (Maximum - Minimum);
                double x = ActualWidth * pos;

                if (x >= 0 || x <= ActualWidth)
                {
                    var start = new Point(x, 22);
                    var segments = new[]
                    {
                        new LineSegment(new Point(x - 4, 16), true),
                        new LineSegment(new Point(x + 4, 16), true)
                    };

                    var figure = new PathFigure(start, segments, true);
                    var geo = new PathGeometry(new[] { figure });
                    dc.DrawGeometry(Brushes.Yellow, pen, geo);
                }
            }
        }

        private void RenderTime(DrawingContext dc)
        {
            if (TotalMilliSeconds <= 0)
            {
                return;
            }
            int span = 1;
            int vSpan = 1;
            ulong selectedLength = (ulong)(TotalMilliSeconds - (1 - Maximum + Minimum) * TotalMilliSeconds);
            GetSpan(selectedLength, out span, out vSpan);
            TickList.Clear();

            for (int i = (int)(Minimum * TotalMilliSeconds / span); i <= (int)(Maximum * TotalMilliSeconds / span); i++)
            {
                int tick = span * i;
                double pos = ((double)tick - Minimum * TotalMilliSeconds) / (TotalMilliSeconds - (1 - Maximum + Minimum) * TotalMilliSeconds);
                TickList.Add((double)tick / TotalMilliSeconds);
                double x = ActualWidth * pos;
                if (x < -5 || x > ActualWidth + 5)
                {
                    continue;
                }

                var pen = new Pen();
                pen.Thickness = 0.75;
                pen.Brush = Brushes.Black;

                if (tick % vSpan == 0)
                {
                    dc.DrawLine(pen, new Point(x, 16), new Point(x, 22));

                    Point p = new Point(x - 8, 0);
                    var ms = TimeSpan.FromMilliseconds(tick);

                    FormattedText formattedText = new FormattedText(ms.ToString(@"m\:ss"), CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, new Typeface("Yu Gothic UI"), 12, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    dc.DrawText(formattedText, p);
                }
                else
                {
                    dc.DrawLine(pen, new Point(x, 18), new Point(x, 22));
                }
            }
        }

        private void RenderMeasure(DrawingContext dc)
        {
            if (TotalMilliSeconds <= 0 || Tempo == float.PositiveInfinity)
            {
                return;
            }

            int span = 1;
            int vSpan = 1;
            int measureMult = 1;
            ulong selectedLength = (ulong)(TotalMilliSeconds - (1 - Maximum + Minimum) * TotalMilliSeconds);
            GetMeasureSpan(selectedLength, out span, out vSpan, out measureMult);
            TickList.Clear();

            for (int i = (int)(Minimum * TotalMilliSeconds / span); i <= (int)(Maximum * TotalMilliSeconds / span); i++)
            {
                int tick = span * i + MeasureOffset;
                double pos = ((double)tick - Minimum * TotalMilliSeconds) / (TotalMilliSeconds - (1 - Maximum + Minimum) * TotalMilliSeconds);
                TickList.Add((double)tick / TotalMilliSeconds);
                double x = ActualWidth * pos;
                if (x < -5 || x > ActualWidth + 5)
                {
                    continue;
                }

                var pen = new Pen();
                pen.Thickness = 0.75;
                pen.Brush = Brushes.Black;

                if (tick % vSpan == MeasureOffset)
                {
                    dc.DrawLine(pen, new Point(x, 16), new Point(x, 22));

                    Point p = new Point(x - 4, 0);
                    int measureNum = measureMult * i;

                    FormattedText formattedText = new FormattedText(measureNum.ToString(), CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, new Typeface("Yu Gothic UI"), 12, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    dc.DrawText(formattedText, p);
                }
                else
                {
                    dc.DrawLine(pen, new Point(x, 18), new Point(x, 22));
                }
            }
        }
    }
}
