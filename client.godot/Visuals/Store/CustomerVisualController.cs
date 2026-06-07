using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Core.Simulation.Data;
using Core.Simulation.Logic;

namespace Client.Scripts.Visuals.Store
{
    public sealed class CustomerVisualController
    {
        private const int MaxCustomers = 6;
        private readonly Control _shopView;
        private readonly StoreLayoutManager _layout;
        private readonly Control _layer;
        private readonly List<CustomerActor> _actors = new();
        private readonly List<Vector2> _customerFocusPoints = new();
        private readonly Random _random = new(20260603);

        private GameManager? _game;
        private int _nextQueueSequence = 1;
        private int _respawnCursor;

        public CustomerVisualController(Control shopView, StoreLayoutManager layout)
        {
            _shopView = shopView;
            _layout = layout;
            _layer = EnsureLayer("CustomerVisualLayer", 60);
            EnsureActors();
        }

        public void Refresh(GameManager? game)
        {
            _game = game;
            _layout.RefreshLayout();

            if (_game == null || _game.CurrentPhase != DayPhase.Business)
            {
                foreach (CustomerActor actor in _actors)
                    HideActor(actor);
                return;
            }

            int desiredActiveCustomers = _game.GetGuidedCustomerVisualCount(CalculateDesiredCustomerCount(_game));
            int activeCount = _actors.Count(actor => actor.Active);

            while (activeCount < desiredActiveCustomers)
            {
                CustomerActor? actorToSpawn = _actors.FirstOrDefault(actor => !actor.Active && !actor.Root.Visible);
                if (actorToSpawn == null)
                    break;

                SpawnActor(actorToSpawn, _respawnCursor++);
                activeCount++;
            }

            if (activeCount > desiredActiveCustomers)
            {
                foreach (CustomerActor actor in _actors.Where(actor => actor.Active).OrderByDescending(actor => actor.SpawnIndex).Take(activeCount - desiredActiveCustomers))
                    BeginLeaving(actor);
            }
        }

        public void Advance(double delta)
        {
            if (_game == null || _game.CurrentPhase != DayPhase.Business)
                return;

            foreach (CustomerActor actor in _actors)
            {
                if (!actor.Root.Visible)
                    continue;

                UpdateActor(actor, (float)delta);
                ApplyVisualState(actor);
            }
        }

        public void InvalidatePaths()
        {
            foreach (CustomerActor actor in _actors)
            {
                actor.Path.Clear();
                actor.PathIndex = 0;
                actor.FinalTarget = Vector2.Inf;
            }
        }

        public IReadOnlyList<Vector2> GetCustomerFocusPoints()
        {
            _customerFocusPoints.Clear();
            foreach (CustomerActor actor in _actors)
            {
                if (actor.Root.Visible && actor.State is CustomerVisualState.Browsing or CustomerVisualState.Deciding or CustomerVisualState.Queueing or CustomerVisualState.Paying)
                    _customerFocusPoints.Add(actor.Position + actor.Root.Size * 0.5f);
            }

            return _customerFocusPoints;
        }

        private void EnsureActors()
        {
            if (_actors.Count > 0)
                return;

            for (int index = 0; index < MaxCustomers; index++)
            {
                var root = new Control
                {
                    Name = $"CustomerActor{index + 1}",
                    Visible = false,
                    Size = new Vector2(58f, 34f),
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };

                var body = new ColorRect
                {
                    Name = "Body",
                    Position = new Vector2(21f, 0f),
                    Size = new Vector2(16f, 16f),
                    Color = new Color(0.78f, 0.88f, 0.95f, 1f),
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };

                var labelBackdrop = new ColorRect
                {
                    Name = "LabelBackdrop",
                    Position = new Vector2(2f, 18f),
                    Size = new Vector2(54f, 14f),
                    Color = new Color(0.12f, 0.06f, 0.08f, 0.82f),
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };

                var label = new Label
                {
                    Name = "Label",
                    Position = new Vector2(3f, 18f),
                    Size = new Vector2(52f, 14f),
                    Text = "Client",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    ClipText = true,
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };
                label.AddThemeFontSizeOverride("font_size", 10);

                root.AddChild(body);
                root.AddChild(labelBackdrop);
                root.AddChild(label);
                _layer.AddChild(root);
                _actors.Add(new CustomerActor(root, body, label));
            }
        }

