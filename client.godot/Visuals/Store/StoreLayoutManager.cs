using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Core.Simulation.Data;
using Core.Simulation.Logic;

namespace Client.Scripts.Visuals.Store
{
    public sealed class StoreLayoutManager
    {
        private const int InitialSimulationShelfCount = 8;
        private const float ObstaclePadding = 10f;
        private const float SegmentStep = 8f;

        private readonly Control _shopView;
        private readonly Control? _entranceZone;
        private readonly Control? _shelfCartridgeZone;
        private readonly Control? _shelfConsoleZone;
        private readonly Control? _shelfCollectorZone;
        private readonly Control? _storageZone;
        private readonly Control? _deliveryZone;
        private readonly Control? _registerZone;
        private readonly Control? _queueLane;

        private Rect2 _shopBounds;
        private Rect2 _entranceRect;
        private Rect2 _shelfCartridgeRect;
        private Rect2 _shelfConsoleRect;
        private Rect2 _shelfCollectorRect;
        private Rect2 _storageRect;
        private Rect2 _deliveryRect;
        private Rect2 _registerRect;
        private Rect2 _queueRect;
        private List<Rect2> _dynamicShelfSlots = new();
        private Vector2 _lastShopSize = Vector2.Inf;
        private string _lastObstacleSignature = "";
        private List<Rect2> _cachedSolidShelfObstacles = new();
        private IReadOnlyList<Vector2>? _cachedStaticNavigationAnchors;

        public StoreLayoutManager(
            Control shopView,
            Control? entranceZone,
            Control? shelfCartridgeZone,
            Control? shelfConsoleZone,
            Control? shelfCollectorZone,
            Control? storageZone,
            Control? deliveryZone,
            Control? registerZone,
            Control? queueLane)
        {
            _shopView = shopView;
            _entranceZone = entranceZone;
            _shelfCartridgeZone = shelfCartridgeZone;
            _shelfConsoleZone = shelfConsoleZone;
            _shelfCollectorZone = shelfCollectorZone;
            _storageZone = storageZone;
            _deliveryZone = deliveryZone;
            _registerZone = registerZone;
            _queueLane = queueLane;
            RefreshLayout();
        }

        public Rect2 ShopBounds => _shopBounds;

        public void InvalidateNavigationCache()
        {
            _lastShopSize = Vector2.Inf;
            _lastObstacleSignature = "";
            _cachedStaticNavigationAnchors = null;
            _cachedSolidShelfObstacles.Clear();
        }

        public void RefreshLayout()
        {
            Vector2 shopSize = _shopView.Size;
            if (shopSize.X < 320f || shopSize.Y < 240f)
                shopSize = new Vector2(960f, 620f);

            if (_lastShopSize.DistanceTo(shopSize) <= 0.5f)
                return;

            _lastShopSize = shopSize;
            _shopBounds = new Rect2(Vector2.Zero, shopSize);
            _entranceRect = ResolveRect(_entranceZone, new Rect2(350f, 543f, 202f, 62f));
            _shelfCartridgeRect = ResolveRect(_shelfCartridgeZone, new Rect2(59f, 200f, 196f, 116f));
            _shelfConsoleRect = ResolveRect(_shelfConsoleZone, new Rect2(264f, 200f, 196f, 116f));
            _shelfCollectorRect = ResolveRect(_shelfCollectorZone, new Rect2(264f, 329f, 196f, 116f));
            _storageRect = ResolveRect(_storageZone, new Rect2(520f, 71f, 206f, 83f));
            _deliveryRect = ResolveRect(_deliveryZone, new Rect2(736f, 71f, 196f, 82f));
            _registerRect = ResolveRect(_registerZone, new Rect2(788f, 351f, 146f, 134f));
            _queueRect = ResolveRect(_queueLane, new Rect2(698f, 488f, 236f, 46f));
            _dynamicShelfSlots = BuildDynamicShelfSlots();
            _cachedStaticNavigationAnchors = null;
            _lastObstacleSignature = "";
        }

        public Control? CreateShelfVisualFromOriginal(ShelfDisplayType displayType)
        {
            Control? template = GetShelfTemplate(displayType);
            return template?.Duplicate() as Control;
        }

