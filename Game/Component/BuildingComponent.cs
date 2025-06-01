using Godot;
using PuzzleCourse.Game.Autoload;
using PuzzleCourse.Resources.Building;

namespace PuzzleCourse.Game.Component;

public partial class BuildingComponent : Node2D
{

    [Export(PropertyHint.File, "*.tres")] private string _buildingResourcePath;

    public BuildingResource BuildingResource { get; private set; }
    
    public override void _Ready()
    {
        if (_buildingResourcePath != null)
        {
            BuildingResource = GD.Load<BuildingResource>(_buildingResourcePath);
        }
        
        AddToGroup(nameof(BuildingComponent));

        Callable
            .From(() => GameEvents.EmitBuildingPlaced(this))
            .CallDeferred();
    }

    public Vector2I GetGridCellPosition()
    {
        var (x, y) = (GlobalPosition / Grid.CellPixelSize).Floor();

        return new Vector2I((int)x, (int)y);
    }
}
