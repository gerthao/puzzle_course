using System;
using System.Collections.Generic;
using Godot;

namespace PuzzleCourse.Scripts;

public static class NodeExtensions
{
    extension(Node node)
    {
        public List<T> FindAllNodesOfType<T>() where T : Node
        {
            var children = node.GetChildren();
            var results = new List<T>();
            foreach (var child in children)
                if (child is T result)
                    results.Add(result);

            return results;
        }

        public List<T> FindAllNodesOfType<T>(Func<T, bool> predicate) where T : Node
        {
            var children = node.GetChildren();
            var results = new List<T>();
            foreach (var child in children)
                if (child is T result && predicate(result))
                    results.Add(result);

            return results;
        }

        public T? FindFirstNodeOfType<T>() where T : Node
        {
            var children = node.GetChildren();
            foreach (var child in children)
                if (child is T result)
                    return result;

            return null;
        }

        public T? FindFirstNodeOfType<T>(Func<T, bool> predicate) where T : Node
        {
            var children = node.GetChildren();
            foreach (var child in children)
                if (child is T result && predicate(result))
                    return result;

            return null;
        }
    }
}
