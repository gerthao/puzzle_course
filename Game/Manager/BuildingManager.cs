using Godot;
using PuzzleCourse.Game.UI;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.Manager;

public partial class BuildingManager : Node
{
    private readonly StringName _actionCancel = "cancel";

    private readonly StringName _actionLeftClick = "left_click";

    private readonly StringName _actionRightClick = "right_click";

    private BuildingGhost _buildingGhost;

    [Export]
    private PackedScene _buildingGhostScene;

    private int _currentlyUsedResourceCount;
    private int _currentResourceCount;

    private State _currentState = State.Normal;

    [Export]
    private GameUI _gameUI;

    [Export]
    private GridManager _gridManager;

    private Vector2I _hoveredGridCell;

    private int _startingResourceCount = 4;

    private BuildingResource _toPlaceBuildingResource;

    [Export]
    private Node2D _ySortRoot;

    private int AvailableResourceCount => _startingResourceCount + _currentResourceCount - _currentlyUsedResourceCount;

    private enum State
    {
        Normal,
        PlacingBuilding,
    }

    public override void _Process(double delta)
    {
        var currentGridCellPosition = _gridManager.GetMouseGridCellPosition();

        if (_hoveredGridCell != currentGridCellPosition)
        {
            _hoveredGridCell = currentGridCellPosition;
            UpdateHoveredGridCell();
        }

        switch (_currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                _buildingGhost.Position = currentGridCellPosition * Grid.CellPixelSize;
                break;
        }
    }

    public override void _Ready()
    {
        _gameUI.BuildingResourceSelected  += OnPlaceBuildingResourceSelected;
        _gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        switch (_currentState)
        {
            case State.Normal when @event.IsActionPressed(_actionRightClick):
                DestroyBuildingAtHoveredCellPosition();
                break;
            case State.PlacingBuilding when @event.IsActionPressed(_actionCancel):
                ChangeState(State.Normal);
                break;
            case State.PlacingBuilding when CanPlaceBuilding(@event):
                PlaceBuildingAtHoveredCellPosition();
                break;
        }
    }

    private bool CanPlaceBuilding(InputEvent @event) =>
        @event.IsActionPressed(_actionLeftClick)
        && _toPlaceBuildingResource != null
        && IsBuildingPlaceableAtTile(_hoveredGridCell);

    private void ChangeState(State newState)
    {
        switch (_currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                ClearBuildingGhost();
                _toPlaceBuildingResource = null;
                break;
        }

        _currentState = newState;

        switch (_currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                _buildingGhost = _buildingGhostScene.Instantiate<BuildingGhost>();
                _ySortRoot.AddChild(_buildingGhost);
                break;
        }
    }

    private void ClearBuildingGhost()
    {
        _gridManager.ClearHighlightTileMapLayer();

        if (IsInstanceValid(_buildingGhost)) _buildingGhost.QueueFree();

        _buildingGhost = null;
    }

    private void DestroyBuildingAtHoveredCellPosition()
    {
    }

    private bool IsBuildingPlaceableAtTile(Vector2I tilePosition) =>
        _gridManager.IsWithinValidBuildArea(tilePosition)
        && AvailableResourceCount >= _toPlaceBuildingResource.ResourceCost;

    private bool IsPlacingBuilding(Vector2I gridPosition) =>
        _hoveredGridCell != gridPosition;

    private void OnPlaceBuildingResourceSelected(BuildingResource resource)
    {
        ChangeState(State.PlacingBuilding);

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
        var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        building.GlobalPosition = _hoveredGridCell * Grid.CellPixelSize;

        _ySortRoot.AddChild(building);

        _currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;

        ChangeState(State.Normal);
    }

    private void UpdateGridDisplay()
    {
        _gridManager.ClearHighlightTileMapLayer();
        _gridManager.HighlightBuildRadiusOfOccupiedTiles();

        if (!IsBuildingPlaceableAtTile(_hoveredGridCell))
        {
            _buildingGhost.SetInvalid();
            return;
        }

        _gridManager.HighlightPotentialTilesForBuildings(
            _hoveredGridCell,
            _toPlaceBuildingResource.BuildableRadius);

        _gridManager.HighlightResourceTiles(
            _hoveredGridCell,
            _toPlaceBuildingResource.ResourceRadius);

        _buildingGhost.SetValid();
    }

    private void UpdateHoveredGridCell()
    {
        switch (_currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                UpdateGridDisplay();
                break;
            default:
                return;
        }
    }
}
