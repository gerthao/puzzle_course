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
    public delegate void GridStateUpdatedEventHandler();

    [Signal]
    public delegate void ResourceTilesUpdatedEventHandler(int collectedTiles);

    private const string IsBuildable = "is_buildable";
    private const string IsWood = "is_wood";
    private const string IsIgnored = "is_ignored";

    private readonly HashSet<Vector2I> _collectedResourceTiles = [];

    private readonly HashSet<Vector2I> _occupiedTiles = [];

    private readonly HashSet<Vector2I> _validBuildableTiles = [];

    private List<TileMapLayer> _allTileMapLayers = [];

    [Export]
    private TileMapLayer _baseTerrainTileMapLayer;

    [Export]
    private TileMapLayer _highlightTileMapLayer;

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced    += OnBuildingPlaced;
        GameEvents.Instance.BuildingDestroyed += OnBuildingDestroyed;

        _allTileMapLayers = GetAllTileMapLayers(_baseTerrainTileMapLayer);
    }

    public void ClearHighlightTileMapLayer() => _highlightTileMapLayer.Clear();

    public Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
    {
        var (x, y) = (worldPosition / Grid.CellPixelSize).Floor();

        return new Vector2I((int)x, (int)y);
    }

    public Vector2I GetMouseGridCellPosition()
    {
        var mousePosition = _highlightTileMapLayer.GetGlobalMousePosition();

        return ConvertWorldPositionToTilePosition(mousePosition);
    }


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
            .Except(_occupiedTiles);
        var atlasCoord = new Vector2I(1, 0);

        // HighlightBuildRadiusOfOccupiedTiles();

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
            var customData = layer.GetCellTileData(tilePosition);

            if (customData == null || (bool)customData.GetCustomData(IsIgnored)) continue;

            return (bool)customData.GetCustomData(customDataName);
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

    private void OnBuildingDestroyed(BuildingComponent component)
    {
        RecalculateGrid(component);
    }

    private void OnBuildingPlaced(BuildingComponent component)
    {
        UpdateValidBuildableTiles(component);
        UpdateCollectedResourceTiles(component);
    }

    private void RecalculateGrid(BuildingComponent excludedComponent)
    {
        _occupiedTiles.Clear();
        _validBuildableTiles.Clear();
        _collectedResourceTiles.Clear();

        var buildingComponents =
            GetTree()
                .GetNodesInGroup(nameof(BuildingComponent))
                .Cast<BuildingComponent>()
                .Where(bc => bc != excludedComponent);

        foreach (var component in buildingComponents)
        {
            UpdateValidBuildableTiles(component);
            UpdateCollectedResourceTiles(component);
        }

        EmitSignalResourceTilesUpdated(_collectedResourceTiles.Count);
        EmitSignalGridStateUpdated();
    }

    private void UpdateCollectedResourceTiles(BuildingComponent component)
    {
        var rootCell = component.GetGridCellPosition();
        var radius   = component.BuildingResource.ResourceRadius;
        var tiles    = GetResourceTileRegion(rootCell, radius).ToHashSet();

        var previousCount = _collectedResourceTiles.Count;
        _collectedResourceTiles.UnionWith(tiles);
        var newCount = _collectedResourceTiles.Count;

        if (previousCount != newCount) EmitSignalResourceTilesUpdated(_collectedResourceTiles.Count);

        EmitSignalGridStateUpdated();
    }

    private void UpdateValidBuildableTiles(BuildingComponent component)
    {
        _occupiedTiles.Add(component.GetGridCellPosition());

        var rootCell = component.GetGridCellPosition();
        var radius   = component.BuildingResource.BuildableRadius;
        var tiles    = GetBuildableTileRegion(rootCell, radius).ToHashSet();

        _validBuildableTiles.UnionWith(tiles);
        _validBuildableTiles.ExceptWith(_occupiedTiles);

        EmitSignalGridStateUpdated();
    }
}
