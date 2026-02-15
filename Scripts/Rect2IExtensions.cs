using System.Collections.Generic;
using Godot;

namespace PuzzleCourse.Scripts;

public static class Rect2IExtensions
{
    public static Rect2 ToRect2(this Rect2I rect2I) => new(rect2I.Position, rect2I.Size);

    public static IEnumerable<Vector2I> ToTiles(this Rect2I rect2I)
    {
        for (var x = rect2I.Position.X; x < rect2I.End.X; x++)
        for (var y = rect2I.Position.Y; y < rect2I.End.Y; y++)
            yield return new Vector2I(x, y);
    }

    public static IEnumerable<Vector2I> ToTilesInRadius(this Rect2I rect2I, int radius)
    {
        for (var x = rect2I.Position.X - radius; x <= rect2I.End.X + radius; x++)
        for (var y = rect2I.Position.Y - radius; y <= rect2I.End.Y + radius; y++)
            yield return new Vector2I(x, y);
    }
}
