using Microsoft.Maui.ApplicationModel.DataTransfer;
using WordPuzzleGame.ViewModels;
using WordPuzzleGame.Services;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WordPuzzleGame;

public partial class MainPage : ContentPage
{
    private const string BackgroundAudio = "background.wav";
    private const string SuccessAudio = "success.mp3";
    private const string LetterInsertedAudio = "letter_inserted.wav";
    private bool _isLoaded = false;
    private int _lastUserTilesFilled = 0;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = new WordPuzzleViewModel();
        this.Loaded += MainPage_Loaded;
        this.Unloaded += MainPage_Unloaded;
    }

    private WordPuzzleViewModel? ViewModel => BindingContext as WordPuzzleViewModel;

    private async void MainPage_Loaded(object? sender, EventArgs e)
    {
        if (_isLoaded) return;
        _isLoaded = true;
        await AudioService.PlaySoundAsync(BackgroundAudio);
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            if (ViewModel.UserTiles is INotifyCollectionChanged coll)
                coll.CollectionChanged += UserTiles_CollectionChanged;
            _lastUserTilesFilled = ViewModel.UserTiles.Count(x => !string.IsNullOrEmpty(x));
        }
    }

    private void MainPage_Unloaded(object? sender, EventArgs e)
    {
        _isLoaded = false;
        AudioService.StopSound();
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            if (ViewModel.UserTiles is INotifyCollectionChanged coll)
                coll.CollectionChanged -= UserTiles_CollectionChanged;
        }
    }

    private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WordPuzzleViewModel.IsSuccess) && ViewModel != null && ViewModel.IsSuccess)
        {
            await AudioService.PlaySoundAsync(SuccessAudio);
        }
        if (e.PropertyName == nameof(WordPuzzleViewModel.UserTiles) && ViewModel != null)
        {
            if (ViewModel.UserTiles is INotifyCollectionChanged coll)
            {
                coll.CollectionChanged -= UserTiles_CollectionChanged;
                coll.CollectionChanged += UserTiles_CollectionChanged;
            }
            _lastUserTilesFilled = ViewModel.UserTiles.Count(x => !string.IsNullOrEmpty(x));
        }
    }

    private async void UserTiles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (ViewModel == null) return;
        int filled = ViewModel.UserTiles.Count(x => !string.IsNullOrEmpty(x));
        if (filled > _lastUserTilesFilled)
        {
            await AudioService.PlaySoundAsync(LetterInsertedAudio);
        }
        _lastUserTilesFilled = filled;
    }

    // Handle drag start for a tile
    private void OnTileDragStarting(object sender, DragStartingEventArgs e)
    {

        // In MAUI, sender is the DropGestureRecognizer, use (sender as DropGestureRecognizer)?.Parent for the Border
        var dropGesture = sender as DragGestureRecognizer;
        var border = dropGesture?.Parent as Border;
        if (border != null && border.BindingContext is string letter)
        {
            e.Data.Properties["Letter"] = letter;
            e.Data.Text = letter;
        }
    }

    // Handle drop on a user tile slot
    private async void OnTileDrop(object sender, DropEventArgs e)
    {
        // Defensive: sender is DropGestureRecognizer, but Parent may not be Border at launch or if not attached
        var border = (sender as DropGestureRecognizer)?.Parent as Border;
        if (border == null)
            return; // Prevents null reference exception on launch or miswiring

        // Try to get the letter from drag data robustly
        string? letter = null;
        try
        {
            var text = await e.Data.GetTextAsync();
            if (!string.IsNullOrEmpty(text) && text.Length == 1)
                letter = text;
        }
        catch
        {
            // Ignore if GetTextAsync fails
        }
        if (string.IsNullOrEmpty(letter) && e.Data.Properties.TryGetValue("Letter", out var letterObj))
        {
            letter = letterObj as string;
        }

        var userTiles = ViewModel?.UserTiles;
        int index = -1;
        if (border.BindingContext is string slot && userTiles != null)
        {
            // Redesign: Only allow drop on empty slots, do not replace filled slots
            if (string.IsNullOrEmpty(slot))
            {
                index = userTiles.IndexOf(slot);
            }
        }
        if (index >= 0 && !string.IsNullOrEmpty(letter) && userTiles != null && ViewModel != null)
        {
            // Only allow drop if slot is empty
            userTiles[index] = letter;
            ViewModel.ShuffledWordTiles.Remove(letter);
            // If all slots filled, check answer
            if (userTiles.All(x => !string.IsNullOrEmpty(x)))
            {
                ViewModel.UserGuess = string.Join("", userTiles).ToLower();
                ViewModel.SubmitCommand.Execute(null);
            }
        }
    }

    // Allow drop for all tiles in the answer area
    private void OnTileDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }
}
