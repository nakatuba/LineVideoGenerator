using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LineVideoGenerator
{
    class ThumbConverter : IValueConverter
    {
        public static int per = 16;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value * per;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / per;
        }
    }
}
