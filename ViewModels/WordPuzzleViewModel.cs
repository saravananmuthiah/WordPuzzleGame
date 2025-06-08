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
                    // Reset topic stats if topic changes
                    if (_currentTopic != value)
                    {
                        _currentTopic = value;
                        SuccessCount = 0;
                        _wordsSeenInTopic = 0;
                        _seenWords.Clear();
                    }
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

        private bool _isHintAvailable = true;
        public bool IsHintAvailable
        {
            get => _isHintAvailable;
            set { _isHintAvailable = value; OnPropertyChanged(nameof(IsHintAvailable)); }
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

        private int _successCount = 0;
        public int SuccessCount
        {
            get => _successCount;
            set { _successCount = value; OnPropertyChanged(nameof(SuccessCount)); }
        }
        private string? _currentTopic;
        private int _wordsSeenInTopic = 0;
        private const int MaxWordsPerTopic = 20;
        private HashSet<string> _seenWords = new();

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
                SuccessCount++;
                await TextToSpeech.Default.SpeakAsync("Well done!");
                await Task.Delay(1500);
                NextWord();
            }
            else
            {
                IsError = true;
                IsSuccess = false;
                Message = $"Try again! The answer was: {_currentWord}";
                await TextToSpeech.Default.SpeakAsync($"Try again! The answer was: {_currentWord}");
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
            if (!string.IsNullOrEmpty(_currentWord) && _currentWordDefs != null && _currentWordDefs.Length > 1)
            {
                _currentHintIndex = (_currentHintIndex + 1) % _currentWordDefs.Length;
                Hint = $"Hint: {_currentWordDefs[_currentHintIndex].Split('\t').LastOrDefault()}";
                // If we've cycled through all hints, disable the button
                IsHintAvailable = _currentHintIndex < _currentWordDefs.Length - 1;
            }
            else
            {
                IsHintAvailable = false;
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
                if (_wordsSeenInTopic >= MaxWordsPerTopic)
                {
                    ShuffledWord = "Done!";
                    Hint = $"You completed {MaxWordsPerTopic} words in this topic!";
                    IsInputEnabled = false;
                    return;
                }
                var httpClient = new HttpClient();
                string pattern = string.Empty;
                List<DatamuseWord>? valid = null;
                int attempts = 0;
                do
                {
                    var length = _random.Next(3, 6); // 3, 4, or 5
                    var pos = _random.Next(length);
                    char randomChar = (char)('a' + _random.Next(26));
                    char[] patternArr = Enumerable.Repeat('?', length).ToArray();
                    patternArr[pos] = randomChar;
                    pattern = new string(patternArr);
                    var url = $"https://api.datamuse.com/words?sp={pattern}&md=d&max=20";
                    if (!string.IsNullOrEmpty(SelectedTopic))
                    {
                        url += $"&ml={SelectedTopic}&topics={SelectedTopic}";
                    }
                    var response = await httpClient.GetStringAsync(url);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var words = JsonSerializer.Deserialize<List<DatamuseWord>>(response, options);
                    valid = words?.Where(w => !string.IsNullOrEmpty(w.Word) && w.Defs != null && w.Defs.Length > 0 && !_seenWords.Contains(w.Word.ToLower())).ToList();
                    attempts++;
                } while ((valid == null || valid.Count == 0) && attempts < 5);
                if (valid == null || valid.Count == 0)
                {
                    ShuffledWord = "No valid words";
                    Hint = "Hint: No hint available.";
                    _currentWordDefs = null;
                    _currentHintIndex = 0;
                    IsHintAvailable = false;
                    return;
                }
                var chosen = valid[_random.Next(valid.Count)];
                _currentWord = chosen.Word.ToLower();
                _seenWords.Add(_currentWord);
                _wordsSeenInTopic++;
                _currentWordDefs = chosen.Defs;
                _currentHintIndex = 0;
                Hint = $"Hint: {_currentWordDefs[0].Split('\t').LastOrDefault()}";
                IsHintAvailable = _currentWordDefs.Length > 1;
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
                IsHintAvailable = false;
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
