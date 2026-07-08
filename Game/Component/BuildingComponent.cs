using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using PuzzleCourse.Game.Autoload;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.Component;

public partial class BuildingComponent : Node2D
{
    private readonly HashSet<Vector2I> _occupiedTiles = [];

    [Export]
    private BuildingAnimatorComponent? _buildingAnimatorComponent;

    [Export(PropertyHint.File, "*.tres")]
    private string _buildingResourcePath = null!;

    public bool IsDestroying { get; private set; }

    public BuildingResource BuildingResource { get; private set; } = null!;

    public override void _Ready()
    {
        Debug.Assert(_buildingResourcePath != null,
            "BuildingResourcePath export variable not set in BuildingComponent.tscn");

        BuildingResource = GD.Load<BuildingResource>(_buildingResourcePath);

        _buildingAnimatorComponent?.DestroyedAnimationFinished += OnDestroyAnimationFinished;

        AddToGroup(nameof(BuildingComponent));

        Callable
            .From(Initialize)
            .CallDeferred();
    }

    public void Destroy()
    {
        IsDestroying = true;

        GameEvents.EmitBuildingDestroyed(this);
        _buildingAnimatorComponent?.PlayDestroyAnimation();

        if (_buildingAnimatorComponent == null) Owner.QueueFree();
    }

    public Vector2I GetGridCellPosition()
    {
        var (x, y) = (GlobalPosition / Grid.CellPixelSize).Floor();

        return new Vector2I((int)x, (int)y);
    }

    public HashSet<Vector2I> GetOccupiedCellPositions() => _occupiedTiles.ToHashSet();

    public static IEnumerable<BuildingComponent> GetValidBuildingComponents(Node node) =>
        node.GetTree()
            .GetNodesInGroup(nameof(BuildingComponent))
            .Cast<BuildingComponent>()
            .Where(comp => !comp.IsDestroying);

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

    private void OnDestroyAnimationFinished()
    {
        Owner.QueueFree();
    }
}
