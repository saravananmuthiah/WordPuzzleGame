using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Text.Json;

namespace WordPuzzleGame.ViewModels
{
    public partial class WordPuzzleViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private readonly Random _random = new();
        private string? _currentWord;
        private string? _shuffledWord;
        private string? _userGuess;
        private string? _message;
        private bool _isSuccess;
        private bool _isError;
        private bool _isInputEnabled = true;
        private string? _hint;
        public string? Hint
        {
            get => _hint;
            set { _hint = value; OnPropertyChanged(nameof(Hint)); }
        }

        private string? _selectedTopic;
        public string? SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                if (_selectedTopic != value)
                {
                    _selectedTopic = value;
                    OnPropertyChanged(nameof(SelectedTopic));
                    // Load a new word when the topic changes
                    NextWord();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public WordPuzzleViewModel()
        {
            // Removed _remainingWords and _words usage. Only use Datamuse API.
            ShuffledWordTiles = new ObservableCollection<string>();
            UserTiles = new ObservableCollection<string>();
            NextWord();
            SubmitCommand = new Command(async () => await SubmitAsync(), () => IsInputEnabled);
            ShuffleCommand = new Command(ShuffleWord, () => IsInputEnabled);
            TileTapCommand = new Command<string>(OnTileTapped, CanTapTile);
            RefreshHintCommand = new Command(RefreshHint, () => IsInputEnabled);
            GoToTopicSelectionCommand = new Command(async () => await GoToTopicSelectionAsync());
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
        public ICommand RefreshHintCommand { get; }
        public ICommand GoToTopicSelectionCommand { get; }

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
            _ = LoadRandomWordAsync();
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

        private void RefreshHint()
        {
            if (!string.IsNullOrEmpty(_currentWord))
            {
                // Provide a new hint if available (e.g., next definition if multiple)
                if (_currentWordDefs != null && _currentWordDefs.Length > 1)
                {
                    _currentHintIndex = (_currentHintIndex + 1) % _currentWordDefs.Length;
                    Hint = $"Hint: {_currentWordDefs[_currentHintIndex].Split('\t').LastOrDefault()}";
                }
            }
        }

        private string[]? _currentWordDefs;
        private int _currentHintIndex = 0;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("topic", out var topicObj) && topicObj is string topic)
            {
                SelectedTopic = topic;
            }
        }

        private async Task LoadRandomWordAsync()
        {
            try
            {
                var httpClient = new HttpClient();
                var length = _random.Next(2) == 0 ? 3 : 4;
                var pos = _random.Next(length);
                char randomChar = (char)('a' + _random.Next(26));
                char[] patternArr = Enumerable.Repeat('?', length).ToArray();
                patternArr[pos] = randomChar;
                var pattern = new string(patternArr);
                var url = $"https://api.datamuse.com/words?sp={pattern}&md=d&max=20";
                if (!string.IsNullOrEmpty(SelectedTopic))
                {
                    url += $"&topics={SelectedTopic}";
                }
                var response = await httpClient.GetStringAsync(url);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var words = JsonSerializer.Deserialize<List<DatamuseWord>>(response, options);
                var valid = words?.Where(w => !string.IsNullOrEmpty(w.Word) && w.Defs != null && w.Defs.Length > 0).ToList();
                if (valid == null || valid.Count == 0)
                {
                    ShuffledWord = "No valid words";
                    Hint = "Hint: No hint available.";
                    _currentWordDefs = null;
                    _currentHintIndex = 0;
                    return;
                }
                var chosen = valid[_random.Next(valid.Count)];
                _currentWord = chosen.Word.ToLower();
                _currentWordDefs = chosen.Defs;
                _currentHintIndex = 0;
                Hint = $"Hint: {_currentWordDefs[0].Split('\t').LastOrDefault()}";
                ShuffledWord = Shuffle(_currentWord!);
                ShuffledWordTiles = new ObservableCollection<string>(ShuffledWord.ToCharArray().Select(c => c.ToString()));
                UserTiles = new ObservableCollection<string>(Enumerable.Repeat(string.Empty, _currentWord.Length));
                UserGuess = string.Empty;
                Message = string.Empty;
                IsSuccess = false;
                IsError = false;
                IsInputEnabled = true;
            }
            catch
            {
                ShuffledWord = "Error";
                Hint = "Hint: Could not load word.";
                _currentWordDefs = null;
                _currentHintIndex = 0;
            }
        }

        private async Task GoToTopicSelectionAsync()
        {
            await Shell.Current.GoToAsync("//TopicSelectionPage");
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
