namespace WordPuzzleGame;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("MainPage", typeof(MainPage));
		Routing.RegisterRoute("TopicSelectionPage", typeof(TopicSelectionPage));
		// Navigate to TopicSelectionPage on startup
		GoToAsync("//TopicSelectionPage");
	}
}
