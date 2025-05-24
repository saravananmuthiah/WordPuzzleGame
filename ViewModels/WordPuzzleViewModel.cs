using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace WordPuzzleGame.ViewModels
{
    // ViewModel for the word puzzle game, implements INotifyPropertyChanged for data binding
    public class WordPuzzleViewModel : INotifyPropertyChanged
    {
        private readonly string[] _words = new[]
        {
            "cat", "dog", "sun", "car", "hat", "book", "star", "frog", "milk", "tree", "fish", "cake", "bird", "lamp", "ring",
            // Added more 3- and 4-letter words
            "moon", "wolf", "bear", "lion", "goat", "duck", "goat", "pear", "rose", "leaf", "snow", "wind", "rain", "fire", "rock", "sand", "ship", "door", "king", "quiz", "jump", "blue", "pink", "gold", "ruby", "opal", "mint", "corn", "rice", "bean", "cake", "desk", "fork", "hand", "jazz", "kite", "lake", "mask", "nest", "oven", "park", "quiz", "rope", "sock", "toad", "unit", "vase", "wave", "yarn", "zinc"
        };
        private readonly Random _random = new();
        private string? _currentWord;
        private string? _shuffledWord;
        private string? _userGuess;
        private string? _message;
        private bool _isSuccess;
        private bool _isError;
        private bool _isInputEnabled = true;
        private ObservableCollection<string> _remainingWords;

        public event PropertyChangedEventHandler? PropertyChanged;

        public WordPuzzleViewModel()
        {
            _remainingWords = new ObservableCollection<string>(_words.OrderBy(_ => _random.Next()));
            ShuffledWordTiles = new ObservableCollection<string>();
            UserTiles = new ObservableCollection<string>();
            NextWord();
            SubmitCommand = new Command(async () => await SubmitAsync(), () => IsInputEnabled);
            ShuffleCommand = new Command(ShuffleWord, () => IsInputEnabled);
            TileTapCommand = new Command<string>(OnTileTapped, CanTapTile);
        }

        public string? ShuffledWord
        {
            get => _shuffledWord;
            set { _shuffledWord = value; OnPropertyChanged(nameof(ShuffledWord)); }
        }

        public string? UserGuess
        {
            get => _userGuess;
            set { _userGuess = value; OnPropertyChanged(nameof(UserGuess)); }
        }

        public string? Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(nameof(Message)); }
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set { _isSuccess = value; OnPropertyChanged(nameof(IsSuccess)); }
        }

        public bool IsError
        {
            get => _isError;
            set { _isError = value; OnPropertyChanged(nameof(IsError)); }
        }

        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set { _isInputEnabled = value; OnPropertyChanged(nameof(IsInputEnabled)); }
        }

        public ICommand SubmitCommand { get; }
        public ICommand ShuffleCommand { get; }
        public ICommand TileTapCommand { get; }

        private ObservableCollection<string> _shuffledWordTiles = new();
        public ObservableCollection<string> ShuffledWordTiles
        {
            get => _shuffledWordTiles;
            set { _shuffledWordTiles = value; OnPropertyChanged(nameof(ShuffledWordTiles)); }
        }

        private ObservableCollection<string> _userTiles = new();
        public ObservableCollection<string> UserTiles
        {
            get => _userTiles;
            set { _userTiles = value; OnPropertyChanged(nameof(UserTiles)); }
        }

        // Move to the next word, shuffle it, and reset state
        private void NextWord()
        {
            if (_remainingWords.Count == 0)
            {
                ShuffledWord = "Game Over!";
                ShuffledWordTiles.Clear();
                UserTiles.Clear();
                IsInputEnabled = false;
                Message = "You solved all words!";
                return;
            }
            _currentWord = _remainingWords[0];
            _remainingWords.RemoveAt(0);
            ShuffledWord = Shuffle(_currentWord!);
            ShuffledWordTiles = new ObservableCollection<string>(ShuffledWord.ToCharArray().Select(c => c.ToString()));
            UserTiles = new ObservableCollection<string>(Enumerable.Repeat(string.Empty, _currentWord.Length));
            UserGuess = string.Empty;
            Message = string.Empty;
            IsSuccess = false;
            IsError = false;
            IsInputEnabled = true;
        }

        // Shuffle the letters of the word
        private string Shuffle(string word)
        {
            var array = word.ToCharArray();
            do
            {
                array = word.ToCharArray();
                for (int i = array.Length - 1; i > 0; i--)
                {
                    int j = _random.Next(i + 1);
                    (array[i], array[j]) = (array[j], array[i]);
                }
            } while (new string(array) == word); // Ensure it's actually shuffled
            return new string(array).ToUpper();
        }

        // Handle the submit action
        private async Task SubmitAsync()
        {
            if (!IsInputEnabled) return;
            if (string.Equals(UserGuess?.Trim(), _currentWord, StringComparison.OrdinalIgnoreCase))
            {
                IsSuccess = true;
                IsError = false;
                Message = "Well done!";
                IsInputEnabled = false;
                await TextToSpeech.Default.SpeakAsync("Well done!");
                await Task.Delay(1500);
                NextWord();
            }
            else
            {
                IsError = true;
                IsSuccess = false;
                Message = "Try again!";
                await TextToSpeech.Default.SpeakAsync("Try again!");
                await Task.Delay(1000); // Short delay for feedback
                NextWord(); // Reset the word after showing 'Try again!'
            }
        }

        private void ShuffleWord()
        {
            if (!IsInputEnabled || string.IsNullOrEmpty(_currentWord)) return;
            ShuffledWord = Shuffle(_currentWord!);
            ShuffledWordTiles = new ObservableCollection<string>(ShuffledWord.ToCharArray().Select(c => c.ToString()));
            UserTiles = new ObservableCollection<string>(Enumerable.Repeat(string.Empty, _currentWord!.Length));
            UserGuess = string.Empty;
            Message = string.Empty;
            IsSuccess = false;
            IsError = false;
        }

        private bool CanTapTile(string? letter)
        {
            return IsInputEnabled && !string.IsNullOrEmpty(letter) && ShuffledWordTiles.Contains(letter);
        }

        private void OnTileTapped(string? letter)
        {
            if (!CanTapTile(letter) || UserTiles.All(x => !string.IsNullOrEmpty(x)))
                return;
            // Find first empty slot
            int index = UserTiles.IndexOf("");
            if (index >= 0)
            {
                UserTiles[index] = letter!;
                ShuffledWordTiles.Remove(letter!);
                UserGuess = string.Join("", UserTiles).ToLower();
                if (UserTiles.All(x => !string.IsNullOrEmpty(x)))
                {
                    SubmitCommand.Execute(null);
                }
            }
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
