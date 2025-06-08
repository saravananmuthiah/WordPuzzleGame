using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace WordPuzzleGame.Converters
{
    public class UriDecodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
                return Uri.UnescapeDataString(s.Replace("+", " "));
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
