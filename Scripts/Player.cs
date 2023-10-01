using System;
using Godot;

public partial class Player : Sprite2D
{
	[Signal]
	public delegate void PlayerAttemptMoveEventHandler(Direction direction);

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionReleased("ui_left"))
		{
			EmitSignal(SignalName.PlayerAttemptMove, (int)Direction.Left);
		}
		else if (@event.IsActionReleased("ui_right"))
		{
			EmitSignal(SignalName.PlayerAttemptMove, (int)Direction.Right);
		}
		else if (@event.IsActionReleased("ui_up"))
		{
			EmitSignal(SignalName.PlayerAttemptMove, (int)Direction.Up);
		}
		else if (@event.IsActionReleased("ui_down"))
		{
			EmitSignal(SignalName.PlayerAttemptMove, (int)Direction.Down);
		}

	}

}
public enum Direction
{
	Left,
	Right,
	Up,
	Down
}