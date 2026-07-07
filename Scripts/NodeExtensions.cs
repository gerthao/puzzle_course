using Godot;

namespace PuzzleCourse.Scripts;

public static class NodeExtensions
{
    public static T? FindFirstNode<T>(this Node node) where T : Node
    {
        var children = node.GetChildren();
        foreach (var child in children)
            if (child is T result)
                return result;

        return null;
    }
}