        private void SpawnActor(CustomerActor actor, int spawnIndex)
        {
            if (_game == null)
                return;

            actor.Active = true;
            actor.Root.Visible = true;
            actor.SpawnIndex = spawnIndex;
            actor.Position = _layout.GetSpawnPoint(spawnIndex % MaxCustomers);
            actor.Speed = 82f + (spawnIndex % 3) * 8f;
            actor.StateTimer = 0f;
            actor.BrowseStopsRemaining = _random.Next(0, 100) < 35 ? 2 : 1;
            actor.TargetProductId = ChooseProductId(_game, spawnIndex);
            actor.WantsToBuy = EvaluatePurchaseIntent(_game, actor.TargetProductId);
            actor.QueueSequence = 0;
            actor.Path.Clear();
            actor.PathIndex = 0;
            SetState(actor, CustomerVisualState.Entering, _layout.GetEntranceWalkPoint(spawnIndex % MaxCustomers));
        }

        private void UpdateActor(CustomerActor actor, float delta)
        {
            switch (actor.State)
            {
                case CustomerVisualState.Entering:
                    if (MoveActor(actor, actor.Target, actor.Speed, delta) && actor.Position.DistanceTo(actor.Target) <= 2.5f)
                        SetState(actor, CustomerVisualState.Browsing, _layout.GetBrowsePoint(actor.TargetProductId, actor.SpawnIndex, _game));
                    break;

                case CustomerVisualState.Browsing:
                    if (MoveActor(actor, actor.Target, actor.Speed, delta) && actor.Position.DistanceTo(actor.Target) <= 2.5f)
                    {
                        actor.StateTimer += delta;
                        if (actor.StateTimer >= 0.45f)
                        {
                            actor.StateTimer = 0f;
                            actor.State = CustomerVisualState.Deciding;
                        }
                    }
                    break;

                case CustomerVisualState.Deciding:
                    actor.StateTimer += delta;
                    if (actor.StateTimer < 0.35f)
                        break;

                    if (actor.WantsToBuy && QueueingActorsCount() < CalculateTargetQueueCount(_game!))
                    {
                        actor.QueueSequence = _nextQueueSequence++;
                        SetState(actor, CustomerVisualState.Queueing, _layout.GetQueuePoint(GetQueueIndex(actor)));
                    }
                    else if (actor.BrowseStopsRemaining > 1)
                    {
                        actor.BrowseStopsRemaining -= 1;
                        actor.TargetProductId = ChooseAlternativeProductId(_game!, actor.TargetProductId, actor.SpawnIndex);
                        actor.WantsToBuy = EvaluatePurchaseIntent(_game!, actor.TargetProductId);
                        SetState(actor, CustomerVisualState.Browsing, _layout.GetSecondaryBrowsePoint(actor.TargetProductId, actor.SpawnIndex, _game));
                    }
                    else
                    {
                        BeginLeaving(actor);
                    }
                    break;

                case CustomerVisualState.Queueing:
                    actor.Target = _layout.GetQueuePoint(GetQueueIndex(actor));
                    if (MoveActor(actor, actor.Target, actor.Speed, delta) && actor.Position.DistanceTo(actor.Target) <= 2.5f && GetQueueIndex(actor) == 0)
                    {
                        actor.StateTimer += delta;
                        if (actor.StateTimer >= 0.30f && _game!.Employees.CountRole(EmployeeRole.Cashier) > 0)
                            SetState(actor, CustomerVisualState.Paying, _layout.GetRegisterServicePoint(0));
                    }
                    else
                    {
                        actor.StateTimer = 0f;
                    }
                    break;

                case CustomerVisualState.Paying:
                    if (MoveActor(actor, actor.Target, actor.Speed + 12f, delta) && actor.Position.DistanceTo(actor.Target) <= 2.5f)
                    {
                        actor.StateTimer += delta;
                        float payDuration = 0.45f + Math.Min(0.45f, _game!.CurrentQueueLength * 0.08f);
                        if (actor.StateTimer >= payDuration)
                            BeginLeaving(actor);
                    }
                    break;

                case CustomerVisualState.Leaving:
                    if (MoveActor(actor, actor.Target, actor.Speed + 18f, delta) && actor.Position.DistanceTo(actor.Target) <= 2.5f)
                    {
                        HideActor(actor);
                        if (_game != null && _game.CurrentPhase == DayPhase.Business)
                            SpawnActor(actor, _respawnCursor++);
                    }
                    break;
            }
        }