        public IReadOnlyList<ShelfPlacement> GetPurchasedShelfPlacements(GameManager game)
        {
            RefreshLayout();

            var purchasedShelves = game.Inventory.Shelves
                .Where(shelf => shelf.Id > InitialSimulationShelfCount)
                .OrderBy(shelf => shelf.Id)
                .ToList();

            var placements = new List<ShelfPlacement>();
            for (int index = 0; index < purchasedShelves.Count && index < _dynamicShelfSlots.Count; index++)
            {
                ShelfStock shelf = purchasedShelves[index];
                Rect2 rect = _dynamicShelfSlots[index];
                placements.Add(new ShelfPlacement(
                    shelf.Id,
                    shelf.ProductId,
                    shelf.DisplayType,
                    rect,
                    false,
                    GetShelfGroup(shelf.ProductId),
                    index,
                    BuildBrowsePoints(rect),
                    BuildRestockPoint(rect)));
            }

            return placements;
        }

        public IReadOnlyList<ShelfPlacement> GetAllShelfPlacements(GameManager? game)
        {
            RefreshLayout();

            var placements = new List<ShelfPlacement>
            {
                CreateOriginalPlacement(_shelfCartridgeRect, ShelfDisplayType.Basic, 1, 0, 0),
                CreateOriginalPlacement(_shelfConsoleRect, ShelfDisplayType.Premium, 2, 1, 1),
                CreateOriginalPlacement(_shelfCollectorRect, ShelfDisplayType.Featured, 3, 2, 2)
            };

            if (game != null)
                placements.AddRange(GetPurchasedShelfPlacements(game));

            return placements;
        }

        public IReadOnlyList<Rect2> GetSolidShelfObstacles(GameManager? game)
        {
            RefreshLayout();
            string signature = BuildObstacleSignature(game);
            if (signature == _lastObstacleSignature)
                return _cachedSolidShelfObstacles;

            _cachedSolidShelfObstacles = GetAllShelfPlacements(game)
                .Select(placement => ExpandRect(placement.Rect, ObstaclePadding))
                .ToList();
            _lastObstacleSignature = signature;
            return _cachedSolidShelfObstacles;
        }

        public Vector2 GetSpawnPoint(int index)
        {
            float offset = (index - 1.5f) * 22f;
            return ClampPoint(new Vector2(_entranceRect.GetCenter().X + offset, _entranceRect.End.Y - 20f));
        }

        public Vector2 GetEntranceWalkPoint(int index)
        {
            float offset = (index - 1.5f) * 10f;
            return ClampPoint(new Vector2(_entranceRect.GetCenter().X + offset, _entranceRect.Position.Y + 18f));
        }

        public Vector2 GetExitPoint(int index)
        {
            float offset = (index - 1.5f) * 18f;
            return ClampPoint(new Vector2(_entranceRect.GetCenter().X + offset, _entranceRect.End.Y - 10f));
        }

        public Vector2 GetBrowsePoint(int productId, int variant, GameManager? game = null)
        {
            var placements = GetCandidateShelfPlacements(productId, game);
            var selected = placements[Math.Abs(variant) % placements.Count];
            return selected.BrowsePoints[Math.Abs(variant) % selected.BrowsePoints.Count];
        }

        public Vector2 GetSecondaryBrowsePoint(int productId, int variant, GameManager? game = null)
        {
            var placements = GetCandidateShelfPlacements(productId, game);
            var selected = placements[Math.Abs(variant + 1) % placements.Count];
            int browseIndex = Math.Abs(variant + 1) % selected.BrowsePoints.Count;
            return selected.BrowsePoints[browseIndex];
        }

        public Vector2 GetQueuePoint(int index)
        {
            int clampedIndex = Math.Clamp(index, 0, 4);
            return ClampPoint(new Vector2(_queueRect.Position.X + 24f + clampedIndex * 30f, _queueRect.GetCenter().Y - 4f));
        }

        public Vector2 GetRegisterServicePoint(int index)
        {
            float offset = Math.Clamp(index, 0, 2) * 18f;
            return ClampPoint(new Vector2(_registerRect.Position.X + 30f + offset, _registerRect.GetCenter().Y + 10f));
        }

