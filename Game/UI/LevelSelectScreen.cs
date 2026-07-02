using Godot;
using PuzzleCourse.Game.Autoload;

namespace PuzzleCourse.Game.UI;

public partial class LevelSelectScreen : MarginContainer
{
    [Signal]
    public delegate void BackPressedEventHandler();

    private Button _backButton;
    private GridContainer _gridContainer;

    [Export]
    private PackedScene _levelSelectSectionScene;

    public override void _Ready()
    {
        _gridContainer = GetNode<GridContainer>("%GridContainer");
        _backButton = GetNode<Button>("%BackButton");

        var levelDefinitions = LevelManager.GetLevelDefinitions();

        for (var i = 0; i < levelDefinitions.Length; i++)
        {
            var levelDefinition = levelDefinitions[i];
            var levelSelectSection = _levelSelectSectionScene.Instantiate<LevelSelectSection>();
            _gridContainer.AddChild(levelSelectSection);

            levelSelectSection.SetLevelDefinition(levelDefinition);
            levelSelectSection.SetLevelIndex(i);
            levelSelectSection.LevelSelected += OnLevelSelected;
        }

        _backButton.Pressed += OnBackButtonPressed;
    }

    private void OnBackButtonPressed() => EmitSignalBackPressed();

    private void OnLevelSelected(int levelIndex) => LevelManager.Instance.ChangeToLevel(levelIndex);
}
