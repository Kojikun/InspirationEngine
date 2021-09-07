using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace InspirationEngine.WPF.Converters
{
    /// <summary>
    /// Convenience class that provides a class that implements IValueConverter to be used as a Markup Extension
    /// </summary>
    /// <example>
    /// <TextBox xmlns:cv="clr-namespace:InspirationEngine.WPF.Converters" Text="{Binding SomeProperty, Converter={cv:MyConverter}}"/>
    /// </example>
    public abstract class BaseConverter : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
