using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using PuzzleCourse.Game.Autoload;
using PuzzleCourse.Game.Component;
using PuzzleCourse.Game.Level.Utility;
using PuzzleCourse.Scripts;

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

    private readonly HashSet<Vector2I> _allTilesInBuildingRadius = [];

    private readonly HashSet<Vector2I> _collectedResourceTiles = [];

    private readonly HashSet<Vector2I> _occupiedTiles = [];

    private readonly HashSet<Vector2I> _validBuildableTiles = [];

    private List<TileMapLayer> _allTileMapLayers = [];

    [Export]
    private TileMapLayer _baseTerrainTileMapLayer;

    [Export]
    private TileMapLayer _highlightTileMapLayer;

    private Dictionary<TileMapLayer, ElevationLayer> _tileMapLayerToElevationLayer;

    public override void _ExitTree()
    {
        GameEvents.Instance.BuildingPlaced    -= OnBuildingPlaced;
        GameEvents.Instance.BuildingDestroyed -= OnBuildingDestroyed;
    }

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced    += OnBuildingPlaced;
        GameEvents.Instance.BuildingDestroyed += OnBuildingDestroyed;

        /*
         * Alternate way of connecting signals with automatic disconnection
         */

        // GameEvents.Instance.Connect(GameEvents.SignalName.BuildingPlaced,
        //     Callable.From<BuildingComponent>(OnBuildingPlaced));
        // GameEvents.Instance.Connect(GameEvents.SignalName.BuildingDestroyed,
        //     Callable.From<BuildingComponent>(OnBuildingDestroyed));

        _allTileMapLayers             = GetAllTileMapLayers(_baseTerrainTileMapLayer).ToList();
        _tileMapLayerToElevationLayer = BuildTileMapLayerToElevationLayer(_allTileMapLayers);
    }

    public void ClearHighlightTileMapLayer() => _highlightTileMapLayer.Clear();

    public static Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
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

    public void HighlightPotentialTilesForBuildings(Rect2I tileArea, int radius)
    {
        var validTiles = GetValidTilesInRadius(tileArea, radius).ToHashSet();
        var expandedTiles = validTiles
            .Except(_validBuildableTiles)
            .Except(_occupiedTiles);
        var atlasCoord = new Vector2I(1, 0);

        foreach (var t in expandedTiles)
            _highlightTileMapLayer.SetCell(t, 0, atlasCoord);
    }

    public void HighlightResourceTiles(Rect2I tileArea, int radius)
    {
        var resourceTiles = GetResourceTilesInRadius(tileArea, radius);
        var atlasCoord    = new Vector2I(1, 0);

        foreach (var t in resourceTiles)
            _highlightTileMapLayer.SetCell(t, 0, atlasCoord);
    }

    public bool IsTileAreaBuildable(Rect2I tileArea)
    {
        if (tileArea.Size.X == 0 || tileArea.Size.Y == 0)
            return false;

        var (firstLayer, _) = GetTileMapLayerAndCustomData(tileArea.Position, IsBuildable);

        if (!_tileMapLayerToElevationLayer.TryGetValue(firstLayer, out var targetElevationLayer))
            return false;

        foreach (var tilePosition in tileArea.ToTiles())
        {
            var (currentLayer, isBuildable) = GetTileMapLayerAndCustomData(tilePosition, IsBuildable);

            if (!isBuildable.HasValue || !isBuildable.Value.AsBool())
                return false;

            if (!IsWithinValidBuildArea(tilePosition))
                return false;

            if (!_tileMapLayerToElevationLayer.TryGetValue(currentLayer, out var currentElevationLayer))
                return false;

            if (targetElevationLayer != currentElevationLayer)
                return false;
        }

        return true;
    }

    public bool IsTilePositionInAnyBuildingRadius(Vector2I tilePosition) =>
        _allTilesInBuildingRadius.Contains(tilePosition);

    public bool IsWithinValidBuildArea(Vector2I tilePosition) => _validBuildableTiles.Contains(tilePosition);

    private Dictionary<TileMapLayer, ElevationLayer> BuildTileMapLayerToElevationLayer(IEnumerable<TileMapLayer> layers)
    {
        var map = new Dictionary<TileMapLayer, ElevationLayer>();

        foreach (var layer in layers)
        {
            ElevationLayer elevationLayer = null;

            for (Node current = layer; current != null; current = current.GetParent())
                if (current is ElevationLayer foundLayer)
                {
                    elevationLayer = foundLayer;
                    break;
                }

            // all tile map layers with a null elevation layer will be treated as being on the same terrain.
            map[layer] = elevationLayer;
        }

        return map;
    }

    private Variant? FindTileCustomData(Vector2I tilePosition, string customDataName)
    {
        foreach (var layer in _allTileMapLayers)
        {
            var customData = layer.GetCellTileData(tilePosition);

            if (customData == null || customData.GetCustomData(IsIgnored).AsBool()) continue;

            return customData.GetCustomData(customDataName);
        }

        return null;
    }

    private static IEnumerable<TileMapLayer> GetAllTileMapLayers(Node2D rootNode)
    {
        var children = rootNode.GetChildren().OfType<Node2D>().Reverse();

        foreach (var child in children)
        foreach (var layer in GetAllTileMapLayers(child))
            yield return layer;

        if (rootNode is TileMapLayer currentLayer) yield return currentLayer;
    }

    private static IEnumerable<Vector2I> GetCircleRegion(Rect2I tileArea, int radius)
    {
        var tileAreaF      = tileArea.ToRect2();
        var tileAreaCenter = tileAreaF.GetCenter();
        var radiusMod      = Mathf.Max(tileAreaF.Size.X, tileAreaF.Size.Y) / 2;

        foreach (var tilePosition in tileArea.ToTilesInRadius(radius))
            if (IsTileInsideCircle(tileAreaCenter, tilePosition, radius + radiusMod))
                yield return tilePosition;
    }

    private IEnumerable<Vector2I> GetResourceTilesInRadius(Rect2I tileArea, int radius)
    {
        foreach (var t in GetCircleRegion(tileArea, radius))
            if (HasTileBoolCustomData(t, IsWood))
                yield return t;
    }

    private static IEnumerable<Vector2I> GetSquareRegion(Rect2I tileArea, int radius)
    {
        for (var x = tileArea.Position.X - radius; x <= tileArea.End.X + radius; x++)
        for (var y = tileArea.Position.Y - radius; y <= tileArea.End.Y + radius; y++)
            yield return new Vector2I(x, y);
    }

    private (TileMapLayer, Variant?) GetTileMapLayerAndCustomData(Vector2I tilePosition, string customDataName)
    {
        foreach (var layer in _allTileMapLayers)
        {
            var customData = layer.GetCellTileData(tilePosition);

            if (customData == null || customData.GetCustomData(IsIgnored).AsBool()) continue;

            return (layer, customData.GetCustomData(customDataName));
        }

        return (null, null);
    }

    private IEnumerable<Vector2I> GetTilesInRadius(Rect2I tileArea, int radius, Func<Vector2I, bool> filter)
    {
        foreach (var t in GetCircleRegion(tileArea, radius))
            if (filter(t))
                yield return t;
    }

    private IEnumerable<Vector2I> GetTilesInRadius(Rect2I tileArea, int radius)
    {
        foreach (var t in GetCircleRegion(tileArea, radius))
            yield return t;
    }

    private IEnumerable<Vector2I> GetValidTilesInRadius(Rect2I tileArea, int radius)
    {
        foreach (var t in GetCircleRegion(tileArea, radius))
            if (HasTileBoolCustomData(t, IsBuildable))
                yield return t;
    }

    private bool HasTileBoolCustomData(Vector2I tilePosition, string customDataName)
    {
        var customProperty = FindTileCustomData(tilePosition, customDataName);

        return customProperty.HasValue && customProperty.Value.AsBool();
    }

    private static bool IsTileInsideCircle(Vector2 centerPosition, Vector2 tilePosition, float radius)
    {
        const float center = 0.5f;

        var distanceX       = centerPosition.X - (tilePosition.X + center);
        var distanceY       = centerPosition.Y - (tilePosition.Y + center);
        var distanceSquared = distanceX * distanceX + distanceY * distanceY;

        return distanceSquared <= radius * radius;
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
        _allTilesInBuildingRadius.Clear();

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
        var tileArea = new Rect2I(rootCell, component.BuildingResource.Dimensions);
        var radius   = component.BuildingResource.ResourceRadius;
        var tiles    = GetResourceTilesInRadius(tileArea, radius).ToHashSet();

        var previousCount = _collectedResourceTiles.Count;
        _collectedResourceTiles.UnionWith(tiles);
        var newCount = _collectedResourceTiles.Count;

        if (previousCount != newCount) EmitSignalResourceTilesUpdated(_collectedResourceTiles.Count);

        EmitSignalGridStateUpdated();
    }

    private void UpdateValidBuildableTiles(BuildingComponent component)
    {
        _occupiedTiles.UnionWith(component.GetOccupiedCellPositions());

        var radius = component.BuildingResource.BuildableRadius;

        var rootCell = component.GetGridCellPosition();
        var tileArea = new Rect2I(rootCell, component.BuildingResource.Dimensions);

        var allTiles = GetTilesInRadius(tileArea, radius);
        _allTilesInBuildingRadius.UnionWith(allTiles);

        var tiles = GetValidTilesInRadius(tileArea, radius).ToHashSet();

        _validBuildableTiles.UnionWith(tiles);
        _validBuildableTiles.ExceptWith(_occupiedTiles);

        EmitSignalGridStateUpdated();
    }
}
