using Microsoft.Maui.ApplicationModel.DataTransfer;
using WordPuzzleGame.ViewModels;

namespace WordPuzzleGame;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        // Set the BindingContext to the ViewModel (also set in XAML for design-time support)
        BindingContext = new WordPuzzleViewModel();
    }

    private WordPuzzleViewModel? ViewModel => BindingContext as WordPuzzleViewModel;

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
