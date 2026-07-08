using System.Diagnostics;
using System.Linq;
using Godot;
using PuzzleCourse.Game.Building;
using PuzzleCourse.Game.Component;
using PuzzleCourse.Game.UI;
using PuzzleCourse.Resources.Building;
using PuzzleCourse.Scripts;

namespace PuzzleCourse.Game.Manager;

public partial class BuildingManager : Node
{
    [Signal]
    public delegate void AvailableResourceCountUpdatedEventHandler(int availableResourceCount);

    private readonly StringName _actionCancel = "cancel";

    private readonly StringName _actionLeftClick = "left_click";

    private readonly StringName _actionRightClick = "right_click";

    private BuildingGhost? _buildingGhost;

    private Vector2I? _buildingGhostDimensions;

    [Export]
    private PackedScene _buildingGhostScene = null!;

    private int _currentlyUsedResourceCount;
    private int _currentResourceCount;

    private State _currentState = State.Normal;

    [Export]
    private GameUI _gameUI = null!;

    [Export]
    private GridManager _gridManager = null!;


    private Rect2I _hoveredGridArea = new(Vector2I.Zero, Vector2I.One);

    private int _startingResourceCount;

    private BuildingResource? _toPlaceBuildingResource;

    [Export]
    private Node2D _ySortRoot = null!;


    private int AvailableResourceCount => _startingResourceCount + _currentResourceCount - _currentlyUsedResourceCount;

    private enum State
    {
        Normal,
        PlacingBuilding,
    }

    public override void _Process(double delta)
    {
        Vector2I mouseGridPosition;

        switch (_currentState)
        {
            case State.PlacingBuilding when _buildingGhost != null && _buildingGhostDimensions != null:
                mouseGridPosition =
                    _gridManager.GetMouseGetCellPositionWithDimensionOffset(_buildingGhostDimensions.Value);
                _buildingGhost.Position = mouseGridPosition * Grid.CellPixelSize;
                break;
            case State.Normal:
            default:
                mouseGridPosition = _gridManager.GetMouseGridCellPosition();
                break;
        }

        var rootCell = _hoveredGridArea.Position;
        if (rootCell == mouseGridPosition) return;

        _hoveredGridArea.Position = mouseGridPosition;
        UpdateHoveredGridArea();
    }

    public override void _Ready()
    {
        Debug.Assert(_ySortRoot != null, "YSortRoot export variable not set in BuildingManager.tscn");
        Debug.Assert(_gameUI != null, "GameUI export variable not set in BuildingManager.tscn");
        Debug.Assert(_gridManager != null, "GridManager export variable not set in BuildingManager.tscn");
        Debug.Assert(_buildingGhostScene != null, "BuildingGhostScene export variable not set in BuildingManager.tscn");

        _gameUI.BuildingResourceSelected += OnPlaceBuildingResourceSelected;
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

    public void SetStartingResourceCount(int startingResourceCount) => _startingResourceCount = startingResourceCount;

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
            BuildingComponent.GetValidBuildingComponents(this)
                .FirstOrDefault(bc => bc.BuildingResource.IsDeleteable && bc.IsTileInBuildingArea(rootCell));

        if (buildingToDestroy == null) return;

        _currentlyUsedResourceCount -= buildingToDestroy.BuildingResource.ResourceCost;

        buildingToDestroy.Destroy();
    }

    private bool IsBuildingPlaceableAtArea(Rect2I tileArea)
    {
        Debug.Assert(_toPlaceBuildingResource != null);

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
        _buildingGhost?.AddSpriteNode(buildingSprite);
        _buildingGhost?.SetDimensions(resource.Dimensions);
        _buildingGhostDimensions = resource.Dimensions;
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
        Debug.Assert(_toPlaceBuildingResource != null);

        var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        _ySortRoot.AddChild(building);

        building.GlobalPosition = _hoveredGridArea.Position * Grid.CellPixelSize;
        building.FindFirstNodeOfType<BuildingAnimatorComponent>()?.PlayInAnimation();

        _currentlyUsedResourceCount += _toPlaceBuildingResource.ResourceCost;

        ChangeState(State.Normal);
        EmitSignalAvailableResourceCountUpdated(AvailableResourceCount);
    }

    private void UpdateGridDisplay()
    {
        Debug.Assert(_buildingGhost != null);
        Debug.Assert(_toPlaceBuildingResource != null);
        Debug.Assert(_gridManager != null);

        _gridManager.ClearHighlightTileMapLayer();
        _gridManager.HighlightBuildRadiusOfOccupiedTiles();

        if (!IsBuildingPlaceableAtArea(_hoveredGridArea))
        {
            _buildingGhost.SetInvalid();
        }
        else
        {
            _gridManager.HighlightPotentialTilesForBuildings(
                _hoveredGridArea,
                _toPlaceBuildingResource.BuildableRadius);

            _gridManager.HighlightResourceTiles(
                _hoveredGridArea,
                _toPlaceBuildingResource.ResourceRadius);

            _buildingGhost.SetValid();
        }

        _buildingGhost.DoHoverAnimation();
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