        public Vector2 GetCashierAnchorPoint(int index)
        {
            float offset = index % 2 == 0 ? -12f : 12f;
            return ClampPoint(new Vector2(_registerRect.GetCenter().X + offset, _registerRect.End.Y - 34f));
        }

        public Vector2 GetStockerStoragePoint(int index)
        {
            float offset = index % 2 == 0 ? -24f : 24f;
            return ClampPoint(new Vector2(_storageRect.GetCenter().X + offset, _storageRect.End.Y - 12f));
        }

        public Vector2 GetStockerShelfPoint(int productId, int index, GameManager? game = null)
        {
            var placements = GetCandidateShelfPlacements(productId, game);
            var selected = placements[Math.Abs(index) % placements.Count];
            return selected.RestockPoint;
        }

        public Vector2 GetSalesAssociatePoint(int index)
        {
            return (index % 4) switch
            {
                0 => ClampPoint(new Vector2(_shelfCartridgeRect.GetCenter().X, _shelfCartridgeRect.End.Y + 28f)),
                1 => ClampPoint(new Vector2(_shelfConsoleRect.GetCenter().X, _shelfConsoleRect.End.Y + 24f)),
                2 => ClampPoint(new Vector2(_shelfCollectorRect.Position.X - 24f, _shelfCollectorRect.GetCenter().Y)),
                _ => ClampPoint(new Vector2(_shelfConsoleRect.End.X + 56f, _shelfCollectorRect.End.Y + 18f))
            };
        }

        public Vector2 GetSecurityPatrolPoint(int index)
        {
            return (index % 4) switch
            {
                0 => ClampPoint(new Vector2(_deliveryRect.End.X - 18f, _deliveryRect.End.Y + 36f)),
                1 => ClampPoint(new Vector2(_registerRect.End.X - 24f, _registerRect.Position.Y - 22f)),
                2 => ClampPoint(new Vector2(_entranceRect.End.X - 26f, _entranceRect.Position.Y - 28f)),
                _ => ClampPoint(new Vector2(_shelfCartridgeRect.Position.X + 12f, _storageRect.Position.Y + 24f))
            };
        }

        public Vector2 GetManagerPoint(int index)
        {
            return (index % 4) switch
            {
                0 => ClampPoint(new Vector2(_storageRect.Position.X - 34f, _storageRect.GetCenter().Y + 10f)),
                1 => ClampPoint(new Vector2(_shelfConsoleRect.End.X + 44f, _shelfConsoleRect.GetCenter().Y)),
                2 => ClampPoint(new Vector2(_registerRect.Position.X - 40f, _registerRect.GetCenter().Y)),
                _ => ClampPoint(new Vector2(_shelfCollectorRect.GetCenter().X + 32f, _shelfCollectorRect.End.Y + 28f))
            };
        }

        public Rect2 GetStorageFurnitureSlot(int slotIndex)
        {
            return CreateGridSlot(_storageRect, slotIndex, 2, 2, new Vector2(52f, 20f), 54f);
        }

        public Rect2 GetDecorationFurnitureSlot(int slotIndex)
        {
            Vector2[] positions =
            {
                new(_shelfCartridgeRect.Position.X + 8f, _shelfCartridgeRect.Position.Y - 28f),
                new(_shelfConsoleRect.GetCenter().X - 28f, _shelfConsoleRect.Position.Y - 28f),
                new(_deliveryRect.Position.X + 10f, _deliveryRect.Position.Y + 6f),
                new(_registerRect.End.X - 72f, _registerRect.Position.Y - 28f),
                new(_shelfCollectorRect.End.X + 24f, _shelfCollectorRect.GetCenter().Y - 8f),
                new(_entranceRect.Position.X + 10f, _entranceRect.Position.Y - 34f)
            };

            Vector2 position = positions[slotIndex % positions.Length];
            return ClampRect(new Rect2(position, new Vector2(56f, 22f)));
        }