        private bool MoveActor(CustomerActor actor, Vector2 finalTarget, float speed, float delta)
        {
            if (actor.Path.Count == 0 || actor.PathIndex >= actor.Path.Count || actor.FinalTarget.DistanceTo(finalTarget) > 3f)
            {
                actor.Path = _layout.BuildPath(actor.Position, finalTarget, _game);
                actor.PathIndex = 0;
                actor.FinalTarget = finalTarget;
            }

            Vector2 waypoint = actor.PathIndex < actor.Path.Count ? actor.Path[actor.PathIndex] : finalTarget;
            actor.Position = actor.Position.MoveToward(waypoint, speed * delta);
            if (actor.Position.DistanceTo(waypoint) <= 2.5f)
            {
                actor.PathIndex += 1;
                if (actor.PathIndex >= actor.Path.Count)
                    return true;
            }

            return false;
        }

        private void SetState(CustomerActor actor, CustomerVisualState state, Vector2 target)
        {
            actor.State = state;
            actor.Target = target;
            actor.StateTimer = 0f;
            actor.Path.Clear();
            actor.PathIndex = 0;
            actor.FinalTarget = target;
        }

        private void ApplyVisualState(CustomerActor actor)
        {
            actor.Root.Position = _layout.ClampPosition(actor.Position, actor.Root.Size);
            actor.Body.Color = actor.State switch
            {
                CustomerVisualState.Queueing => new Color(0.9f, 0.45f, 0.37f, 1f),
                CustomerVisualState.Paying => new Color(0.97f, 0.72f, 0.58f, 1f),
                CustomerVisualState.Leaving => new Color(0.75f, 0.54f, 0.56f, 0.95f),
                CustomerVisualState.Deciding => new Color(0.93f, 0.55f, 0.47f, 1f),
                _ => new Color(0.20f, 0.72f, 0.95f, 1f)
            };
        }

        private int CalculateDesiredCustomerCount(GameManager game)
        {
            int demandBias = Math.Min(2, Math.Max(0, game.Customers.DailyDemandMultiplier - 2));
            int queueBias = Math.Min(2, game.CurrentQueueLength / 2);
            int serviceBias = game.CurrentCustomersServedToday >= 3 ? 1 : 0;
            return Math.Clamp(3 + demandBias + queueBias + serviceBias, 3, MaxCustomers);
        }

        private int CalculateTargetQueueCount(GameManager game)
        {
            if (game.Employees.CountRole(EmployeeRole.Cashier) == 0)
                return 0;

            return Math.Clamp(Math.Max(1, game.CurrentQueueLength), 1, 4);
        }

        private int QueueingActorsCount()
        {
            return _actors.Count(actor => actor.Root.Visible && (actor.State is CustomerVisualState.Queueing or CustomerVisualState.Paying));
        }

        private int GetQueueIndex(CustomerActor actor)
        {
            if (actor.QueueSequence <= 0)
                return 0;

            int index = 0;
            foreach (CustomerActor candidate in _actors)
            {
                if (!candidate.Root.Visible || candidate.QueueSequence <= 0 || candidate.State is not (CustomerVisualState.Queueing or CustomerVisualState.Paying))
                    continue;

                if (candidate.QueueSequence < actor.QueueSequence)
                    index++;
            }

            return index;
        }

        private void BeginLeaving(CustomerActor actor)
        {
            actor.Active = false;
            actor.QueueSequence = 0;
            SetState(actor, CustomerVisualState.Leaving, _layout.GetExitPoint(actor.SpawnIndex % MaxCustomers));
        }

