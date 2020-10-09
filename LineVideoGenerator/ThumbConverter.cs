using System;
using System.Globalization;
using System.Windows.Data;

namespace LineVideoGenerator
{
    class ThumbConverter : IValueConverter
    {
        public static int per = 16;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * per;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) / per;
        }
    }
}
