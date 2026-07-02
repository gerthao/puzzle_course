using Godot;

namespace PuzzleCourse.Game.Building;

public partial class BuildingGhost : Node2D
{
    private Node2D _bottomLeft;
    private Node2D _bottomRight;
    private Node2D _topLeft;
    private Node2D _topRight;

    public override void _Ready()
    {
        _topLeft = GetNode<Node2D>("%TopLeft");
        _topRight = GetNode<Node2D>("%TopRight");
        _bottomLeft = GetNode<Node2D>("%BottomLeft");
        _bottomRight = GetNode<Node2D>("%BottomRight");
    }

    public void SetDimensions(Vector2I dimensions)
    {
        _topLeft.Position = dimensions * new Vector2(0, 0);
        _topRight.Position = dimensions * new Vector2(Grid.CellPixelSize, 0);
        _bottomLeft.Position = dimensions * new Vector2(0, Grid.CellPixelSize);
        _bottomRight.Position = dimensions * new Vector2(Grid.CellPixelSize, Grid.CellPixelSize);
    }

    public void SetInvalid()
    {
        Modulate = Colors.Red;
    }

    public void SetValid()
    {
        Modulate = Colors.White;
    }
}
