using System.Diagnostics;
using Godot;
using PuzzleCourse.Game.Manager;
using PuzzleCourse.Game.UI;
using PuzzleCourse.Resources.Level;

namespace PuzzleCourse.Game.Level;

public partial class BaseLevel : Node
{
    private Node2D _baseBuilding = null!;
    private TileMapLayer _baseTerrainTileMapLayer = null!;
    private BuildingManager _buildingManager = null!;
    private GameCamera _gameCamera = null!;
    private GameUI _gameUI = null!;
    private GoldMine _goldMine = null!;
    private GridManager _gridManager = null!;

    [Export]
    private PackedScene _levelCompleteScreen = null!;

    [Export]
    private LevelDefinitionResource _levelDefinitionResource = null!;

    public override void _Ready()
    {
        Debug.Assert(_levelCompleteScreen != null, "LevelCompleteScreen export variable not set in BaseLevel.tscn");
        Debug.Assert(_levelDefinitionResource != null,
            "LevelDefinitionResource export variable not set in BaseLevel.tscn");

        _baseTerrainTileMapLayer = GetNode<TileMapLayer>("%BaseTerrainTileMapLayer");
        _gameCamera = GetNode<GameCamera>("GameCamera");
        _gridManager = GetNode<GridManager>("GridManager");
        _goldMine = GetNode<GoldMine>("%GoldMine");
        _baseBuilding = GetNode<Node2D>("%Base");
        _gameUI = GetNode<GameUI>("GameUI");
        _buildingManager = GetNode<BuildingManager>("BuildingManager");

        _buildingManager.SetStartingResourceCount(_levelDefinitionResource.StartingResourceCount);

        _gameCamera.SetBoundingRect(_baseTerrainTileMapLayer.GetUsedRect());
        _gameCamera.CenterOn(_baseBuilding.GlobalPosition);

        _gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private bool IsGoldMineInReach()
    {
        var goldMineTilePosition = GridManager.ConvertWorldPositionToTilePosition(_goldMine.GlobalPosition);

        return _gridManager.IsTilePositionInAnyBuildingRadius(goldMineTilePosition);
    }

    private void OnGridStateUpdated()
    {
        if (!IsGoldMineInReach()) return;

        _goldMine.SetActive();

        ShowLevelCompleteScreen();

        _gameUI.HideUI();
    }

    private void ShowLevelCompleteScreen()
    {
        var scene = _levelCompleteScreen.Instantiate<LevelCompleteScreen>();
        AddChild(scene);
    }
}