        public Rect2 GetHardwareFurnitureSlot(int catalogItemId, int slotIndex)
        {
            if (catalogItemId == 6)
            {
                Vector2[] registerSlots =
                {
                    new(_registerRect.Position.X + 12f, _registerRect.Position.Y + 10f),
                    new(_registerRect.Position.X + 72f, _registerRect.Position.Y + 10f)
                };
                return ClampRect(new Rect2(registerSlots[slotIndex % registerSlots.Length], new Vector2(50f, 20f)));
            }

            Vector2[] stockCartSlots =
            {
                new(_storageRect.Position.X + 12f, _storageRect.Position.Y + 8f),
                new(_deliveryRect.Position.X + 12f, _deliveryRect.End.Y + 10f),
                new(_storageRect.End.X - 62f, _storageRect.Position.Y + 8f)
            };
            return ClampRect(new Rect2(stockCartSlots[slotIndex % stockCartSlots.Length], new Vector2(56f, 20f)));
        }

        public List<Vector2> BuildPath(Vector2 start, Vector2 target, GameManager? game = null)
        {
            RefreshLayout();

            List<Rect2> obstacles = GetSolidShelfObstacles(game).ToList();
            Vector2 safeStart = FindNearestWalkablePoint(start, obstacles);
            Vector2 safeTarget = FindNearestWalkablePoint(target, obstacles);

            if (HasClearSegment(safeStart, safeTarget, obstacles))
                return new List<Vector2> { safeTarget };

            var nodes = new List<Vector2> { safeStart, safeTarget };
            nodes.AddRange(GetStaticNavigationAnchors());
            foreach (Rect2 obstacle in obstacles)
                nodes.AddRange(GetObstacleWaypoints(obstacle));

            nodes = nodes
                .Select(point => ClampPoint(point))
                .Where(point => IsWalkablePoint(point, obstacles))
                .Distinct(new Vector2ApproxComparer())
                .ToList();

            int startIndex = FindPointIndex(nodes, safeStart);
            int targetIndex = FindPointIndex(nodes, safeTarget);
            if (startIndex < 0 || targetIndex < 0)
                return new List<Vector2> { safeTarget };

            var distances = Enumerable.Repeat(float.PositiveInfinity, nodes.Count).ToArray();
            var previous = Enumerable.Repeat(-1, nodes.Count).ToArray();
            var open = new List<int> { startIndex };
            distances[startIndex] = 0f;

            while (open.Count > 0)
            {
                int current = open
                    .OrderBy(index => distances[index] + nodes[index].DistanceTo(nodes[targetIndex]))
                    .First();
                open.Remove(current);

                if (current == targetIndex)
                    break;

                for (int neighbor = 0; neighbor < nodes.Count; neighbor++)
                {
                    if (neighbor == current)
                        continue;
                    if (!HasClearSegment(nodes[current], nodes[neighbor], obstacles))
                        continue;

                    float tentative = distances[current] + nodes[current].DistanceTo(nodes[neighbor]);
                    if (tentative >= distances[neighbor])
                        continue;

                    distances[neighbor] = tentative;
                    previous[neighbor] = current;
                    if (!open.Contains(neighbor))
                        open.Add(neighbor);
                }
            }

            if (previous[targetIndex] == -1)
                return BuildFallbackPath(safeStart, safeTarget, obstacles);

            var path = new List<Vector2>();
            int cursor = targetIndex;
            while (cursor != -1)
            {
                path.Add(nodes[cursor]);
                cursor = previous[cursor];
            }

            path.Reverse();
            if (path.Count > 0 && path[0].DistanceTo(safeStart) <= 2f)
                path.RemoveAt(0);
            return path.Count == 0 ? new List<Vector2> { safeTarget } : path;
        }

        public Vector2 ClampPoint(Vector2 point)
        {
            const float margin = 6f;
            return new Vector2(
                Math.Clamp(point.X, _shopBounds.Position.X + margin, _shopBounds.End.X - margin),
                Math.Clamp(point.Y, _shopBounds.Position.Y + margin, _shopBounds.End.Y - margin));
        }

