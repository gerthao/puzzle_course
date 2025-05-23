using Godot;
using PuzzleCourse.Game.Manager;

namespace PuzzleCourse.Game;

public partial class Main : Node
{
    private Sprite2D _cursor;
    private PackedScene _buildingScene;
    private Button _placeBuildingButton;
    private GridManager _gridManager;

    private Vector2I? _hoveredGridCell;

    public override void _Ready()
    {
        _cursor              = GetNode<Sprite2D>("Cursor");
        _placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
        _gridManager         = GetNode<GridManager>("GridManager");
        _buildingScene       = GD.Load<PackedScene>("res://Game/Building/Building.tscn");

        _cursor.Visible = false;

        _placeBuildingButton.Pressed += OnButtonPressed;
    }

    public override void _Process(double delta)
    {
        var gridPos = _gridManager.GetMouseGridCellPosition();
        _cursor.Position = gridPos * Grid.CellPixelSize;

        if (!IsPlacingBuilding(gridPos)) return;

        _hoveredGridCell = gridPos;
        _gridManager.HighlightBuildableTiles();
    }

    private bool IsPlacingBuilding(Vector2I gridPosition) =>
        _cursor.Visible && (!_hoveredGridCell.HasValue || _hoveredGridCell.Value != gridPosition);

    private bool CanPlaceBuilding(InputEvent @event) =>
        @event.IsActionPressed("left_click")
        && _hoveredGridCell.HasValue
        && _gridManager.IsTilePositionValid(_hoveredGridCell.Value);


    public override void _UnhandledInput(InputEvent @event)
    {
        if (!CanPlaceBuilding(@event)) return;

        PlaceBuildingAtHoveredCellPosition();
        _cursor.Visible = false;
    }


    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!_hoveredGridCell.HasValue) return;

        var building = _buildingScene.Instantiate<Node2D>();
        AddChild(building);

        building.GlobalPosition = _hoveredGridCell.Value * Grid.CellPixelSize;

        _gridManager.MarkTileAsOccupied(_hoveredGridCell.Value) ;


        _hoveredGridCell = null;
        _gridManager.ClearHighlightTileMapLayer();
    }

    private void OnButtonPressed() => _cursor.Visible = true;
}
