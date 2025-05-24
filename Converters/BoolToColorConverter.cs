using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace WordPuzzleGame.Converters
{
    // Converts a boolean to a color (e.g., green for success, red for error)
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is string paramStr && paramStr.Contains(","))
            {
                var colors = paramStr.Split(',');
                if (value is bool b)
                    return b ? colors[0] : colors[1];
            }
            return "Black";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
