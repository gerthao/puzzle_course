using System.Collections.Generic;
using Godot;
using PuzzleCourse.Game.Manager;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game;

public partial class Main : Node
{
    private Sprite2D         _cursor;
    private BuildingResource _towerResource;
    private BuildingResource _villageResource;
    private Button           _placeTowerButton;
    private Button           _placeVillageButton;
    private GridManager      _gridManager;
    private Vector2I?        _hoveredGridCell;
    private Node2D           _ySortRoot;
    private BuildingResource _toPlaceBuildingResource;

    public override void _Ready()
    {
        _cursor             = GetNode<Sprite2D>("Cursor");
        _placeTowerButton   = GetNode<Button>("PlaceTowerButton");
        _placeVillageButton = GetNode<Button>("PlaceVillageButton");
        _gridManager        = GetNode<GridManager>("GridManager");
        _ySortRoot          = GetNode<Node2D>("YSortRoot");
        _towerResource      = GD.Load<BuildingResource>("res://Resources/Building/Tower.tres");
        _villageResource   = GD.Load<BuildingResource>("res://Resources/Building/Village.tres");

        _cursor.Visible = false;

        _placeTowerButton.Pressed   += OnPlaceTowerButtonPressed;
        _placeVillageButton.Pressed += OnPlaceVillageButtonPressed;
    }

    public override void _Process(double delta)
    {
        var currentGridCellPosition = _gridManager.GetMouseGridCellPosition();
        _cursor.Position = currentGridCellPosition * Grid.CellPixelSize;

        if (_toPlaceBuildingResource == null || !IsPlacingBuilding(currentGridCellPosition)) return;

        _hoveredGridCell = currentGridCellPosition;
        _gridManager.HighlightBuildRadiusOfIntendedTile(_hoveredGridCell.Value,
            _toPlaceBuildingResource.BuildableRadius);
    }

    private bool IsPlacingBuilding(Vector2I gridPosition) =>
        _cursor.Visible && (!_hoveredGridCell.HasValue || _hoveredGridCell.Value != gridPosition);

    private bool CanPlaceBuilding(InputEvent @event) =>
        @event.IsActionPressed("left_click")
        && _hoveredGridCell.HasValue
        && _gridManager.IsWithinValidBuildArea(_hoveredGridCell.Value);


    public override void _UnhandledInput(InputEvent @event)
    {
        if (!CanPlaceBuilding(@event)) return;

        PlaceBuildingAtHoveredCellPosition();
        _cursor.Visible = false;
    }


    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!_hoveredGridCell.HasValue) return;

        var building = _toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        building.GlobalPosition = _hoveredGridCell.Value * Grid.CellPixelSize;

        _ySortRoot.AddChild(building);

        _hoveredGridCell = null;
        _gridManager.ClearHighlightTileMapLayer();
    }

    private void OnPlaceTowerButtonPressed()
    {
        _toPlaceBuildingResource = _towerResource;
        _cursor.Visible          = true;

        _gridManager.HighlightBuildRadiiOfOccupiedTiles();
    }

    private void OnPlaceVillageButtonPressed()
    {
        _toPlaceBuildingResource = _villageResource;
        _cursor.Visible          = true;

        _gridManager.HighlightBuildRadiiOfOccupiedTiles();
    }
}
