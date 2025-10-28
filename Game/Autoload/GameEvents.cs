using Godot;
using PuzzleCourse.Game.Component;

namespace PuzzleCourse.Game.Autoload;

public partial class GameEvents : Node
{
    [Signal]
    public delegate void BuildingPlacedEventHandler(BuildingComponent component);

    public static GameEvents Instance { get; private set; }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated) Instance = this;
    }

    public static void EmitBuildingPlaced(BuildingComponent component) =>
        Instance.EmitSignal(SignalName.BuildingPlaced, component);
}
