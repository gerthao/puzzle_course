using Godot;

namespace PuzzleCourse.Game;

public partial class Main : Node2D
{
    private const float PixelSize = 64;

    private Sprite2D _cursor;
    private PackedScene _buildingScene;
    private Button _placeBuildingButton;
    private TileMapLayer _highlightTileMapLayer;

    private Vector2? _hoveredGridCell;

    public override void _Ready()
    {
        _cursor                = GetNode<Sprite2D>("Cursor");
        _buildingScene         = GD.Load<PackedScene>("res://Game/Building/Building.tscn");
        _placeBuildingButton   = GetNode<Button>("PlaceBuildingButton");
        _highlightTileMapLayer = GetNode<TileMapLayer>("HighlightTileMapLayer");

        _cursor.Visible = false;

        _placeBuildingButton.Pressed += OnButtonPressed;
    }

    public override void _Process(double delta)
    {
        var gridPos = GetMouseGridCellPosition;
        _cursor.Position = gridPos * PixelSize;

        if (IsPlacingBuilding(gridPos))
        {
            _hoveredGridCell = gridPos;
            UpdateHighlightTileMapLayer();
        }
    }

    private bool IsPlacingBuilding(Vector2 gridPosition) =>
        _cursor.Visible && (!_hoveredGridCell.HasValue || _hoveredGridCell.Value != gridPosition);

    private bool CanPlaceBuilding(InputEvent @event) => _cursor.Visible && @event.IsActionPressed("left_click");

    private void UpdateHighlightTileMapLayer()
    {
        _highlightTileMapLayer.Clear();
        
        if (!_hoveredGridCell.HasValue) return;

        for (var x = _hoveredGridCell.Value.X - 3; x <= _hoveredGridCell.Value.X + 3; x++)
        {
            for (var y = _hoveredGridCell.Value.Y - 3; y <= _hoveredGridCell.Value.Y + 3; y++)
            {
                _highlightTileMapLayer.SetCell(
                    coords: new Vector2I((int)x, (int)y),
                    sourceId: 0,
                    atlasCoords: Vector2I.Zero
                );
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!CanPlaceBuilding(@event)) return;

        PlaceBuildingAtMousePosition();
        _cursor.Visible = false;
    }

    private Vector2 GetMouseGridCellPosition => (GetGlobalMousePosition() / PixelSize).Floor();

    private void PlaceBuildingAtMousePosition()
    {
        var building = _buildingScene.Instantiate<Node2D>();
        
        AddChild(building);

        building.GlobalPosition = GetMouseGridCellPosition * PixelSize;

        _hoveredGridCell = null;
        
        UpdateHighlightTileMapLayer();
    }

    private void OnButtonPressed()
    {
        _cursor.Visible = true;
    }
}
