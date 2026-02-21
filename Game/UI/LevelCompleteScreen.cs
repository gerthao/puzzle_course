using Godot;
using PuzzleCourse.Game.Autoload;

namespace PuzzleCourse.Game.UI;

public partial class LevelCompleteScreen : CanvasLayer
{
    private Button _nextLevelButton;

    public override void _Ready()
    {
        _nextLevelButton         =  GetNode<Button>("%NextLevelButton");
        _nextLevelButton.Pressed += OnNextLevelPressed;
    }

    private void OnNextLevelPressed()
    {
        LevelManager.Instance.ChangeToNextLevel();
    }
}
