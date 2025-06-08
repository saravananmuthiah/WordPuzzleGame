using Microsoft.Maui.Controls;
using WordPuzzleGame;
using WordPuzzleGame.ViewModels;

namespace WordPuzzleGame
{
    public partial class TopicSelectionPage : ContentPage
    {
        public TopicSelectionPage()
        {
            InitializeComponent();
            BindingContext = new TopicSelectionViewModel();
        }
    }
}