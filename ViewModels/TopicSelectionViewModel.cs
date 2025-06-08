using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace WordPuzzleGame.ViewModels
{
    public class TopicSelectionViewModel
    {
        public ICommand SelectTopicCommand { get; }
        public TopicSelectionViewModel()
        {
            SelectTopicCommand = new Command<string>(OnTopicSelected);
        }

        private async void OnTopicSelected(string topic)
        {
            // Navigate to MainPage and pass the selected topic
            await Shell.Current.GoToAsync($"//MainPage?topic={topic}");
        }
    }
}