        public Vector2 ClampPosition(Vector2 position, Vector2 size)
        {
            const float margin = 4f;
            return new Vector2(
                Math.Clamp(position.X, _shopBounds.Position.X + margin, _shopBounds.End.X - size.X - margin),
                Math.Clamp(position.Y, _shopBounds.Position.Y + margin, _shopBounds.End.Y - size.Y - margin));
        }

        public int GetShelfGroup(int productId)
        {
            return productId switch
            {
                1 or 4 or 7 => 0,
                2 or 5 => 1,
                _ => 2
            };
        }

        private Control? GetShelfTemplate(ShelfDisplayType displayType)
        {
            if (displayType == ShelfDisplayType.Premium)
                return _shelfConsoleZone;
            if (displayType == ShelfDisplayType.Featured)
                return _shelfCollectorZone;

            return _shelfCartridgeZone;
        }

        private ShelfPlacement CreateOriginalPlacement(Rect2 rect, ShelfDisplayType type, int productId, int group, int variantIndex)
        {
            return new ShelfPlacement(
                variantIndex + 1,
                productId,
                type,
                rect,
                true,
                group,
                variantIndex,
                BuildBrowsePoints(rect),
                BuildRestockPoint(rect));
        }

        private List<ShelfPlacement> GetCandidateShelfPlacements(int productId, GameManager? game)
        {
            int group = GetShelfGroup(productId);
            var placements = GetAllShelfPlacements(game)
                .Where(placement => placement.Group == group)
                .ToList();

            if (placements.Count == 0)
            {
                placements.Add(group switch
                {
                    0 => CreateOriginalPlacement(_shelfCartridgeRect, ShelfDisplayType.Basic, 1, 0, 0),
                    1 => CreateOriginalPlacement(_shelfConsoleRect, ShelfDisplayType.Premium, 2, 1, 1),
                    _ => CreateOriginalPlacement(_shelfCollectorRect, ShelfDisplayType.Featured, 3, 2, 2)
                });
            }

            return placements;
        }

        private List<Rect2> BuildDynamicShelfSlots()
        {
            Vector2 shelfSize = _shelfCartridgeRect.Size;
            if (shelfSize.X < 40f || shelfSize.Y < 40f)
                shelfSize = new Vector2(196f, 116f);

            return new List<Rect2>
            {
                ClampRect(new Rect2(new Vector2(58f, 70f), shelfSize)),
                ClampRect(new Rect2(new Vector2(264f, 70f), shelfSize)),
                ClampRect(new Rect2(new Vector2(520f, 200f), shelfSize)),
                ClampRect(new Rect2(new Vector2(58f, 329f), shelfSize)),
                ClampRect(new Rect2(new Vector2(520f, 329f), shelfSize))
            };
        }

        private IReadOnlyList<Vector2> BuildBrowsePoints(Rect2 rect)
        {
            var candidates = new List<Vector2>
            {
                new(rect.GetCenter().X, rect.End.Y + 22f),
                new(rect.Position.X - 24f, rect.GetCenter().Y),
                new(rect.End.X + 24f, rect.GetCenter().Y),
                new(rect.GetCenter().X, rect.Position.Y - 18f)
            };

            List<Rect2> staticObstacles = GetOriginalObstacleRects();
            var safePoints = candidates
                .Select(ClampPoint)
                .Where(point => IsWalkablePoint(point, staticObstacles))
                .ToList();

            return safePoints.Count > 0 ? safePoints : new[] { ClampPoint(new Vector2(rect.Position.X - 24f, rect.GetCenter().Y)) };
        }

        private Vector2 BuildRestockPoint(Rect2 rect)
        {
            Vector2 candidate = new(rect.End.X + 22f, rect.GetCenter().Y + 12f);
            List<Rect2> staticObstacles = GetOriginalObstacleRects();
            if (!IsWalkablePoint(candidate, staticObstacles))
                candidate = new Vector2(rect.Position.X - 24f, rect.GetCenter().Y + 12f);
            return ClampPoint(candidate);
        }

        private List<Rect2> GetOriginalObstacleRects()
        {
            return new List<Rect2>
            {
                ExpandRect(_shelfCartridgeRect, ObstaclePadding),
                ExpandRect(_shelfConsoleRect, ObstaclePadding),
                ExpandRect(_shelfCollectorRect, ObstaclePadding)
            };
        }

