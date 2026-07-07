using Godot;
using PuzzleCourse.Game.Autoload;

namespace PuzzleCourse.Game.UI;

public partial class LevelCompleteScreen : CanvasLayer
{
    private Button _nextLevelButton = null!;

    public override void _Ready()
    {
        _nextLevelButton = GetNode<Button>("%NextLevelButton");
        _nextLevelButton.Pressed += OnNextLevelPressed;
    }

    private static void OnNextLevelPressed()
    {
        LevelManager.Instance.ChangeToNextLevel();
    }
}
