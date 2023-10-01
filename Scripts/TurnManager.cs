using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TurnManager : Node
{
	[Signal]
	public delegate void GameOverEventHandler();

	[Signal]
	public delegate void LevelCompleteEventHandler();

	[Signal]
	public delegate void ForceFieldTurnsLeftChangedEventHandler(int turnsLeft);


	private AudioStream _victorySound;
	private AudioStream _gameOverSound;
	private AudioStream _forceFieldSound;
	private AudioStream _pushedBoxSound;
	private AudioStream _pushedBoxIntoGoalSound;
	private AudioStream _undoSound;


	private Node2D _player;
	private Node2D _exit;

	private TileMap _tileMap;

	private Node _activeCargoNode;
	private List<Node2D> _activeCargo;

	private List<ForceField> _forceFields;

	[Export]
	private int _turnsUntilForcefieldExtends = 5;
	private int _currentTurn;

	private PlayerVariables _playerVariables;

	private AudioStreamPlayer2D _soundEffectPlayer;

	private const int ImpassableLayer = 1;
	private const int GoalLayer = 2;

	private bool _gameOver = false;

	private int _tileSize;
	// Called when the node enters the scene tree for the first time.


	private List<LevelState> _previousStates;

	private (int SourceId, Vector2I AtlasCoords) _completedGoalTileData = (5, new Vector2I(0, 0));
	private (int SourceId, Vector2I AtlasCoords) _goalTileData = (4, new Vector2I(0, 0));


	private PackedScene _cargoResource;

	public override void _Ready()
	{
		_player = GetNode<Node2D>("Player");
		_exit = GetNode<Node2D>("Exit");
		_tileMap = GetNode<TileMap>("TileMap");
		_tileSize = _tileMap.TileSet.TileSize.X;
		_activeCargoNode = GetNode<Node>("Active Cargo");
		_activeCargo = _activeCargoNode.GetChildren().Cast<Node2D>().ToList();

		_forceFields = GetChildren().Where(x => x is ForceField).Cast<ForceField>().ToList();

		_playerVariables = GetNode<PlayerVariables>("/root/PlayerVariables");
		_playerVariables.Paused = false;

		_soundEffectPlayer = GetNode<AudioStreamPlayer2D>("Sound Effect Player");

		_victorySound = GD.Load<AudioStream>("res://Audio/victory.wav");
		_gameOverSound = GD.Load<AudioStream>("res://Audio/game over.wav");
		_forceFieldSound = GD.Load<AudioStream>("res://Audio/forcefield.wav");
		_pushedBoxSound = GD.Load<AudioStream>("res://Audio/pushed_box.wav");
		_pushedBoxIntoGoalSound = GD.Load<AudioStream>("res://Audio/pushed_box_to_goal.wav");
		_undoSound = GD.Load<AudioStream>("res://Audio/undo.wav");

		_currentTurn = 0;


		ForceFieldTurnsLeftUpdate();
		_previousStates = new List<LevelState>();

		_cargoResource = GD.Load<PackedScene>("res://Scenes/cargo.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_player.Position == _exit.Position && !_activeCargo.Any() && !_gameOver)
		{
			EmitSignal(SignalName.LevelComplete);
			_gameOver = true;
			PlaySound(_victorySound);

		}
	}

	public void OnPlayerAttemptMove(Direction direction)
	{

		if (_gameOver || _playerVariables.Paused)
		{
			return;
		}

		_currentTurn += 1;

		ForceFieldTurnsLeftUpdate();


		SaveCurrentState();

		var playerPosition = _tileMap.LocalToMap(_player.Position);

		var directionVector = direction switch
		{
			Direction.Left => new Vector2I(-1, 0),
			Direction.Right => new Vector2I(1, 0),
			Direction.Up => new Vector2I(0, -1),
			Direction.Down => new Vector2I(0, 1),
			_ => throw new NotImplementedException(),
		};

		var newPlayerPosition = playerPosition + directionVector;


		if (_tileMap.GetCellTileData(ImpassableLayer, newPlayerPosition) != null || _forceFields.Any(f => f.HasPoint(_tileMap.MapToLocal(newPlayerPosition))))
		{
			newPlayerPosition = playerPosition;
		}

		// Might want to have a separate cargo manager instead of doing it all in this class
		var cargoAtNewPosition = _activeCargo.FirstOrDefault(c => _tileMap.LocalToMap(c.Position) == newPlayerPosition);
		if (cargoAtNewPosition != null)
		{
			var newCargoPosition = newPlayerPosition + directionVector;

			if (_tileMap.GetCellTileData(ImpassableLayer, newCargoPosition) != null || _activeCargo.Any(c => _tileMap.LocalToMap(c.Position) == newCargoPosition) || _forceFields.Any(f => f.HasPoint(_tileMap.MapToLocal(newCargoPosition))))
			{
				newCargoPosition = newPlayerPosition;
				newPlayerPosition = playerPosition;
			}
			else
			{
				PlaySound(_pushedBoxSound);
			}
			cargoAtNewPosition.Position = _tileMap.MapToLocal(newCargoPosition);

			if (_tileMap.GetCellTileData(GoalLayer, newCargoPosition) != null)
			{
				// At goal
				_tileMap.SetCell(GoalLayer, newCargoPosition, sourceId: -1);
				_tileMap.SetCell(0, newCargoPosition, _completedGoalTileData.SourceId, _completedGoalTileData.AtlasCoords);

				_activeCargo.Remove(cargoAtNewPosition);
				cargoAtNewPosition.QueueFree();
				PlaySound(_pushedBoxIntoGoalSound);
			}
		}


		if (_currentTurn % _turnsUntilForcefieldExtends == 0)
		{
			PlaySound(_forceFieldSound);
			// force fields now extend and potentially push player
			foreach (var forceField in _forceFields)
			{
				forceField.Extend(_tileSize);
				var forceFieldDirection = Vector2I.Zero;
				if (forceField.ExtendsUpwards)
				{
					forceFieldDirection = Vector2I.Up;
				}
				else
				{
					forceFieldDirection = Vector2I.Down;
				}


				while (forceField.HasPoint(_tileMap.MapToLocal(newPlayerPosition)))
				{

					var playerForcedNewPosition = newPlayerPosition + forceFieldDirection;

					if (_tileMap.GetCellTileData(ImpassableLayer, playerForcedNewPosition) != null)
					{
						break;
					}

					if (_forceFields.Any(f => f.HasPoint(playerForcedNewPosition)))
					{
						break;
					}

					var cargoAtForcedNewPosition = _activeCargo.FirstOrDefault(c => _tileMap.LocalToMap(c.Position) == playerForcedNewPosition);

					if (cargoAtForcedNewPosition != null)
					{
						var newCargoPosition = playerForcedNewPosition + forceFieldDirection;

						if (_tileMap.GetCellTileData(ImpassableLayer, newCargoPosition) != null || _activeCargo.Any(c => _tileMap.LocalToMap(c.Position) == newCargoPosition) || _forceFields.Any(f => f.HasPoint(_tileMap.MapToLocal(newCargoPosition))))
						{
							break;
						}
						else
						{
							if (_tileMap.GetCellTileData(GoalLayer, newCargoPosition) != null)
							{
								// At goal
								_tileMap.SetCell(GoalLayer, newCargoPosition, sourceId: -1);
								_tileMap.SetCell(0, newCargoPosition, sourceId: 5, atlasCoords: new Vector2I(0, 0));

								_activeCargo.Remove(cargoAtForcedNewPosition);
								cargoAtForcedNewPosition.QueueFree();
							}
							else
							{
								cargoAtForcedNewPosition.Position = _tileMap.MapToLocal(newCargoPosition);
							}
						}
					}

					newPlayerPosition = playerForcedNewPosition;
				}
			}
		}
		_player.Position = _tileMap.MapToLocal(newPlayerPosition);


		if (_forceFields.Any(f => f.HasPoint(_player.Position)) || _activeCargo.Any(c => _forceFields.Any(f => f.HasPoint(c.Position))) || _forceFields.Any(f => f.HasPoint(_exit.Position)))
		{
			_gameOver = true;
			PlaySound(_gameOverSound);
			EmitSignal(SignalName.GameOver);
		}

	}

	private void SaveCurrentState()
	{
		var forceFieldStates = _forceFields.Select(f => (f.Position, f.Size)).ToList();
		var cargoPositions = _activeCargo.Select(c => _tileMap.LocalToMap(c.Position)).ToList();
		var goalPositions = _tileMap.GetUsedCells(GoalLayer).ToList();
		var currentState = new LevelState(_tileMap.LocalToMap(_player.Position), forceFieldStates, cargoPositions, goalPositions, new List<Vector2I>());
		_previousStates.Add(currentState);
	}

	private void PlaySound(AudioStream sound)
	{
		_soundEffectPlayer.Stream = sound;
		_soundEffectPlayer.Play();
	}


	public void OnUndo()
	{
		if (_gameOver)
		{
			return;
		}

		if (!_previousStates.Any())
		{
			PlaySound(_gameOverSound);
			return;
		}

		PlaySound(_undoSound);

		var previousState = _previousStates.Last();
		// This works as it's just a stack
		_currentTurn = _previousStates.Count - 1;
		_previousStates.RemoveAt(_previousStates.Count - 1);
		ForceFieldTurnsLeftUpdate();

		_player.Position = _tileMap.MapToLocal(previousState.PlayerPosition);
		for (var i = 0; i < _forceFields.Count; i++)
		{
			_forceFields[i].Position = previousState.ForceFieldPositionsAndSizes[i].Position;
			_forceFields[i].Size = previousState.ForceFieldPositionsAndSizes[i].Size;
		}

		foreach (var activeCargo in _activeCargo)
		{
			_activeCargoNode.RemoveChild(activeCargo);
		}
		_activeCargo.Clear();


		foreach (var cargoPosition in previousState.CargoPositions)
		{
			var cargo = _cargoResource.Instantiate<Node2D>();
			cargo.Position = _tileMap.MapToLocal(cargoPosition);
			_activeCargoNode.AddChild(cargo);
			_activeCargo.Add(cargo);
		}

		foreach (var goalPosition in previousState.GoalPositions)
		{
			_tileMap.SetCell(GoalLayer, goalPosition, _goalTileData.SourceId, _goalTileData.AtlasCoords);
		}

		foreach (var completedGoalPosition in previousState.CompletedGoalPositions)
		{
			_tileMap.SetCell(0, completedGoalPosition, _completedGoalTileData.SourceId, _completedGoalTileData.AtlasCoords);
		}
	}

	private void ForceFieldTurnsLeftUpdate()
	{
		EmitSignal(SignalName.ForceFieldTurnsLeftChanged, _turnsUntilForcefieldExtends - (_currentTurn % _turnsUntilForcefieldExtends));
	}

	// TODO, Polish: Priority order of sounds if multiple played per turn
}
