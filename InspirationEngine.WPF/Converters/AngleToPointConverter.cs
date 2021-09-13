using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace InspirationEngine.WPF.Converters
{
    /// <summary>
    /// Converts an angle (in degree) to a point used in <see cref="System.Windows.Media.ArcSegment.Point"/>
    /// </summary>
    /// <remarks>
    /// Used in a style that defines the ControlTemplate for <see cref="UserControls.CircularProgressBar"/>
    /// </remarks>
    /// <see href="https://stackoverflow.com/a/39302102"/>
    class AngleToPointConverter : BaseConverter, IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double angle = (double)value;
            double radius = 50;
            double piang = angle * Math.PI / 180;

            double px = Math.Sin(piang) * radius + radius;
            double py = -Math.Cos(piang) * radius + radius;

            return new Point(px, py);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
