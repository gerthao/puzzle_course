using Godot;
using PuzzleCourse.Game.Manager;

namespace PuzzleCourse.Game;

public partial class Main : Node
{
    private GoldMine _goldMine;
    private GridManager _gridManager;

    public override void _Ready()
    {
        _gridManager = GetNode<GridManager>("GridManager");
        _goldMine    = GetNode<GoldMine>("%GoldMine");

        _gridManager.GridStateUpdated += OnGridStateUpdated;
    }

    private void OnGridStateUpdated()
    {
        var goldMineTilePosition = _gridManager.ConvertWorldPositionToTilePosition(_goldMine.GlobalPosition);

        if (_gridManager.IsWithinValidBuildArea(goldMineTilePosition)) _goldMine.SetActive();
    }
}
