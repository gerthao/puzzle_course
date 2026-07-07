using System.Diagnostics;
using Godot;

namespace PuzzleCourse.Game;

public partial class GoldMine : Node2D
{
    [Export]
    private Texture2D _activeTexture = null!;

    private Sprite2D _sprite = null!;

    public override void _Ready()
    {
        Debug.Assert(_activeTexture != null, "ActiveTexture export variable not set in GoldMine.tscn");

        _sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public void SetActive() => _sprite.Texture = _activeTexture;
}
