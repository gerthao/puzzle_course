using System.Collections.Generic;
using System.Linq;
using Godot;
using PuzzleCourse.Game.Component;

namespace PuzzleCourse.Game.Manager;

[GlobalClass]
public partial class GridManager : Node
{
    [Export] private TileMapLayer _highlightTileMapLayer;
    [Export] private TileMapLayer _baseTerrainTileMapLayer;

    private HashSet<Vector2I> _occupiedCells = [];

    public void MarkTileAsOccupied(Vector2I tilePosition) =>
        _occupiedCells.Add(tilePosition);

    private bool IsTileBuildable(Vector2I tilePosition) =>
        (_baseTerrainTileMapLayer
            ?.GetCellTileData(tilePosition)
            ?.GetCustomData("buildable")
            .AsBool()
        ).GetValueOrDefault(false);

    public bool IsTilePositionValid(Vector2I tilePosition) =>
        !_occupiedCells.Contains(tilePosition)
        && IsTileBuildable(tilePosition);

    public void HighlightBuildableTiles()
    {
        _highlightTileMapLayer.Clear();

        var buildingComponents = GetTree()
            .GetNodesInGroup(nameof(BuildingComponent))
            .Cast<BuildingComponent>();

        foreach (var component in buildingComponents)
        {
            HighlightValidTilesInRadius(component.GetGridCellPosition(), component.BuildableRadius);
        }
    }

    public void ClearHighlightTileMapLayer() =>
        _highlightTileMapLayer.Clear();

    public Vector2I GetMouseGridCellPosition()
    {
        var (x, y) = (_highlightTileMapLayer.GetGlobalMousePosition() / Grid.CellPixelSize).Floor();

        return new Vector2I((int)x, (int)y);
    }

    private void HighlightValidTilesInRadius(Vector2I rootCell, int radius)
    {
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
        {
            if (!IsTilePositionValid(new Vector2I(x, y))) continue;

            _highlightTileMapLayer.SetCell(
                coords: new Vector2I(x, y),
                sourceId: 0,
                atlasCoords: Vector2I.Zero
            );
        }
    }
}
