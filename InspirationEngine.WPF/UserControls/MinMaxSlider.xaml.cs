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

namespace InspirationEngine.WPF.UserControls
{
    /// <summary>
    /// Defines a Slider control with two thumbs to define a range of values
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/5395957/wpf-slider-with-two-thumbs"/>
    public partial class MinMaxSlider : UserControl
    {
        /// <summary>
        /// The minimum value of the entire slider
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(MinMaxSlider), new UIPropertyMetadata(0d));

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

        /// <summary>
        /// The maximum value of the entire slider
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(MinMaxSlider), new UIPropertyMetadata(1d));

        /// <summary>
        /// Whether or not to snap to ticks defined by <see cref="TickFrequency"/>
        /// </summary>
        public bool IsSnapToTickEnabled
        {
            get { return (bool)GetValue(IsSnapToTickEnabledProperty); }
            set { SetValue(IsSnapToTickEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsSnapToTickEnabledProperty =
            DependencyProperty.Register("IsSnapToTickEnabled", typeof(bool), typeof(MinMaxSlider), new UIPropertyMetadata(false));

        /// <summary>
        /// The difference between values to create a tick at
        /// </summary>
        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }
        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(double), typeof(MinMaxSlider), new UIPropertyMetadata(0.1d));

        /// <summary>
        /// Where the ticks will visually show on the control
        /// </summary>
        public TickPlacement TickPlacement
        {
            get { return (TickPlacement)GetValue(TickPlacementProperty); }
            set { SetValue(TickPlacementProperty, value); }
        }
        public static readonly DependencyProperty TickPlacementProperty =
            DependencyProperty.Register("TickPlacement", typeof(TickPlacement), typeof(MinMaxSlider), new UIPropertyMetadata(TickPlacement.None));

        /// <summary>
        /// An ordered collection of where the ticks exist on the slider
        /// </summary>
        public DoubleCollection Ticks
        {
            get { return (DoubleCollection)GetValue(TicksProperty); }
            set { SetValue(TicksProperty, value); }
        }
        public static readonly DependencyProperty TicksProperty =
            DependencyProperty.Register("Ticks", typeof(DoubleCollection), typeof(MinMaxSlider), new UIPropertyMetadata(null));


        /// <summary>
        /// Constructor
        /// </summary>
        public MinMaxSlider()
        {
            InitializeComponent();
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