        private void HideActor(CustomerActor actor)
        {
            actor.Active = false;
            actor.QueueSequence = 0;
            actor.Root.Visible = false;
            actor.StateTimer = 0f;
            actor.Path.Clear();
            actor.PathIndex = 0;
        }

        private int ChooseProductId(GameManager game, int preferredSeed)
        {
            var candidates = game.Inventory.Products
                .Select(product => new
                {
                    ProductId = product.Id,
                    Score = ScoreProductInterest(game, product) + ((preferredSeed + product.Id) % 17)
                })
                .OrderByDescending(entry => entry.Score)
                .ToList();

            return candidates.FirstOrDefault()?.ProductId ?? 1;
        }

        private int ChooseAlternativeProductId(GameManager game, int currentProductId, int seed)
        {
            var candidates = game.Inventory.Products
                .Where(product => product.Id != currentProductId)
                .Select(product => new
                {
                    ProductId = product.Id,
                    Score = ScoreProductInterest(game, product) + ((seed + product.Id * 3) % 11)
                })
                .OrderByDescending(entry => entry.Score)
                .ToList();

            return candidates.FirstOrDefault()?.ProductId ?? currentProductId;
        }

        private bool EvaluatePurchaseIntent(GameManager game, int productId)
        {
            Product? product = game.Inventory.GetProduct(productId);
            if (product == null)
                return false;

            int shelfStock = game.Inventory.GetShelvesForProduct(productId).Sum(shelf => shelf.CurrentStock);
            if (shelfStock <= 0)
                return false;

            double score = ScoreProductInterest(game, product);
            score -= game.CurrentQueueLength * 5.5d;
            score += _random.Next(-10, 11);
            return score >= 52d;
        }

        private double ScoreProductInterest(GameManager game, Product product)
        {
            int shelfStock = game.Inventory.GetShelvesForProduct(product.Id).Sum(shelf => shelf.CurrentStock);
            double priceRatio = product.CostPrice.ToMicros() <= 0
                ? 1d
                : (double)product.SalePrice.ToMicros() / product.CostPrice.ToMicros();
            double demandFactor = game.GetProductDemandBasisPoints(product) / 220d;
            double globalDemandFactor = game.GetGlobalDemandBasisPoints() / 950d;
            double stockFactor = Math.Min(28d, shelfStock * 2.5d);
            double popularityFactor = product.Popularity * 0.55d;
            double pricePenalty = Math.Max(0d, priceRatio - 1.75d) * 18d;
            return demandFactor + globalDemandFactor + stockFactor + popularityFactor - pricePenalty;
        }

        private Control EnsureLayer(string name, int zIndex)
        {
            if (_shopView.GetNodeOrNull<Control>(name) is { } existingLayer)
                return existingLayer;

            var layer = new Control
            {
                Name = name,
                LayoutMode = 1,
                AnchorsPreset = (int)Control.LayoutPreset.FullRect,
                AnchorRight = 1f,
                AnchorBottom = 1f,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                ZIndex = zIndex
            };
            _shopView.AddChild(layer);
            return layer;
        }

        public enum CustomerVisualState
        {
            Entering,
            Browsing,
            Deciding,
            Queueing,
            Paying,
            Leaving
        }

        private sealed class CustomerActor
        {
            public CustomerActor(Control root, ColorRect body, Label label)
            {
                Root = root;
                Body = body;
                Label = label;
            }

            public Control Root { get; }
            public ColorRect Body { get; }
            public Label Label { get; }
            public bool Active { get; set; }
            public CustomerVisualState State { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Target { get; set; }
            public Vector2 FinalTarget { get; set; }
            public List<Vector2> Path { get; set; } = new();
            public int PathIndex { get; set; }
            public float Speed { get; set; }
            public float StateTimer { get; set; }
            public int SpawnIndex { get; set; }
            public int TargetProductId { get; set; }
            public bool WantsToBuy { get; set; }
            public int BrowseStopsRemaining { get; set; }
            public int QueueSequence { get; set; }
        }
    }
}
