using Godot;

namespace PuzzleCourse.Game.UI;

public partial class MainMenu : Node
{
    private LevelSelectScreen _levelSelectScreen = null!;
    private Control _mainMenuContainer = null!;
    private Button _optionsButton = null!;
    private Button _playButton = null!;
    private Button _quitButton = null!;


    public override void _Ready()
    {
        _optionsButton = GetNode<Button>("%OptionsButton");
        _playButton = GetNode<Button>("%PlayButton");
        _quitButton = GetNode<Button>("%QuitButton");
        _mainMenuContainer = GetNode<Control>("%MainMenuContainer");
        _levelSelectScreen = GetNode<LevelSelectScreen>("%LevelSelectScreen");

        _playButton.Pressed += OnPlayButtonPressed;
        _levelSelectScreen.BackPressed += OnLevelSelectScreenBackPressed;
        _quitButton.Pressed += OnQuitButtonPressed;

        _mainMenuContainer.Visible = true;
        _levelSelectScreen.Visible = false;
    }

    private void OnLevelSelectScreenBackPressed()
    {
        _mainMenuContainer.Visible = true;
        _levelSelectScreen.Visible = false;
    }

    private void OnPlayButtonPressed()
    {
        _mainMenuContainer.Visible = false;
        _levelSelectScreen.Visible = true;
    }

    private void OnQuitButtonPressed() => GetTree().Quit();
}
