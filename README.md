# Word Puzzle Game (.NET MAUI)

A simple and visually appealing word puzzle game built with .NET MAUI and MVVM architecture.

## Features
- Shuffled 3- and 4-letter English words
- User guesses the word by typing in a text box
- Success and error feedback with messages and basic animation
- No word repeats in a session
- ObservableCollection and INotifyPropertyChanged for data binding
- Organized, modern vertical UI

## How to Play
1. Look at the large, shuffled letters.
2. Type your guess in the input box.
3. If correct, you’ll see a success message and move to the next word.
4. If incorrect, you’ll see an error message and can try again.

## How to Run
1. Open this folder in Visual Studio 2022+ or VS Code with .NET MAUI support.
2. Build and run the project for your target platform (Android, iOS, Windows, MacCatalyst).

## Project Structure
- `ViewModels/WordPuzzleViewModel.cs`: Game logic and state
- `MainPage.xaml`: UI layout and bindings
- `Converters/`: Value converters for UI feedback

## Requirements
- .NET 9.0 SDK
- .NET MAUI workload installed

---
Enjoy solving word puzzles!
