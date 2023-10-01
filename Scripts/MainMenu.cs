using Godot;

public partial class MainMenu : Control
{
	public void OnStart()
	{
		GetTree().ChangeSceneToFile($"res://Scenes/level-1.tscn");
	}

	public void OnQuit()
	{
		GetTree().Quit();
	}
}
