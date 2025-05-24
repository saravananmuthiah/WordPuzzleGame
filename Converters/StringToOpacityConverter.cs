using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace WordPuzzleGame.Converters
{
    // Converts a string to opacity: 1 if not empty, 0 if empty
    public class StringToOpacityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string) ? 0.0 : 1.0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
