using System;
using System.Globalization;
using System.Windows.Data;

namespace LineVideoGenerator
{
    class VideoTotalTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DateTime.Today.Add(TimeSpan.FromSeconds((int)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).TimeOfDay.TotalSeconds;
        }
    }
}
