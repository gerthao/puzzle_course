using Godot;

namespace PuzzleCourse.Game;

public partial class GameCamera : Camera2D
{
    private const float CameraMoveSpeed = 600;
    private readonly StringName _actionPanDown = "pan_down";
    private readonly StringName _actionPanLeft = "pan_left";
    private readonly StringName _actionPanRight = "pan_right";
    private readonly StringName _actionPanUp = "pan_up";

    public override void _Process(double delta)
    {
        GlobalPosition = GetScreenCenterPosition();

        var movementVector = Input.GetVector(
            _actionPanLeft,
            _actionPanRight,
            _actionPanUp,
            _actionPanDown
        );

        GlobalPosition += movementVector * CameraMoveSpeed * (float)delta;
    }

    public void CenterOn(Vector2 position) => GlobalPosition = position;

    public void SetBoundingRect(Rect2I boundingRect)
    {
        LimitLeft   = boundingRect.Position.X * Grid.CellPixelSize;
        LimitRight  = boundingRect.End.X * Grid.CellPixelSize;
        LimitTop    = boundingRect.Position.Y * Grid.CellPixelSize;
        LimitBottom = boundingRect.End.Y * Grid.CellPixelSize;
    }
}
