using Godot;
using PuzzleCourse.Game.Component;

namespace PuzzleCourse.Game.Autoload;

public partial class GameEvents : Node
{
    [Signal]
    public delegate void BuildingDestroyedEventHandler(BuildingComponent component);

    [Signal]
    public delegate void BuildingPlacedEventHandler(BuildingComponent component);

    public static GameEvents Instance { get; private set; }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated) Instance = this;
    }

    public static void EmitBuildingDestroyed(BuildingComponent component) =>
        Instance.EmitSignalBuildingDestroyed(component);

    public static void EmitBuildingPlaced(BuildingComponent component) =>
        Instance.EmitSignalBuildingPlaced(component);
}
