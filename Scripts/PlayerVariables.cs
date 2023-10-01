using Godot;

public partial class PlayerVariables : Node
{
	public int Level { get; private set; } = 1;
	public bool Paused { get; set; } = false;
	public bool Muted { get; set; } = false;

	private int _numberOfLevels { get; } = 7;


	public void NextLevel()
	{
		Level++;
	}

	public bool AtLastLevel()
	{
		return Level == _numberOfLevels;
	}
}
