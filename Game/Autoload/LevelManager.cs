using System.Linq;
using Godot;
using PuzzleCourse.Resources.Level;

namespace PuzzleCourse.Game.Autoload;

public partial class LevelManager : Node
{
    private int _currentLevelIndex;

    [Export]
    private LevelDefinitionResource[] _levelDefinitions;

    public static LevelManager Instance { get; private set; }

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated) Instance = this;
    }

    public void ChangeToLevel(int levelIndex)
    {
        if (_levelDefinitions.Length == 0)
        {
            GD.PushWarning("No level definitions have been set.");
            return;
        }

        if (levelIndex < 0 || levelIndex >= _levelDefinitions.Length)
        {
            GD.PushWarning($"Level index {levelIndex} is out of bounds.");
            return;
        }

        _currentLevelIndex = levelIndex;

        GetTree().ChangeSceneToFile(_levelDefinitions[_currentLevelIndex].LevelScenePath);
    }

    public void ChangeToNextLevel() => ChangeToLevel(_currentLevelIndex + 1);

    public static LevelDefinitionResource[] GetLevelDefinitions() => Instance._levelDefinitions.ToArray();

    public void RestartLevel() => ChangeToLevel(_currentLevelIndex);
}