        private IReadOnlyList<Vector2> GetStaticNavigationAnchors()
        {
            if (_cachedStaticNavigationAnchors != null)
                return _cachedStaticNavigationAnchors;

            _cachedStaticNavigationAnchors = new[]
            {
                ClampPoint(new Vector2(_entranceRect.GetCenter().X, _entranceRect.Position.Y - 18f)),
                ClampPoint(new Vector2(_shelfCartridgeRect.Position.X + 36f, _shelfCartridgeRect.Position.Y - 24f)),
                ClampPoint(new Vector2(_shelfConsoleRect.GetCenter().X, _shelfConsoleRect.Position.Y - 24f)),
                ClampPoint(new Vector2(_shelfConsoleRect.End.X + 34f, _shelfConsoleRect.GetCenter().Y)),
                ClampPoint(new Vector2(_shelfCartridgeRect.Position.X + 34f, _shelfCartridgeRect.End.Y + 28f)),
                ClampPoint(new Vector2(_shelfConsoleRect.GetCenter().X, _shelfCollectorRect.End.Y + 28f)),
                ClampPoint(new Vector2(_storageRect.Position.X - 26f, _storageRect.GetCenter().Y)),
                ClampPoint(new Vector2(_deliveryRect.Position.X - 22f, _deliveryRect.End.Y + 26f)),
                ClampPoint(new Vector2(_registerRect.Position.X - 26f, _registerRect.GetCenter().Y)),
                ClampPoint(new Vector2(_queueRect.Position.X - 18f, _queueRect.GetCenter().Y)),
                ClampPoint(new Vector2(_queueRect.GetCenter().X, _queueRect.Position.Y - 20f))
            };
            return _cachedStaticNavigationAnchors;
        }

        private string BuildObstacleSignature(GameManager? game)
        {
            var builder = new System.Text.StringBuilder(128);
            builder.Append(MathF.Round(_shopBounds.Size.X)).Append('x').Append(MathF.Round(_shopBounds.Size.Y));
            if (game == null)
                return builder.ToString();

            foreach (var shelf in game.Inventory.Shelves.OrderBy(shelf => shelf.Id))
                builder.Append("|s").Append(shelf.Id).Append(':').Append(shelf.ProductId).Append(':').Append(shelf.Capacity).Append(':').Append((int)shelf.DisplayType);

            return builder.ToString();
        }

        private IEnumerable<Vector2> GetObstacleWaypoints(Rect2 obstacle)
        {
            float offset = 18f;
            yield return new Vector2(obstacle.Position.X - offset, obstacle.Position.Y - offset);
            yield return new Vector2(obstacle.End.X + offset, obstacle.Position.Y - offset);
            yield return new Vector2(obstacle.End.X + offset, obstacle.End.Y + offset);
            yield return new Vector2(obstacle.Position.X - offset, obstacle.End.Y + offset);
            yield return new Vector2(obstacle.GetCenter().X, obstacle.Position.Y - offset);
            yield return new Vector2(obstacle.GetCenter().X, obstacle.End.Y + offset);
            yield return new Vector2(obstacle.Position.X - offset, obstacle.GetCenter().Y);
            yield return new Vector2(obstacle.End.X + offset, obstacle.GetCenter().Y);
        }

        private Vector2 FindNearestWalkablePoint(Vector2 point, List<Rect2> obstacles)
        {
            Vector2 clamped = ClampPoint(point);
            if (IsWalkablePoint(clamped, obstacles))
                return clamped;

            foreach (Vector2 candidate in GetStaticNavigationAnchors())
            {
                if (IsWalkablePoint(candidate, obstacles))
                    return candidate;
            }

            foreach (Rect2 obstacle in obstacles)
            {
                foreach (Vector2 waypoint in GetObstacleWaypoints(obstacle))
                {
                    Vector2 candidate = ClampPoint(waypoint);
                    if (IsWalkablePoint(candidate, obstacles))
                        return candidate;
                }
            }

            return clamped;
        }

