using Godot;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.UI;

public partial class GameUI : MarginContainer
{
    [Signal]
    public delegate void BuildingResourceSelectedEventHandler(BuildingResource resource);

    [Export]
    private BuildingResource[] _buildingResources;

    private HBoxContainer _hBoxContainer;

    public override void _Ready()
    {
        _hBoxContainer = GetNode<HBoxContainer>("HBoxContainer");

        InitializeBuildingButtons();
    }

    private void InitializeBuildingButtons()
    {
        foreach (var resource in _buildingResources)
        {
            var buildingButton = new Button();
            buildingButton.Text = $"Place {resource.DisplayName}";

            buildingButton.Pressed += () => EmitSignalBuildingResourceSelected(resource);

            _hBoxContainer.AddChild(buildingButton);
        }
    }
}
