using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using InspirationEngine.WPF.Utilities;

namespace InspirationEngine.WPF.Controls
{
    public class NumericEdit<T> : NumericEditBase where T : IEquatable<T>
    {
        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(T), typeof(NumericEdit<T>), new UIPropertyMetadata(default(T), (s, e) =>
            {
                if (s is NumericEdit<T> self && e.OldValue is T oldVal && e.NewValue is T newVal)
                {
                    var difference = GenericOperations<T, T, T>.Subtract(newVal, oldVal);
                    if (!GenericOperations<T, T>.Equal(difference, default))
                    {
                        self.RaiseEvent(new ValueChangedEventArgs(ValueChangedEvent, oldVal, newVal));
                        if (GenericOperations<T, T>.Equal(difference, self.IncrementValue))
                            self.RaiseEvent(new ValueChangedEventArgs(IncrementedEvent, oldVal, newVal));
                        else if (GenericOperations<T, T>.Equal(difference, GenericOperations<T, T, T>.Subtract(default, self.IncrementValue)))
                            self.RaiseEvent(new ValueChangedEventArgs(DecrementedEvent, oldVal, newVal));
                    }
                }
            }));


        public T IncrementValue
        {
            get { return (T)GetValue(IncrementValueProperty); }
            set { SetValue(IncrementValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IncrementValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IncrementValueProperty =
            DependencyProperty.Register("IncrementValue", typeof(T), typeof(NumericEdit<T>), new UIPropertyMetadata(default));




        public override void OnApplyTemplate()
        {
            TextBox = GetTemplateChild(PART_TextBox) as TextBox;
            UpButton = GetTemplateChild(PART_UpButton) as RepeatButton;
            DownButton = GetTemplateChild(PART_DownButton) as RepeatButton;
        }

        private void Increment() =>
            Value = GenericOperations<T, T, T>.Add(Value, IncrementValue);

        private void Decrement() =>
            Value = GenericOperations<T, T, T>.Subtract(Value, IncrementValue);


        private TextBox _textBox;
        private TextBox TextBox
        {
            get => _textBox;
            set
            {
                if (_textBox is not null)
                {
                    _textBox.MouseWheel -= new MouseWheelEventHandler(MouseWheelHandler);
                    _textBox.PreviewMouseDown -= new MouseButtonEventHandler(MouseDownHandler);
                    _textBox.MouseMove -= new MouseEventHandler(MouseMoveHandler);
                    _textBox.PreviewMouseUp -= new MouseButtonEventHandler(MouseUpHandler);
                }
                _textBox = value;
                if (_textBox is not null)
                {
                    _textBox.MouseWheel += new MouseWheelEventHandler(MouseWheelHandler);
                    _textBox.PreviewMouseDown += new MouseButtonEventHandler(MouseDownHandler);
                    _textBox.MouseMove += new MouseEventHandler(MouseMoveHandler);
                    _textBox.PreviewMouseUp += new MouseButtonEventHandler(MouseUpHandler);

                }
            }
        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                Increment();

            else if (e.Delta < 0)
                Decrement();
        }

        private (Point Point, T Value) MouseDownInfo { get; set; }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled || IsReadOnly)
                return;
            e.Handled = true;

            TextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            TextBox.SelectionLength = 0;
            MouseDownInfo = (e.GetPosition(this), Value);
            TextBox.CaptureMouse();
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                e.GetPosition(this) is Point currentPoint &&
                (Math.Abs(currentPoint.X - MouseDownInfo.Point.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(currentPoint.Y - MouseDownInfo.Point.Y) >= SystemParameters.MinimumVerticalDragDistance))
            {
                var distance = MouseDownInfo.Point.Y - currentPoint.Y;
                Value = GenericOperations<T, T, T>.Add(MouseDownInfo.Value, GenericOperations<T, double, T>.Multiply(IncrementValue, Math.Round(distance / IncrementDistance)));
            }
        }

        private void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled)
                return;
            e.Handled = true;

            TextBox.ReleaseMouseCapture();
            if (e.GetPosition(this).Y == MouseDownInfo.Point.Y)
            {
                TextBox.Focus();
                TextBox.SelectAll();
            }
        }

        private RepeatButton _upButton;
        private RepeatButton UpButton
        {
            get => _upButton;
            set
            {
                if (_upButton is not null)
                    _upButton.Click -= new RoutedEventHandler(UpButton_Click);

                _upButton = value;

                if (_upButton is not null)
                    _upButton.Click += new RoutedEventHandler(UpButton_Click);
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Increment();
        }

        private RepeatButton _downButton;
        private RepeatButton DownButton
        {
            get => _downButton;
            set
            {
                if (_downButton is not null)
                    _downButton.Click -= new RoutedEventHandler(DownButton_Click);

                _downButton = value;

                if (_downButton is not null)
                    _downButton.Click += new RoutedEventHandler(DownButton_Click);
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            Increment();
        }

        #region Events

        /// <summary>
        /// Represents a change in double value
        /// </summary>
        public class ValueChangedEventArgs : RoutedEventArgs
        {
            public ValueChangedEventArgs(RoutedEvent id, T oldVal, T newVal)
            {
                RoutedEvent = id;
                OldValue = oldVal;
                NewValue = newVal;
            }

            public T OldValue { get; set; }
            public T NewValue { get; set; }
        }

        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Direct,
                typeof(EventHandler<ValueChangedEventArgs>), typeof(NumericEditBase));

        public event EventHandler<ValueChangedEventArgs> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        public static readonly RoutedEvent IncrementedEvent =
            EventManager.RegisterRoutedEvent("Incremented", RoutingStrategy.Direct,
                typeof(EventHandler<ValueChangedEventArgs>), typeof(NumericEditBase));

        public event EventHandler<ValueChangedEventArgs> Incremented
        {
            add { AddHandler(IncrementedEvent, value); }
            remove { RemoveHandler(IncrementedEvent, value); }
        }

        public static readonly RoutedEvent DecrementedEvent =
            EventManager.RegisterRoutedEvent("Decremented", RoutingStrategy.Direct,
                typeof(EventHandler<ValueChangedEventArgs>), typeof(NumericEditBase));

        public event EventHandler<ValueChangedEventArgs> Decremented
        {
            add { AddHandler(DecrementedEvent, value); }
            remove { RemoveHandler(DecrementedEvent, value); }
        }

        #endregion
    }
}
