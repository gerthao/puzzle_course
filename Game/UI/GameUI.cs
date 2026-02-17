using Godot;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.UI;

public partial class GameUI : CanvasLayer
{
    [Signal]
    public delegate void BuildingResourceSelectedEventHandler(BuildingResource resource);

    [Export]
    private BuildingResource[] _buildingResources;

    [Export]
    private PackedScene _buildingSectionScene;

    private VBoxContainer _hBoxContainer;

    public override void _Ready()
    {
        _hBoxContainer = GetNode<VBoxContainer>("%BuildingSectionContainer");

        InitializeBuildingSections();
    }

    private void InitializeBuildingSections()
    {
        foreach (var resource in _buildingResources)
        {
            var section = _buildingSectionScene.Instantiate<BuildingSection>();
            _hBoxContainer.AddChild(section);

            section.SetBuildingResource(resource);
            section.SelectButtonPressed += () => EmitSignalBuildingResourceSelected(resource);
        }
    }
}
