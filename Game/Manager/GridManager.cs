using System.Collections.Generic;
using Godot;

namespace PuzzleCourse.Game.Manager;

[GlobalClass]
public partial class GridManager : Node
{
    public const float PixelSize = 64;
    
    [Export] private TileMapLayer _highlightTileMapLayer;
    [Export] private TileMapLayer _baseTerrainTileMapLayer;
    
    private HashSet<Vector2> _occupiedCells;
    
    public override void _Ready()
    {
        _occupiedCells = [];
    }

    public void MarkTileAsOccupied(Vector2 tilePosition)
    {
        _occupiedCells.Add(tilePosition);
    }

    public bool IsTilePositionValid(Vector2 tilePosition)
    {
        return !_occupiedCells.Contains(tilePosition);
    }

    public void HighlightValidTilesInRadius(Vector2 rootCell, int radius)
    {
        _highlightTileMapLayer.Clear();

        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                if (!IsTilePositionValid(new(x, y))) continue;
                
                _highlightTileMapLayer.SetCell(
                    coords: new((int)x, (int)y),
                    sourceId: 0,
                    atlasCoords: Vector2I.Zero
                );
            }
        }
    }

    public void ClearHighlightTileMapLayer()
    {
        _highlightTileMapLayer.Clear();
    }

    public Vector2 GetMouseGridCellPosition => (_highlightTileMapLayer.GetGlobalMousePosition() / PixelSize).Floor();
}
