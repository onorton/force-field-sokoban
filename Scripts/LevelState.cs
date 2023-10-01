


using System.Collections.Generic;
using Godot;

public record LevelState(Vector2I PlayerPosition, List<(Vector2 Position, Vector2 Size)> ForceFieldPositionsAndSizes, List<Vector2I> CargoPositions, List<Vector2I> GoalPositions, List<Vector2I> CompletedGoalPositions);