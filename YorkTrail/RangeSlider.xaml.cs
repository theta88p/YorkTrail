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
        void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            LowerSlider.ValueChanged += LowerSlider_ValueChanged;
            UpperSlider.ValueChanged += UpperSlider_ValueChanged;
        }

        private void LowerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpperSlider.Value = Math.Max(UpperSlider.Value, LowerSlider.Value);
            if (LowerValueChanged != null)
            {
                this.LowerValueChanged(this, e);
            }
        }

        private void UpperSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LowerSlider.Value = Math.Min(UpperSlider.Value, LowerSlider.Value);
            if (UpperValueChanged != null)
            {
                this.UpperValueChanged(this, e);
            }
        }

        public event RoutedPropertyChangedEventHandler<double> LowerValueChanged;
        public event RoutedPropertyChangedEventHandler<double> UpperValueChanged;
        public event DragCompletedEventHandler LowerSliderDragCompleted;
        public event DragCompletedEventHandler UpperSliderDragCompleted;

        private void LowerSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (LowerSliderDragCompleted != null)
            {
                this.LowerSliderDragCompleted(sender, e);
            }
        }

        private void UpperSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (UpperSliderDragCompleted != null)
            {
                this.UpperSliderDragCompleted(sender, e);
            }
        }


        public double Minimum {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double LowerValue {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); }}

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


    }
}
