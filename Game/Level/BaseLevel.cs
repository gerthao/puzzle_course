using Godot;
using PuzzleCourse.Game.Manager;
using PuzzleCourse.Game.UI;

namespace PuzzleCourse.Game.Level;

public partial class BaseLevel : Node
{
    private Node2D _baseBuilding;
    private TileMapLayer _baseTerrainTileMapLayer;
    private GameCamera _gameCamera;
    private GameUI _gameUI;
    private GoldMine _goldMine;
    private GridManager _gridManager;

    [Export]
    private PackedScene _levelCompleteScreen;

    public override void _Ready()
    {
        _baseTerrainTileMapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
        _gameCamera              = GetNode<GameCamera>("GameCamera");
        _gridManager             = GetNode<GridManager>("GridManager");
        _goldMine                = GetNode<GoldMine>("%GoldMine");
        _baseBuilding            = GetNode<Node2D>("%Base");
        _gameUI                  = GetNode<GameUI>("GameUI");

        _gameCamera.SetBoundingRect(_baseTerrainTileMapLayer.GetUsedRect());
        _gameCamera.CenterOn(_baseBuilding.GlobalPosition);

        _gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private void OnGridStateUpdated()
    {
        var goldMineTilePosition = GridManager.ConvertWorldPositionToTilePosition(_goldMine.GlobalPosition);

        if (!_gridManager.IsWithinValidBuildArea(goldMineTilePosition)) return;

        _goldMine.SetActive();

        var scene = _levelCompleteScreen.Instantiate<LevelCompleteScreen>();
        AddChild(scene);

        _gameUI.HideUI();
    }
}
