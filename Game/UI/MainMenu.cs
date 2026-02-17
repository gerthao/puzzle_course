using Godot;
using PuzzleCourse.Game.Autoload;

namespace PuzzleCourse.Game.UI;

public partial class MainMenu : Node
{
    private Button _optionsButton;
    private Button _playButton;
    private Button _quitButton;


    public override void _Ready()
    {
        _optionsButton = GetNode<Button>("%OptionsButton");
        _playButton    = GetNode<Button>("%PlayButton");
        _quitButton    = GetNode<Button>("%QuitButton");

        _playButton.Pressed += OnPlayButtonPressed;
    }

    private void OnPlayButtonPressed()
    {
        LevelManager.Instance.ChangeToLevel(0);
    }
}
