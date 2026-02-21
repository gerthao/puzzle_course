using Godot;
using PuzzleCourse.Game.Manager;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.UI;

public partial class GameUI : CanvasLayer
{
    [Signal]
    public delegate void BuildingResourceSelectedEventHandler(BuildingResource resource);

    [Export]
    private BuildingManager _buildingManager;

    [Export]
    private BuildingResource[] _buildingResources;

    [Export]
    private PackedScene _buildingSectionScene;

    private VBoxContainer _hBoxContainer;

    private Label _resourceLabel;

    public override void _Ready()
    {
        _hBoxContainer                                 =  GetNode<VBoxContainer>("%BuildingSectionContainer");
        _resourceLabel                                 =  GetNode<Label>("%ResourceLabel");
        _buildingManager.AvailableResourceCountUpdated += OnAvailableResourceCountChanged;

        InitializeBuildingSections();
    }

    public void HideUI() => Visible = false;

    public void ShowUI() => Visible = true;

    public void ToggleUI() => Visible = !Visible;

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

    private void OnAvailableResourceCountChanged(int newCount) => _resourceLabel.Text = newCount.ToString();
}
