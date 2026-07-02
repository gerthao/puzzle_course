using Godot;
using PuzzleCourse.Resources.Level;

namespace PuzzleCourse.Game.UI;

public partial class LevelSelectSection : PanelContainer
{
    [Signal]
    public delegate void LevelSelectedEventHandler(int levelIndex);

    private Button _button;
    private int _levelIndex;
    private Label _levelNumberLabel;
    private Label _resourceCountLabel;

    public override void _Ready()
    {
        _button = GetNode<Button>("%Button");
        _levelNumberLabel = GetNode<Label>("%LevelNumberLabel");
        _resourceCountLabel = GetNode<Label>("%ResourceCountLabel");

        _button.Pressed += OnButtonPressed;
    }

    public void SetLevelDefinition(LevelDefinitionResource levelDefinition)
    {
        _resourceCountLabel.Text = levelDefinition.StartingResourceCount.ToString();
    }

    public void SetLevelIndex(int levelIndex)
    {
        _levelIndex = levelIndex;
        _levelNumberLabel.Text = $"Level {levelIndex + 1}";
    }

    private void OnButtonPressed() => EmitSignalLevelSelected(_levelIndex);
}
