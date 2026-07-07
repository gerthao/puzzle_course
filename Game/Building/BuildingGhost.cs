using Godot;

namespace PuzzleCourse.Game.Building;

public partial class BuildingGhost : Node2D
{
    private Node2D _bottomLeft = null!;
    private Node2D _bottomRight = null!;
    private Node2D _spriteRoot = null!;
    private Tween _spriteTween = null!;
    private Node2D _topLeft = null!;
    private Node2D _topRight = null!;
    private Node2D _upDownRoot = null!;

    public override void _Ready()
    {
        _topLeft = GetNode<Node2D>("%TopLeft");
        _topRight = GetNode<Node2D>("%TopRight");
        _bottomLeft = GetNode<Node2D>("%BottomLeft");
        _bottomRight = GetNode<Node2D>("%BottomRight");
        _spriteRoot = GetNode<Node2D>("%SpriteRoot");
        _upDownRoot = GetNode<Node2D>("%UpDownRoot");

        var upDownTween = CreateTween();
        upDownTween.SetLoops(); // sets indefinitely
        upDownTween
            .TweenProperty(_upDownRoot, "position", Vector2.Down * 6, 0.5)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Quad);
        upDownTween
            .TweenProperty(_upDownRoot, "position", Vector2.Up * 6, 0.5)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Quad);
    }

    public void AddSpriteNode(Node2D spriteNode) => _upDownRoot.AddChild(spriteNode);

    public void DoHoverAnimation()
    {
        if (_spriteTween != null && _spriteTween.IsValid()) _spriteTween.Kill();

        _spriteTween = CreateTween();
        _spriteTween
            .TweenProperty(_spriteRoot, "global_position", GlobalPosition, 0.3)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
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
        _upDownRoot.Modulate = Modulate;
    }

    public void SetValid()
    {
        Modulate = Colors.White;
        _upDownRoot.Modulate = Modulate;
    }
}
