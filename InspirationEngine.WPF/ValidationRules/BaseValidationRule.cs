using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace InspirationEngine.WPF.ValidationRules
{
    /// <summary>
    /// Allows XAML binding to a single ValidationRule
    /// </summary>
    /// <example>
    /// <TextBox xmlns:vr="clr-namespace:InspirationEngine.WPF.ValidationRules"
    ///          Text="{vr:VRBinding {Binding Path=SomeTextProperty} ValidationRule={vr:PathValidationRule}}"/>
    /// </example>
    public class VRBinding : MarkupExtension
    {
        public VRBinding() { }
        public VRBinding(Binding binding)
        {
            Binding = binding;
        }

        public Binding Binding { get; set; }
        public ValidationRule ValidationRule { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ValidationRule is not null)
                Binding.ValidationRules.Add(ValidationRule);
            return Binding.ProvideValue(serviceProvider);
        }
    }

    /// <summary>
    /// Provides a markup extension for ValidationRule types
    /// </summary>
    /// <typeparam name="T">ValidationRule</typeparam>
    public class BaseValidationRule<T> : MarkupExtension where T : ValidationRule, new()
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new T();
        }
    }
}
