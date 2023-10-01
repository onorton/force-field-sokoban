using Godot;
using System;

public partial class ForceField : NinePatchRect
{
	[Export]
	public bool ExtendsUpwards;

	public void Extend(int pixels)
	{
		Size = new Vector2(Size.X, Size.Y + pixels);
		if (ExtendsUpwards)
		{
			Position = new Vector2(Position.X, Position.Y - pixels);
		}
	}

	public bool HasPoint(Vector2 point)
	{
		var bounding_box = new Rect2(Position, Size);
		return bounding_box.HasPoint(point);
	}
}
