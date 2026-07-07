using Godot;

namespace PuzzleCourse.Game.Component;

public partial class BuildingAnimatorComponent : Node2D
{
    [Signal]
    public delegate void DestroyedAnimationFinishedEventHandler();

    private Tween? _activeTween;
    private Node2D? _animationRootNode;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetUpNodes();
    }

    public void PlayDestroyAnimation()
    {
        if (_animationRootNode == null) return;

        if (_activeTween != null && _activeTween.IsValid()) _activeTween.Kill();

        _activeTween = CreateTween();

        _activeTween.TweenProperty(_animationRootNode, "rotation_degrees", -5, 0.1);
        _activeTween.TweenProperty(_animationRootNode, "rotation_degrees", 5, 0.1);
        _activeTween.TweenProperty(_animationRootNode, "rotation_degrees", -2, 0.1);
        _activeTween.TweenProperty(_animationRootNode, "rotation_degrees", 2, 0.1);
        _activeTween.TweenProperty(_animationRootNode, "rotation_degrees", 0, 0.1);

        _activeTween
            .TweenProperty(_animationRootNode, "position", Vector2.Down * 300, 0.4)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.In);

        _activeTween.Finished += EmitSignalDestroyedAnimationFinished;
    }

    public void PlayInAnimation()
    {
        if (_animationRootNode == null) return;

        if (_activeTween != null && _activeTween.IsValid()) _activeTween.Kill();

        _activeTween = CreateTween();

        _activeTween
            .TweenProperty(_animationRootNode, "position", Vector2.Zero, 0.3)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In)
            .From(Vector2.Up * 124);

        _activeTween
            .TweenProperty(_animationRootNode, "position", Vector2.Up * 16, 0.1)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        _activeTween
            .TweenProperty(_animationRootNode, "position", Vector2.Zero, 0.1)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
    }

    private void SetUpNodes()
    {
        var spriteNode = GetChild<Node2D>(0);

        if (spriteNode == null) return;
        RemoveChild(spriteNode);

        Position = new Vector2(spriteNode.Position.X, spriteNode.Position.Y);
        _animationRootNode = new Node2D();
        AddChild(_animationRootNode);
        _animationRootNode.AddChild(spriteNode);

        spriteNode.Position = Vector2.Zero;
    }
}
