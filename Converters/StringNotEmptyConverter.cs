using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicPlayer_by_d3solat1on.Converters
{
    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            if (value is string str) return !string.IsNullOrWhiteSpace(str);
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}