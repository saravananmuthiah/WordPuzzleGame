using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System.Diagnostics.CodeAnalysis;

namespace WordPuzzleGame.ViewModels
{
    public class ThemeViewModel : INotifyPropertyChanged
    {
        public static IList<AppTheme> Themes { get; } = new List<AppTheme> { AppTheme.Unspecified, AppTheme.Light, AppTheme.Dark };

        private AppTheme _currentTheme;
        public AppTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged();
                    if (Application.Current != null)
                        Application.Current.UserAppTheme = value;
                }
            }
        }

        public ThemeViewModel()
        {
            _currentTheme = Application.Current?.UserAppTheme ?? AppTheme.Unspecified;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
