using System.Diagnostics;
using Godot;
using PuzzleCourse.Game.Autoload;

namespace PuzzleCourse.Game.UI;

public partial class LevelSelectScreen : MarginContainer
{
    [Signal]
    public delegate void BackPressedEventHandler();

    private Button _backButton = null!;
    private GridContainer _gridContainer = null!;

    [Export]
    private PackedScene _levelSelectSectionScene = null!;

    public override void _Ready()
    {
        Debug.Assert(_levelSelectSectionScene != null,
            "LevelSelectSection export variable not set in LevelSelectScreen.tscn");

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
