using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InspirationEngine.WPF.Converters
{
    [ValueConversion(typeof(TimeSpan), typeof(double))]
    public class TimeSpanToDoubleConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan time)
            {
                return time.Ticks / 10000000d;
            }
            else if (value is null)
            {
                return 0d;
            }

            throw new ArithmeticException("Unable to cast value to TimeSpan.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double seconds)
            {
                return new TimeSpan(System.Convert.ToInt64(seconds * 10000000d));
            }
            else if (value is null)
            {
                return new TimeSpan();
            }

            throw new ArithmeticException("Unable to cast value to double.");
        }
    }
}
