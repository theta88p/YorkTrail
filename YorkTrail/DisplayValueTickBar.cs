using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;

namespace YorkTrail
{
    public class DisplayValueTickBar : TickBar
    {
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

        public uint MeasureOffset
        {
            get { return (uint)GetValue(MeasureOffsetProperty); }
            set { SetValue(MeasureOffsetProperty, value); }
        }

        public static readonly DependencyProperty MeasureOffsetProperty =
            DependencyProperty.Register("MeasureOffset", typeof(uint), typeof(DisplayValueTickBar), new FrameworkPropertyMetadata(0u, FrameworkPropertyMetadataOptions.AffectsRender));

        public uint TimeSignature
        {
            get { return (uint)GetValue(TimeSignatureProperty); }
            set { SetValue(TimeSignatureProperty, value); }
        }

        public static readonly DependencyProperty TimeSignatureProperty =
            DependencyProperty.Register("TimeSignature", typeof(uint), typeof(DisplayValueTickBar), new FrameworkPropertyMetadata(0u, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool ShowTimeAtMeasure
        {
            get { return (bool)GetValue(ShowTimeAtMeasureProperty); }
            set { SetValue(ShowTimeAtMeasureProperty, value); }
        }

        public static readonly DependencyProperty ShowTimeAtMeasureProperty =
            DependencyProperty.Register("ShowTimeAtMeasure", typeof(bool), typeof(DisplayValueTickBar), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        

        protected override void OnRender(DrawingContext dc)
        {
            if (ShowTimeAtMeasure)
            {
                RanderMeasure(dc);
            }
            else
            {
                RanderTime(dc);
            }
        }

        private void GetSpan(ulong selectedLength, out uint span, out uint vSpan)
        {
            if (selectedLength > 6000000)
            {
                span = 600000;
                vSpan = 1200000;
            }
            else if (selectedLength > 1200000)
            {
                span = 300000;
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
            else if (selectedLength > 120000)
            {
                span = 30000;
                vSpan = 60000;
            }
            else if (selectedLength > 60000)
            {
                span = 10000;
                vSpan = 30000;
            }
            else if (selectedLength > 30000)
            {
                span = 10000;
                vSpan = 20000;
            }
            else if (selectedLength > 10000)
            {
                span = 5000;
                vSpan = 10000;
            }
            else if (selectedLength > 5000)
            {
                span = 1000;
                vSpan = 5000;
            }
            else if (selectedLength > 2000)
            {
                span = 500;
                vSpan = 1000;
            }
            else
            {
                span = 200;
                vSpan = 1000;
            }
        }

        private void GetBarSpan(ulong selectedLength, out uint span, out uint vSpan, out uint measureMult)
        {
            float factor = selectedLength * Tempo / 100;
            uint measure = (uint)(60000.0f / Tempo * TimeSignature);
            uint vMult = 1;
            measureMult = 1;

            if (Tempo == 0)
            {
                factor = selectedLength / 100;
                measure = 240000;
            }

            if (factor > 48000)
            {
                for (uint i = 10; i > 0; i--)
                {
                    uint m = 1;
                    for (uint j = 1; j < i; j++)
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

        private void RanderTime(DrawingContext dc)
        {
            if (TotalMilliSeconds <= 0)
            {
                return;
            }

            uint span = 1;
            uint vSpan = 1;
            ulong selectedLength = (ulong)(TotalMilliSeconds - (1 - Maximum + Minimum) * TotalMilliSeconds);
            GetSpan(selectedLength, out span, out vSpan);
            TickList.Clear();

            for (uint i = (uint)(Minimum * TotalMilliSeconds / span); i <= (uint)(Maximum * TotalMilliSeconds / span); i++)
            {
                uint tick = span * i;
                double pos = ((double)tick - Minimum * TotalMilliSeconds) / (TotalMilliSeconds - (1 - Maximum + Minimum) * TotalMilliSeconds);
                TickList.Add((double)tick / TotalMilliSeconds);
                double x = base.ActualWidth * pos;
                if (x < -10)
                {
                    continue;
                }

                var pen = new Pen();
                pen.Thickness = 0.75;
                pen.Brush = Brushes.Black;

                if (tick % vSpan == 0)
                {
                    dc.DrawLine(pen, new Point(x, -2), new Point(x, -8));

                    Point p = new Point(x - 8, -24);
                    var ms = TimeSpan.FromMilliseconds(tick);

                    FormattedText formattedText = new FormattedText(ms.ToString(@"m\:ss"), CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, new Typeface("Yu Gothic UI"), 12, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    dc.DrawText(formattedText, p);
                }
                else
                {
                    dc.DrawLine(pen, new Point(x, -2), new Point(x, -6));
                }
            }
        }

        private void RanderMeasure(DrawingContext dc)
        {
            if (TotalMilliSeconds <= 0 || Tempo == float.PositiveInfinity)
            {
                return;
            }

            uint span = 1;
            uint vSpan = 1;
            uint measureMult = 1;
            ulong selectedLength = (ulong)(TotalMilliSeconds - (1 - Maximum + Minimum) * TotalMilliSeconds);
            GetBarSpan(selectedLength, out span, out vSpan, out measureMult);
            TickList.Clear();

            for (uint i = (uint)(Minimum * TotalMilliSeconds / span); i <= (uint)(Maximum * TotalMilliSeconds / span); i++)
            {
                uint tick = span * i + MeasureOffset;
                double pos = ((double)tick - Minimum * TotalMilliSeconds) / (TotalMilliSeconds - (1 - Maximum + Minimum) * TotalMilliSeconds);
                TickList.Add((double)tick / TotalMilliSeconds);
                double x = base.ActualWidth * pos;

                var pen = new Pen();
                pen.Thickness = 0.75;
                pen.Brush = Brushes.Black;

                if (tick % vSpan == MeasureOffset)
                {
                    dc.DrawLine(pen, new Point(x, -2), new Point(x, -8));

                    Point p = new Point(x - 4, -24);
                    uint measureNum = measureMult * i;

                    FormattedText formattedText = new FormattedText(measureNum.ToString(), CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, new Typeface("Yu Gothic UI"), 12, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    dc.DrawText(formattedText, p);
                }
                else
                {
                    dc.DrawLine(pen, new Point(x, -2), new Point(x, -6));
                }
            }
        }
    }
}
