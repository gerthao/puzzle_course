using Godot;

namespace PuzzleCourse.Game.Autoload;

public partial class LevelManager : Node
{
    private int _currentLevelIndex;

    [Export]
    private PackedScene[] _levelScenes;

    public static LevelManager Instance { get; private set; }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated) Instance = this;
    }

    public void ChangeToLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _levelScenes.Length)
        {
            GD.PushWarning($"Level index {levelIndex} is out of bounds.");
            return;
        }

        _currentLevelIndex = levelIndex;

        GetTree().ChangeSceneToPacked(_levelScenes[_currentLevelIndex]);
    }

    public void ChangeToNextLevel() => ChangeToLevel(_currentLevelIndex + 1);

    public void RestartLevel() => ChangeToLevel(_currentLevelIndex);
}
