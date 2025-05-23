using System.Collections.Generic;
using System.Linq;
using Godot;
using PuzzleCourse.Game.Autoload;
using PuzzleCourse.Game.Component;

namespace PuzzleCourse.Game.Manager;

[GlobalClass]
public partial class GridManager : Node
{
    [Export] private TileMapLayer _highlightTileMapLayer;
    [Export] private TileMapLayer _baseTerrainTileMapLayer;

    private HashSet<Vector2I> _validBuildableTiles = [];

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += UpdateValidBuildableTiles;
    }

    private void UpdateValidBuildableTiles(BuildingComponent component)
    {
        var rootCell = component.GetGridCellPosition();
        var radius = component.BuildableRadius;
        var validTiles = GetValidTilesInRadius(rootCell, radius);

        _validBuildableTiles.UnionWith(validTiles);
        _validBuildableTiles.ExceptWith(GetOccupiedTiles());
    }

    private IEnumerable<Vector2I> GetOccupiedTiles() =>
        GetTree()
            .GetNodesInGroup(nameof(BuildingComponent))
            .Cast<BuildingComponent>()
            .Select(bc => bc.GetGridCellPosition());


    private bool HasBuildableProperty(Vector2I tilePosition) =>
        (_baseTerrainTileMapLayer
            ?.GetCellTileData(tilePosition)
            ?.GetCustomData("buildable")
            .AsBool()
        ).GetValueOrDefault(false);

    public bool IsTilePositionBuildable(Vector2I tilePosition) =>
        _validBuildableTiles.Contains(tilePosition);

    public void HighlightBuildRadiusOfIntendedTile(Vector2I tilePosition, int radius)
    {
        ClearHighlightTileMapLayer();

        var validTiles = GetValidTilesInRadius(tilePosition, radius).ToHashSet();
        var expandedTiles = validTiles
            .Except(_validBuildableTiles)
            .Except(GetOccupiedTiles());
        var atlasCoord = new Vector2I(1, 0);

        HighlightBuildRadiiOfOccupiedTiles();

        foreach (var t in expandedTiles)
            _highlightTileMapLayer.SetCell(t, 0, atlasCoord);
    }

    public void HighlightBuildRadiiOfOccupiedTiles()
    {
        foreach (var t in _validBuildableTiles)
            _highlightTileMapLayer.SetCell(t, 0, Vector2I.Zero);
    }

    public void ClearHighlightTileMapLayer() =>
        _highlightTileMapLayer.Clear();

    public Vector2I GetMouseGridCellPosition()
    {
        var (x, y) = (_highlightTileMapLayer.GetGlobalMousePosition() / Grid.CellPixelSize).Floor();

        return new Vector2I((int)x, (int)y);
    }

    private List<Vector2I> GetValidTilesInRadius(Vector2I tilePosition, int radius)
    {
        List<Vector2I> result = [];

        for (var x = tilePosition.X - radius; x <= tilePosition.X + radius; x++)
        for (var y = tilePosition.Y - radius; y <= tilePosition.Y + radius; y++)
        {
            var v = new Vector2I(x, y);

            if (!HasBuildableProperty(v))
                continue;

            result.Add(v);
        }

        return result;
    }
}
