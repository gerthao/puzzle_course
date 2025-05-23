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
        
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
        {
            var v = new Vector2I(x, y);

            if (!HasBuildableProperty(v))
                continue;
            
            _validBuildableTiles.Add(v);
        }
        
        _validBuildableTiles.Remove(component.GetGridCellPosition());
    }

    private bool HasBuildableProperty(Vector2I tilePosition) =>
        (_baseTerrainTileMapLayer
            ?.GetCellTileData(tilePosition)
            ?.GetCustomData("buildable")
            .AsBool()
        ).GetValueOrDefault(false);
    
    public bool IsTilePositionBuildable(Vector2I tilePosition) =>
        _validBuildableTiles.Contains(tilePosition);
    

    public void HighlightBuildableTiles()
    {
        foreach (var tilePosition in _validBuildableTiles) 
            _highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
    }

    public void ClearHighlightTileMapLayer() =>
        _highlightTileMapLayer.Clear();

    public Vector2I GetMouseGridCellPosition()
    {
        var (x, y) = (_highlightTileMapLayer.GetGlobalMousePosition() / Grid.CellPixelSize).Floor();

        return new Vector2I((int)x, (int)y);
    }
}
