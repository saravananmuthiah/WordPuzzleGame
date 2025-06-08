using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace WordPuzzleGame.Converters
{
    public class ThemeListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new List<AppTheme> { AppTheme.Unspecified, AppTheme.Light, AppTheme.Dark };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
