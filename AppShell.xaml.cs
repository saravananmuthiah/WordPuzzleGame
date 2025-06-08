using WordPuzzleGame.ViewModels;

namespace WordPuzzleGame;

public partial class AppShell : Shell
{
	public ThemeViewModel ThemeVM { get; } = new ThemeViewModel();
	public AppShell()
	{
		InitializeComponent();
		this.BindingContext = this;
		Routing.RegisterRoute("MainPage", typeof(MainPage));
		Routing.RegisterRoute("TopicSelectionPage", typeof(TopicSelectionPage));
		// Navigate to TopicSelectionPage on startup
		GoToAsync("//TopicSelectionPage");
	}
}
