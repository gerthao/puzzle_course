using System.Collections.Generic;
using System.Linq;
using Godot;
using PuzzleCourse.Game.Autoload;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.Component;

public partial class BuildingComponent : Node2D
{
    private readonly HashSet<Vector2I> _occupiedTiles = new();

    [Export(PropertyHint.File, "*.tres")]
    private string _buildingResourcePath;

    public BuildingResource BuildingResource { get; private set; }

    public override void _Ready()
    {
        if (_buildingResourcePath != null)
            BuildingResource = GD.Load<BuildingResource>(_buildingResourcePath);

        AddToGroup(nameof(BuildingComponent));

        Callable
            .From(Initialize)
            .CallDeferred();
    }

    public void Destroy()
    {
        GameEvents.EmitBuildingDestroyed(this);
        Owner.QueueFree();
    }

    public Vector2I GetGridCellPosition()
    {
        var (x, y) = (GlobalPosition / Grid.CellPixelSize).Floor();

        return new Vector2I((int)x, (int)y);
    }

    public HashSet<Vector2I> GetOccupiedCellPositions() => _occupiedTiles.ToHashSet();

    public bool IsTileInBuildingArea(Vector2I tilePosition) => _occupiedTiles.Contains(tilePosition);

    private void CalculateOccupiedCellPositions()
    {
        var gridCellPosition = GetGridCellPosition();

        for (var x = gridCellPosition.X; x < gridCellPosition.X + BuildingResource.Dimensions.X; x++)
        for (var y = gridCellPosition.Y; y < gridCellPosition.Y + BuildingResource.Dimensions.Y; y++)
            _occupiedTiles.Add(new Vector2I(x, y));
    }

    private void Initialize()
    {
        CalculateOccupiedCellPositions();
        GameEvents.EmitBuildingPlaced(this);
    }
}
