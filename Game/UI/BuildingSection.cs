using Godot;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.UI;

public partial class BuildingSection : PanelContainer
{
    [Signal]
    public delegate void SelectButtonPressedEventHandler();

    private Label _costLabel;
    private Label _descriptionLabel;
    private Button _selectButton;
    private Label _titleLabel;

    public override void _Ready()
    {
        _titleLabel       = GetNode<Label>("%TitleLabel");
        _descriptionLabel = GetNode<Label>("%DescriptionLabel");
        _costLabel        = GetNode<Label>("%CostLabel");
        _selectButton     = GetNode<Button>("%Button");

        _selectButton.Pressed += OnSelectButtonPressed;
    }

    public void SetBuildingResource(BuildingResource buildingResource)
    {
        _titleLabel.Text       = buildingResource.DisplayName;
        _descriptionLabel.Text = buildingResource.Description;
        _costLabel.Text        = buildingResource.ResourceCost.ToString();
    }

    private void OnSelectButtonPressed() => EmitSignalSelectButtonPressed();
}
