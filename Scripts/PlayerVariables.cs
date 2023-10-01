using Godot;

public partial class PlayerVariables : Node
{
	public int Level { get; private set; } = 1;
	public bool Paused { get; set; } = false;
	public bool Muted { get; set; } = false;

	private int _numberOfLevels { get; } = 7;

	public int TotalTurns { get; private set; } = 0;


	public void UpdateTotalTurns(int turnsForLevel)
	{
		TotalTurns += turnsForLevel;
	}

	public void NextLevel()
	{
		Level++;
	}

	public bool AtLastLevel()
	{
		return Level == _numberOfLevels;
	}
}
