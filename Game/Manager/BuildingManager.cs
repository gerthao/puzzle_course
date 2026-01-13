using Godot;
using PuzzleCourse.Game.UI;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.Manager;

public partial class BuildingManager : Node
{
    private BuildingGhost _buildingGhost;

    [Export]
    private PackedScene _buildingGhostScene;

    private int _currentlyUsedResourceCount;
    private int _currentResourceCount;

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
        if (!IsInstanceValid(_buildingGhost)) return;

        var currentGridCellPosition = _gridManager.GetMouseGridCellPosition();
        _buildingGhost.Position = currentGridCellPosition * Grid.CellPixelSize;

        if (_toPlaceBuildingResource == null ||
            (_hoveredGridCell.HasValue && _hoveredGridCell.Value == currentGridCellPosition))
            return;

        _hoveredGridCell = currentGridCellPosition;
        UpdateGridDisplay();
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
    }

    private bool CanPlaceBuilding(InputEvent @event) =>
        @event.IsActionPressed("left_click")
        && _toPlaceBuildingResource != null
        && _hoveredGridCell.HasValue
        && IsBuildingPlaceableAtTile(_hoveredGridCell.Value);

    private bool IsBuildingPlaceableAtTile(Vector2I tilePosition) =>
        _gridManager.IsWithinValidBuildArea(tilePosition)
        && AvailableResourceCount >= _toPlaceBuildingResource.ResourceCost;

    private bool IsPlacingBuilding(Vector2I gridPosition) =>
        !_hoveredGridCell.HasValue || _hoveredGridCell.Value != gridPosition;

    private void OnPlaceBuildingResourceSelected(BuildingResource resource)
    {
        if (IsInstanceValid(_buildingGhost)) _buildingGhost.QueueFree();

        _buildingGhost = _buildingGhostScene.Instantiate<BuildingGhost>();
        _ySortRoot.AddChild(_buildingGhost);

        var buildingSprite = resource.SpriteScene.Instantiate<Sprite2D>();
        _buildingGhost.AddChild(buildingSprite);

        _toPlaceBuildingResource = resource;

        UpdateGridDisplay();
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

        _buildingGhost.QueueFree();
        _buildingGhost = null;
    }

    private void UpdateGridDisplay()
    {
        if (_hoveredGridCell == null) return;

        _gridManager.ClearHighlightTileMapLayer();
        _gridManager.HighlightBuildRadiusOfOccupiedTiles();

        if (!IsBuildingPlaceableAtTile(_hoveredGridCell.Value))
        {
            _buildingGhost.SetInvalid();
            return;
        }

        _gridManager.HighlightPotentialTilesForBuildings(
            _hoveredGridCell.Value,
            _toPlaceBuildingResource.BuildableRadius);

        _gridManager.HighlightResourceTiles(
            _hoveredGridCell.Value,
            _toPlaceBuildingResource.ResourceRadius);

        _buildingGhost.SetValid();
    }
}
