using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace InspirationEngine.WPF.Converters
{
    public class BoolToVisibilityConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool val)
            {
                if (!val && parameter is string param && param.ToLowerInvariant() == "hidden")
                    return Visibility.Hidden;
                else if (val)
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                switch (visibility)
                {
                    case Visibility.Collapsed:
                    case Visibility.Hidden:
                        return false;
                    case Visibility.Visible:
                        return true;
                }
            }

            return false;
        }
    }
}
