using System.Linq;
using Godot;
using PuzzleCourse.Game.Component;
using PuzzleCourse.Game.UI;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.Manager;

public partial class BuildingManager : Node
{
    [Signal]
    public delegate void AvailableResourceCountUpdatedEventHandler(int availableResourceCount);

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

    private Rect2I _hoveredGridArea = new(Vector2I.Zero, Vector2I.One);

    [Export]
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
        var mouseGridPosition = _gridManager.GetMouseGridCellPosition();
        var rootCell          = _hoveredGridArea.Position;

        if (rootCell != mouseGridPosition)
        {
            _hoveredGridArea.Position = mouseGridPosition;
            UpdateHoveredGridArea();
        }

        switch (_currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                _buildingGhost.Position = mouseGridPosition * Grid.CellPixelSize;
                break;
        }
    }

    public override void _Ready()
    {
        _gameUI.BuildingResourceSelected  += OnPlaceBuildingResourceSelected;
        _gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;

        Callable
            .From(() => EmitSignalAvailableResourceCountUpdated(AvailableResourceCount))
            .CallDeferred();
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
        && IsBuildingPlaceableAtArea(_hoveredGridArea);

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
        var rootCell = _hoveredGridArea.Position;

        var buildingToDestroy =
            GetTree()
                .GetNodesInGroup(nameof(BuildingComponent))
                .Cast<BuildingComponent>()
                .FirstOrDefault(bc => bc.BuildingResource.IsDeleteable && bc.IsTileInBuildingArea(rootCell));

        if (buildingToDestroy == null) return;

        _currentlyUsedResourceCount -= buildingToDestroy.BuildingResource.ResourceCost;

        buildingToDestroy.Destroy();
    }

    private bool IsBuildingPlaceableAtArea(Rect2I tileArea)
    {
        var allTilesBuildable = _gridManager.IsTileAreaBuildable(tileArea);

        return allTilesBuildable
            && AvailableResourceCount >= _toPlaceBuildingResource.ResourceCost;
    }

    private bool IsPlacingBuilding(Vector2I gridPosition) =>
        _hoveredGridArea.Position != gridPosition;

    private void OnPlaceBuildingResourceSelected(BuildingResource resource)
    {
        ChangeState(State.PlacingBuilding);

        _hoveredGridArea.Size = resource.Dimensions;

        var buildingSprite = resource.SpriteScene.Instantiate<Sprite2D>();
        _buildingGhost.AddChild(buildingSprite);

        _toPlaceBuildingResource = resource;

        UpdateGridDisplay();
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        _currentResourceCount = resourceCount;

        EmitSignalAvailableResourceCountUpdated(AvailableResourceCount);
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        building.GlobalPosition = _hoveredGridArea.Position * Grid.CellPixelSize;

        _ySortRoot.AddChild(building);

        _currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;

        ChangeState(State.Normal);
        EmitSignalAvailableResourceCountUpdated(AvailableResourceCount);
    }

    private void UpdateGridDisplay()
    {
        _gridManager.ClearHighlightTileMapLayer();
        _gridManager.HighlightBuildRadiusOfOccupiedTiles();

        if (!IsBuildingPlaceableAtArea(_hoveredGridArea))
        {
            _buildingGhost.SetInvalid();
            return;
        }

        _gridManager.HighlightPotentialTilesForBuildings(
            _hoveredGridArea,
            _toPlaceBuildingResource.BuildableRadius);

        _gridManager.HighlightResourceTiles(
            _hoveredGridArea,
            _toPlaceBuildingResource.ResourceRadius);

        _buildingGhost.SetValid();
    }

    private void UpdateHoveredGridArea()
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
