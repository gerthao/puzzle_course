using Godot;

namespace PuzzleCourse.Game;

public partial class Main : Node2D
{
    private const float PixelSize = 64;
    private Sprite2D _cursor;
    private PackedScene _buildingScene;
    private Button _placeBuildingButton;

    public override void _Ready()
    {
        _cursor              = GetNode<Sprite2D>("Cursor");
        _buildingScene       = GD.Load<PackedScene>("res://Game/Building/Building.tscn");
        _placeBuildingButton = GetNode<Button>("PlaceBuildingButton");

        _cursor.Visible = false;
        
        _placeBuildingButton.Pressed += OnButtonPressed;
    }

    public override void _Process(double delta)
    {
        _cursor.Position = GetMouseGridCellPosition * PixelSize;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_cursor.Visible && @event.IsActionPressed("left_click"))
        {
            PlaceBuildingAtMousePosition();
            _cursor.Visible = false;
        }
    }

    private Vector2 GetMouseGridCellPosition => (GetGlobalMousePosition() / PixelSize).Floor();

    private void PlaceBuildingAtMousePosition()
    {
        var building = _buildingScene.Instantiate<Node2D>();

        AddChild(building);

        building.GlobalPosition = GetMouseGridCellPosition * PixelSize;
    }

    private void OnButtonPressed()
    {
        _cursor.Visible = true;
    }
}
