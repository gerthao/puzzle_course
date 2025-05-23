using Godot;
using PuzzleCourse.Game.Autoload;

namespace PuzzleCourse.Game.Component;

public partial class BuildingComponent : Node2D
{
	[Export]
	public int BuildableRadius { get; private set; }
	
	public override void _Ready()
	{
		AddToGroup(nameof(BuildingComponent));
		
		Callable
			.From(() => GameEvents.EmitBuildingPlaced(this))
			.CallDeferred();
	}

	public Vector2I GetGridCellPosition()
	{
		var (x, y) = (GlobalPosition / Grid.CellPixelSize).Floor();

		return new Vector2I((int)x, (int)y);
	}
}