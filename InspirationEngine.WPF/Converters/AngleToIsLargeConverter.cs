using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InspirationEngine.WPF.Converters
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://stackoverflow.com/a/39302102"/>
    class AngleToIsLargeConverter : BaseConverter, IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double angle = (double)value;

            return angle > 180;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
