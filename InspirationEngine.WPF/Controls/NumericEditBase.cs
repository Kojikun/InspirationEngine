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
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:InspirationEngine.WPF.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:InspirationEngine.WPF.Controls;assembly=InspirationEngine.WPF.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:NumericEdit/>
    ///
    /// </summary>
    [TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_UpButton, Type = typeof(RepeatButton))]
    [TemplatePart(Name = PART_DownButton, Type = typeof(RepeatButton))]
    public abstract class NumericEditBase : Control
    {
        #region Template Part Names

        public const string PART_TextBox = "PART_TextBox";
        public const string PART_UpButton = "PART_UpButton";
        public const string PART_DownButton = "PART_DownButton";

        #endregion
        /// <summary>
        /// The value displayed by the text box
        /// </summary>

        public GridLength ButtonWidth
        {
            get { return (GridLength)GetValue(ButtonWidthProperty); }
            set { SetValue(ButtonWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.Register("ButtonWidth", typeof(GridLength), typeof(NumericEditBase), new PropertyMetadata(GridLength.Auto));

        /// <summary>
        /// Defines whether the textbox can be typed in
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="IsReadOnly"/> in that 
        /// the buttons and mouse events can still edit the value even if this property is false.
        /// </remarks>
        public bool IsTextEditable
        {
            get { return (bool)GetValue(IsTextEditableProperty); }
            set { SetValue(IsTextEditableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTextEditable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextEditableProperty =
            DependencyProperty.Register("IsTextEditable", typeof(bool), typeof(NumericEditBase), new PropertyMetadata(true));

        /// <summary>
        /// Defines whether the <see cref="Value"/> can be editable via user interaction with this control.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NumericEditBase), new PropertyMetadata(false));

        /// <summary>
        /// Sets the Text Alignment of the displayed value in the TextBox
        /// </summary>
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(NumericEditBase), new PropertyMetadata(TextAlignment.Left));

        /// <summary>
        /// The number of pixels that the mouse must move while dragging to increment by the IncrementValue
        /// </summary>
        public double IncrementDistance
        {
            get { return (double)GetValue(IncrementDistanceProperty); }
            set { SetValue(IncrementDistanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IncrementDistance.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IncrementDistanceProperty =
            DependencyProperty.Register("IncrementDistance", typeof(double), typeof(NumericEditBase), new PropertyMetadata(10d));



        //static NumericEdit()
        //{
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericEdit), new FrameworkPropertyMetadata(typeof(NumericEdit)));
        //}

        public NumericEditBase()
        {
            DefaultStyleKey = typeof(NumericEditBase);
        }
    }



}
