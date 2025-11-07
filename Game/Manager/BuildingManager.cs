using Godot;
using PuzzleCourse.Game.UI;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.Manager;

public partial class BuildingManager : Node
{
    private int _currentlyUsedResourceCount;
    private int _currentResourceCount;

    [Export]
    private Sprite2D _cursor;

    [Export]
    private GameUI _gameUI;

    [Export]
    private GridManager _gridManager;

    private Vector2I? _hoveredGridCell;

    private int _startingResourceCount = 4;

    private BuildingResource _toPlaceBuildingResource;

    [Export]
    private Node2D _ySortRoot;

    private int AvailableResourceCount => _startingResourceCount + _currentResourceCount - _currentlyUsedResourceCount;

    public override void _Process(double delta)
    {
        var currentGridCellPosition = _gridManager.GetMouseGridCellPosition();
        _cursor.Position = currentGridCellPosition * Grid.CellPixelSize;

        if (_toPlaceBuildingResource == null || !IsPlacingBuilding(currentGridCellPosition)) return;

        _hoveredGridCell = currentGridCellPosition;

        _gridManager.ClearHighlightTileMapLayer();

        _gridManager.HighlightPotentialTilesForBuildings(
            _hoveredGridCell.Value,
            _toPlaceBuildingResource.BuildableRadius);

        _gridManager.HighlightResourceTiles(
            _hoveredGridCell.Value,
            _toPlaceBuildingResource.ResourceRadius);
    }

    public override void _Ready()
    {
        _gameUI.BuildingResourceSelected  += OnPlaceBuildingResourceSelected;
        _gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!CanPlaceBuilding(@event)) return;

        PlaceBuildingAtHoveredCellPosition();
        _cursor.Visible = false;
    }

    private bool CanPlaceBuilding(InputEvent @event) =>
        @event.IsActionPressed("left_click")
        && _toPlaceBuildingResource != null
        && _hoveredGridCell.HasValue
        && _gridManager.IsWithinValidBuildArea(_hoveredGridCell.Value)
        && AvailableResourceCount >= _toPlaceBuildingResource.ResourceCost;

    private bool IsPlacingBuilding(Vector2I gridPosition) =>
        _cursor.Visible && (!_hoveredGridCell.HasValue || _hoveredGridCell.Value != gridPosition);

    private void OnPlaceBuildingResourceSelected(BuildingResource resource)
    {
        _toPlaceBuildingResource = resource;
        _cursor.Visible          = true;

        _gridManager.HighlightBuildRadiusOfOccupiedTiles();
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        _currentResourceCount = resourceCount;
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!_hoveredGridCell.HasValue) return;

        var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        building.GlobalPosition = _hoveredGridCell.Value * Grid.CellPixelSize;

        _ySortRoot.AddChild(building);

        _hoveredGridCell = null;
        _gridManager.ClearHighlightTileMapLayer();

        _currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;

        GD.Print($"Available resource count: {AvailableResourceCount}");
    }
}
