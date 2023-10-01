using Godot;

public partial class PlayerVariables : Node
{
	public int Level { get; private set; } = 1;
	public bool Paused { get; set; } = false;
	public bool Muted { get; set; } = false;


	public void NextLevel()
	{
		Level++;
	}
}