        private bool HasClearSegment(Vector2 from, Vector2 to, List<Rect2> obstacles)
        {
            foreach (Rect2 obstacle in obstacles)
            {
                if (SegmentIntersectsRect(from, to, obstacle))
                    return false;
            }

            return true;
        }

        private bool IsWalkablePoint(Vector2 point, List<Rect2> obstacles)
        {
            if (!_shopBounds.Grow(-4f).HasPoint(point))
                return false;

            return obstacles.All(obstacle => !obstacle.HasPoint(point));
        }

        private static bool SegmentIntersectsRect(Vector2 from, Vector2 to, Rect2 rect)
        {
            if (rect.HasPoint(from) || rect.HasPoint(to))
                return true;

            float distance = from.DistanceTo(to);
            int steps = Math.Max(2, (int)Math.Ceiling(distance / SegmentStep));
            for (int index = 1; index < steps; index++)
            {
                float t = index / (float)steps;
                if (rect.HasPoint(from.Lerp(to, t)))
                    return true;
            }

            return false;
        }

        private List<Vector2> BuildFallbackPath(Vector2 start, Vector2 target, List<Rect2> obstacles)
        {
            foreach (Vector2 anchor in GetStaticNavigationAnchors())
            {
                if (!IsWalkablePoint(anchor, obstacles))
                    continue;
                if (HasClearSegment(start, anchor, obstacles) && HasClearSegment(anchor, target, obstacles))
                    return new List<Vector2> { anchor, target };
            }

            return new List<Vector2> { start };
        }

        private static int FindPointIndex(List<Vector2> points, Vector2 value)
        {
            for (int index = 0; index < points.Count; index++)
            {
                if (points[index].DistanceTo(value) <= 1f)
                    return index;
            }

            return -1;
        }

        private static Rect2 ExpandRect(Rect2 rect, float amount)
        {
            return new Rect2(
                rect.Position - new Vector2(amount, amount),
                rect.Size + new Vector2(amount * 2f, amount * 2f));
        }

        private Rect2 ResolveRect(Control? zone, Rect2 fallback)
        {
            if (zone == null)
                return fallback;

            Vector2 scale = zone.Scale;
            Vector2 size = new(
                Math.Abs(zone.Size.X * (Math.Abs(scale.X) < 0.001f ? 1f : scale.X)),
                Math.Abs(zone.Size.Y * (Math.Abs(scale.Y) < 0.001f ? 1f : scale.Y)));

            if (size.X < 8f || size.Y < 8f)
                return fallback;

            return ClampRect(new Rect2(zone.Position, size));
        }

        private Rect2 CreateGridSlot(Rect2 zone, int index, int columns, int rows, Vector2 slotSize, float topInset)
        {
            int slotCount = Math.Max(1, columns * rows);
            int normalizedIndex = Math.Abs(index) % slotCount;
            int column = normalizedIndex % columns;
            int row = normalizedIndex / columns;

            float usableWidth = Math.Max(slotSize.X, zone.Size.X - 20f);
            float xGap = columns <= 1 ? 0f : Math.Max(6f, (usableWidth - columns * slotSize.X) / (columns - 1));
            float startX = zone.Position.X + 10f;
            float startY = zone.Position.Y + topInset;
            float yGap = 8f;

            Vector2 position = new(
                startX + column * (slotSize.X + xGap),
                startY + row * (slotSize.Y + yGap));

            return ClampRect(new Rect2(position, slotSize));
        }

        private Rect2 ClampRect(Rect2 rect)
        {
            return new Rect2(ClampPosition(rect.Position, rect.Size), rect.Size);
        }

        public sealed record ShelfPlacement(
            int ShelfId,
            int ProductId,
            ShelfDisplayType DisplayType,
            Rect2 Rect,
            bool IsOriginal,
            int Group,
            int VariantIndex,
            IReadOnlyList<Vector2> BrowsePoints,
            Vector2 RestockPoint);

        private sealed class Vector2ApproxComparer : IEqualityComparer<Vector2>
        {
            public bool Equals(Vector2 left, Vector2 right) => left.DistanceTo(right) <= 0.5f;
            public int GetHashCode(Vector2 value) => HashCode.Combine(MathF.Round(value.X), MathF.Round(value.Y));
        }
    }
}
