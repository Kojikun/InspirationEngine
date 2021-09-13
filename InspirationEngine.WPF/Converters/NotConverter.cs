using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InspirationEngine.WPF.Converters
{
    public class NotConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool val)
                return !val;
            throw new ArgumentException($"{nameof(value)} is not of type bool", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool val)
                return !val;
            throw new ArgumentException($"{nameof(value)} is not of type bool", nameof(value));
        }
    }
}
