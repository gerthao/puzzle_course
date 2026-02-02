using Godot;
using PuzzleCourse.Game.Manager;

namespace PuzzleCourse.Game.Level;

public partial class BaseLevel : Node
{
    private Node2D _baseBuilding;
    private TileMapLayer _baseTerrainTileMapLayer;
    private GameCamera _gameCamera;
    private GoldMine _goldMine;
    private GridManager _gridManager;

    public override void _Ready()
    {
        _baseTerrainTileMapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
        _gameCamera              = GetNode<GameCamera>("GameCamera");
        _gridManager             = GetNode<GridManager>("GridManager");
        _goldMine                = GetNode<GoldMine>("%GoldMine");
        _baseBuilding            = GetNode<Node2D>("%Base");

        _gameCamera.SetBoundingRect(_baseTerrainTileMapLayer.GetUsedRect());
        _gameCamera.CenterOn(_baseBuilding.GlobalPosition);

        _gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private void OnGridStateUpdated()
    {
        var goldMineTilePosition = _gridManager.ConvertWorldPositionToTilePosition(_goldMine.GlobalPosition);

        if (_gridManager.IsWithinValidBuildArea(goldMineTilePosition)) _goldMine.SetActive();
    }
}
