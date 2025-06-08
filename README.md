# Word Puzzle Game

A simple and visually appealing word puzzle game built with .NET MAUI and MVVM architecture.

## Features
- Shuffled 3-, 4-, and 5-letter English words
- Topic selection screen with a variety of interesting topics
- No word repeats in a topic session (max 20 per topic)
- Success count tracked per topic
- User guesses the word by tapping tiles (no text box)
- Success and error feedback with messages and basic animation
- ObservableCollection and INotifyPropertyChanged for data binding
- Organized, modern vertical UI
- Light and dark theme support with theme picker in navigation bar

## How to Play
1. Choose a topic from the selection screen.
2. Look at the large, shuffled letters.
3. Tap the tiles to guess the word.
4. If correct, you’ll see a success message and move to the next word.
5. If incorrect, you’ll see an error message and the correct answer, then move to the next word.
6. Complete up to 20 words per topic. Your success count is shown for each topic.

## How to Run
1. Open this folder in Visual Studio 2022+ or VS Code with .NET MAUI support.
2. Build and run the project for your target platform (Android, iOS, Windows, MacCatalyst).

## Project Structure
- `ViewModels/WordPuzzleViewModel.cs`: Game logic and state
- `ViewModels/TopicSelectionViewModel.cs`: Topic selection logic
- `ViewModels/ThemeViewModel.cs`: Theme switching logic
- `MainPage.xaml`: Game UI layout and bindings
- `TopicSelectionPage.xaml`: Topic selection UI
- `Converters/`: Value converters for UI feedback
- `Models/DatamuseWord.cs`: Word model for API

## Requirements
- .NET 9.0 SDK
- .NET MAUI workload installed

---
Enjoy solving word puzzles by topic!
