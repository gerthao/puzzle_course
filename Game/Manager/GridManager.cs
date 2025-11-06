using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using PuzzleCourse.Game.Autoload;
using PuzzleCourse.Game.Component;

namespace PuzzleCourse.Game.Manager;

[GlobalClass]
public partial class GridManager : Node
{
    [Signal]
    public delegate void ResourceTilesUpdatedEventHandler(int collectedTiles);

    private const string IsBuildable = "is_buildable";
    private const string IsWood = "is_wood";

    private readonly HashSet<Vector2I> _collectedResourceTiles = [];

    private readonly HashSet<Vector2I> _validBuildableTiles = [];

    private List<TileMapLayer> _allTileMapLayers = [];

    [Export]
    private TileMapLayer _baseTerrainTileMapLayer;

    [Export]
    private TileMapLayer _highlightTileMapLayer;

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;

        _allTileMapLayers = GetAllTileMapLayers(_baseTerrainTileMapLayer);
    }

    public void ClearHighlightTileMapLayer() => _highlightTileMapLayer.Clear();

    public Vector2I GetMouseGridCellPosition()
    {
        var (x, y) = (_highlightTileMapLayer.GetGlobalMousePosition() / Grid.CellPixelSize).Floor();

        return new Vector2I((int)x, (int)y);
    }

    // public void HighlightPotentialTilesForResource(Vector2I tilePosition, int radius)
    // {
    //     var validTiles = GetBuildableTileRegion(tilePosition, radius).ToHashSet();
    //     var expandedTiles = validTiles
    //         .Except(_validBuildableTiles)
    //         .Except(GetOccupiedTiles());
    //     var atlasCoord = new Vector2I(1, 0);
    //
    //     HighlightBuildRadiusOfOccupiedTiles();
    //
    //     foreach (var t in expandedTiles)
    //         _highlightTileMapLayer.SetCell(t, 0, atlasCoord);
    // }


    public void HighlightBuildRadiusOfOccupiedTiles()
    {
        foreach (var t in _validBuildableTiles)
            _highlightTileMapLayer.SetCell(t, 0, Vector2I.Zero);
    }

    public void HighlightPotentialTilesForBuildings(Vector2I tilePosition, int radius)
    {
        var validTiles = GetBuildableTileRegion(tilePosition, radius).ToHashSet();
        var expandedTiles = validTiles
            .Except(_validBuildableTiles)
            .Except(GetOccupiedTiles());
        var atlasCoord = new Vector2I(1, 0);

        HighlightBuildRadiusOfOccupiedTiles();

        foreach (var t in expandedTiles)
            _highlightTileMapLayer.SetCell(t, 0, atlasCoord);
    }

    public void HighlightResourceTiles(Vector2I tilePosition, int radius)
    {
        var resourceTiles = GetResourceTileRegion(tilePosition, radius);
        var atlasCoord    = new Vector2I(1, 0);

        foreach (var t in resourceTiles)
            _highlightTileMapLayer.SetCell(t, 0, atlasCoord);
    }

    public bool IsWithinValidBuildArea(Vector2I tilePosition) => _validBuildableTiles.Contains(tilePosition);

    private Variant? FindTileCustomData(Vector2I tilePosition, string customDataName)
    {
        foreach (var layer in _allTileMapLayers)
        {
            var maybeCustomData = layer
                ?.GetCellTileData(tilePosition)
                ?.GetCustomData(customDataName);

            if (maybeCustomData.HasValue) return maybeCustomData.Value;
        }

        return null;
    }

    private List<TileMapLayer> GetAllTileMapLayers(TileMapLayer layer, List<TileMapLayer> accumulator = null)
    {
        accumulator ??= [];

        var childLayers = layer.GetChildren()
            .OfType<TileMapLayer>()
            .Reverse();

        foreach (var l in childLayers)
            GetAllTileMapLayers(l, accumulator);

        accumulator.Add(layer);

        return accumulator;
    }

    private IEnumerable<Vector2I> GetBuildableTileRegion(Vector2I center, int radius) =>
        GetTilesInRadius(center, radius, tp => HasTileCustomData(tp, IsBuildable));

    private IEnumerable<Vector2I> GetOccupiedTiles()
    {
        foreach (var node in GetTree().GetNodesInGroup(nameof(BuildingComponent)))
        {
            var bc = (BuildingComponent)node;

            yield return bc.GetGridCellPosition();
        }
    }

    private IEnumerable<Vector2I> GetResourceTileRegion(Vector2I center, int radius) =>
        GetTilesInRadius(center, radius, tp => HasTileCustomData(tp, IsWood));

    private static IEnumerable<Vector2I> GetSquareRegion(Vector2I center, int radius)
    {
        for (var x = center.X - radius; x <= center.X + radius; x++)
        for (var y = center.Y - radius; y <= center.Y + radius; y++)
            yield return new Vector2I(x, y);
    }

    private static IEnumerable<Vector2I> GetTilesInRadius(Vector2I center, int radius, Predicate<Vector2I> predicate)
    {
        var result = new List<Vector2I>();

        foreach (var t in GetSquareRegion(center, radius))
        {
            if (!predicate(t)) continue;

            result.Add(t);
        }

        return result;
    }

    private bool HasTileCustomData(Vector2I tilePosition, string customDataName)
    {
        var customProperty = FindTileCustomData(tilePosition, customDataName);

        return customProperty.HasValue && customProperty.Value.AsBool();
    }

    private void OnBuildingPlaced(BuildingComponent component)
    {
        UpdateValidBuildableTiles(component);
        UpdateCollectedResourceTiles(component);
    }

    private void UpdateCollectedResourceTiles(BuildingComponent component)
    {
        var rootCell = component.GetGridCellPosition();
        var radius   = component.BuildingResource.ResourceRadius;
        var tiles    = GetResourceTileRegion(rootCell, radius).ToHashSet();

        var previousCount = _collectedResourceTiles.Count;
        _collectedResourceTiles.UnionWith(tiles);
        var newCount = _collectedResourceTiles.Count;

        if (previousCount == newCount) return;

        EmitSignalResourceTilesUpdated(_collectedResourceTiles.Count);
    }

    private void UpdateValidBuildableTiles(BuildingComponent component)
    {
        var rootCell = component.GetGridCellPosition();
        var radius   = component.BuildingResource.BuildableRadius;
        var tiles    = GetBuildableTileRegion(rootCell, radius).ToHashSet();

        _validBuildableTiles.UnionWith(tiles);
        _validBuildableTiles.ExceptWith(GetOccupiedTiles());
    }
}
