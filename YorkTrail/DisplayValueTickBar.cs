using System;
using System.Collections.Generic;
using System.Drawing;
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
        public ulong TotalMilliSeconds
        {
            get { return (ulong)GetValue(TotalMilliSecondsProperty); }
            set { SetValue(TotalMilliSecondsProperty, value); }
        }

        public static readonly DependencyProperty TotalMilliSecondsProperty =
            DependencyProperty.Register("TotalMilliSeconds", typeof(ulong), typeof(DisplayValueTickBar), new FrameworkPropertyMetadata(0ul, FrameworkPropertyMetadataOptions.AffectsRender));

        protected override void OnRender(DrawingContext dc)
        {
            //int tickCount = (int)((this.Maximum - this.Minimum) / this.TickFrequency);
            //if ((this.Maximum - this.Minimum) % this.TickFrequency > 0)
            //    tickCount++;
            if (TotalMilliSeconds <= 0)
            {
                return;
            }

            uint span = 0;
            uint vSpan = 0;
            ulong selectedLength = (ulong)(TotalMilliSeconds - (1 - base.Maximum + base.Minimum) * TotalMilliSeconds);

            if (selectedLength > 600000)
            {
                span = 300000;
                vSpan = 600000;
            }
            else if (selectedLength > 300000)
            {
                span = 60000;
                vSpan = 300000;
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

            for (uint i = (uint)(base.Minimum * TotalMilliSeconds / span); i <= (uint)(base.Maximum * TotalMilliSeconds / span); i++)
            {
                uint tick = span * i;
                double pos = ((double)tick - base.Minimum * TotalMilliSeconds) / (TotalMilliSeconds - (1 - base.Maximum + base.Minimum) * TotalMilliSeconds);
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
                    dc.DrawLine(pen, new System.Windows.Point(x, -2), new System.Windows.Point(x, -8));

                    System.Windows.Point p = new System.Windows.Point(x - 8, -24);
                    var ms = TimeSpan.FromMilliseconds(tick);

                    FormattedText formattedText = new FormattedText(ms.ToString(@"m\:ss"), CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, new Typeface("Yu Gothic UI"), 12, Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    dc.DrawText(formattedText, p);
                }
                else
                {
                    dc.DrawLine(pen, new System.Windows.Point(x, -2), new System.Windows.Point(x, -6));
                }
            }
        }
    }
}
