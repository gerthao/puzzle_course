using System.Diagnostics;
using Godot;

namespace PuzzleCourse.Game.Component;

public partial class BuildingAnimatorComponent : Node2D
{
    [Signal]
    public delegate void DestroyedAnimationFinishedEventHandler();

    private Tween? _activeTween;
    private Node2D? _animationRootNode;

    [Export]
    private PackedScene _impactParticlesScene = null!;

    private Sprite2D _maskNode = null!;

    [Export]
    private Texture2D _maskTexture = null!;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Debug.Assert(_maskTexture != null, "MaskTexture export variable not set in BuildingAnimatorComponent.tscn");
        Debug.Assert(_impactParticlesScene != null,
            "ImpactParticleScene export variable not set in BuildingAnimatorComponent.tscn");

        SetUpNodes();
    }

    public void PlayDestroyAnimation()
    {
        if (_animationRootNode == null) return;

        if (_activeTween != null && _activeTween.IsValid()) _activeTween.Kill();

        _animationRootNode.Position = Vector2.Zero;

        _maskNode.ClipChildren = ClipChildrenMode.Only;
        _maskNode.Texture = _maskTexture;

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

        _activeTween.TweenCallback(Callable.From(CreateImpactParticles));

        _activeTween
            .TweenProperty(_animationRootNode, "position", Vector2.Up * 16, 0.1)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        _activeTween
            .TweenProperty(_animationRootNode, "position", Vector2.Zero, 0.1)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
    }

    private void CreateImpactParticles()
    {
        var impactParticles = _impactParticlesScene.Instantiate<Node2D>();
        GetParent()?.AddChild(impactParticles);
        impactParticles.GlobalPosition = GlobalPosition;
    }

    private void SetUpNodes()
    {
        var spriteNode = GetChild<Node2D>(0);

        if (spriteNode == null) return;
        RemoveChild(spriteNode);

        Position = new Vector2(spriteNode.Position.X, spriteNode.Position.Y);

        _maskNode = new Sprite2D
        {
            Centered = false,
            Offset = new Vector2(-160, -256),
        };

        AddChild(_maskNode);

        _animationRootNode = new Node2D();
        _maskNode.AddChild(_animationRootNode);
        _animationRootNode.AddChild(spriteNode);

        spriteNode.Position = Vector2.Zero;
    }
}
