using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InspirationEngine.WPF.Controls
{

    public partial class MinMaxSlider : Slider
    {
        /// <summary>
        /// The value of the thumb that defines the lowerbound of the range
        /// </summary>
        public double LowerValue
        {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); }
        }
        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register("LowerValue", typeof(double), typeof(MinMaxSlider), new UIPropertyMetadata(0d, null, LowerValueCoerceValueCallback));

        /// <summary>
        /// The value of the thumb that defines the upperbound of the range
        /// </summary>
        public double UpperValue
        {
            get { return (double)GetValue(UpperValueProperty); }
            set { SetValue(UpperValueProperty, value); }
        }
        public static readonly DependencyProperty UpperValueProperty =
            DependencyProperty.Register("UpperValue", typeof(double), typeof(MinMaxSlider), new UIPropertyMetadata(1d, null, UpperValueCoerceValueCallback));


        static MinMaxSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MinMaxSlider), new FrameworkPropertyMetadata(typeof(MinMaxSlider)));
        }

        /// <summary>
        /// Invoked when value correction is required in case the lower value exceeds the upper value
        /// </summary>
        /// <param name="target"></param>
        /// <param name="valueObject"></param>
        /// <returns></returns>
        private static object LowerValueCoerceValueCallback(DependencyObject target, object valueObject)
        {
            MinMaxSlider targetSlider = (MinMaxSlider)target;
            double value = (double)valueObject;

            return Math.Min(value, targetSlider.UpperValue);
        }

        /// <summary>
        /// Invoked when value correction is required in case the upper value precedes the lower value
        /// </summary>
        /// <param name="target"></param>
        /// <param name="valueObject"></param>
        /// <returns></returns>
        private static object UpperValueCoerceValueCallback(DependencyObject target, object valueObject)
        {
            MinMaxSlider targetSlider = (MinMaxSlider)target;
            double value = (double)valueObject;

            return Math.Max(value, targetSlider.LowerValue);
        }
    }
}
