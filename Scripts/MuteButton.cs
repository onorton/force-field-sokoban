using Godot;
using System;

public partial class MuteButton : Control
{

	private TextureButton _muted;
	private TextureButton _unmuted;
	private PlayerVariables _playerVariables;

	[Signal]
	public delegate void MuteToggleEventHandler();


	public override void _Ready()
	{
		_muted = GetNode<TextureButton>("Muted");
		_unmuted = GetNode<TextureButton>("Unmuted");
		_playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

	}

	public void OnStateChanged(bool muted)
	{
		_playerVariables.Muted = muted;
		_muted.Visible = muted;
		_unmuted.Visible = !muted;
		EmitSignal(SignalName.MuteToggle);
	}
}
