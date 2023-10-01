using Godot;
using System;

public partial class UI : CanvasLayer
{
	private Control _gameOver;
	private Control _levelComplete;
	private Control _paused;
	private Control _victory;

	[Signal]
	public delegate void UndoEventHandler();

	private PlayerVariables _playerVariables;

	private RichTextLabel _forceFieldTurnsText;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_gameOver = GetNode<Control>("Menus/Game Over");
		_levelComplete = GetNode<Control>("Menus/Level Complete");
		_paused = GetNode<Control>("Menus/Paused");
		_victory = GetNode<Control>("Menus/Victory");

		_gameOver.Visible = false;
		_levelComplete.Visible = false;
		_paused.Visible = false;
		_victory.Visible = false;

		_playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");

		_forceFieldTurnsText = GetNode<RichTextLabel>("Force Field Status/VBoxContainer/HBoxContainer/Turns");

		GetNode<MuteButton>("Buttons/HBoxContainer/Mute").OnStateChanged(_playerVariables.Muted);
		UpdateMute();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionReleased("ui_cancel"))
		{
			_playerVariables.Paused = !_playerVariables.Paused;
			_paused.Visible = _playerVariables.Paused;
		}
	}

	public void OnTryAgain()
	{
		GetTree().ReloadCurrentScene();
	}

	public void OnContinue()
	{
		_playerVariables.NextLevel();
		GetTree().ChangeSceneToFile($"res://Scenes/level-{_playerVariables.Level}.tscn");
	}

	public void OnQuit()
	{
		GetTree().Quit();
	}

	public void OnGameOver()
	{
		_gameOver.Visible = true;
	}

	public void OnLevelComplete(int turnsToComplete)
	{
		if (_playerVariables.AtLastLevel())
		{
			_victory.Visible = true;
			_victory.GetNode<RichTextLabel>("MarginContainer/VBoxContainer/CenterContainer4/Turns").Text = $"Turns: {turnsToComplete}";
			_victory.GetNode<RichTextLabel>("MarginContainer/VBoxContainer/CenterContainer3/Total Turns").Text = $"Total Turns: {_playerVariables.TotalTurns}";
		}
		else
		{
			_levelComplete.GetNode<RichTextLabel>("MarginContainer/VBoxContainer/CenterContainer/Turns").Text = $"Turns: {turnsToComplete}";
			_levelComplete.Visible = true;
		}
	}

	public void OnUndo()
	{
		EmitSignal(SignalName.Undo);
	}

	public void UpdateMute()
	{
		AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), _playerVariables.Muted);
	}

	public void OnTurnsUntilForceFieldUpdate(int turnsLeft)
	{
		if (_forceFieldTurnsText == null)
		{
			_forceFieldTurnsText = GetNode<RichTextLabel>("Force Field Status/VBoxContainer/HBoxContainer/Turns");
		}
		_forceFieldTurnsText.Text = $"{turnsLeft}";
	}

}
