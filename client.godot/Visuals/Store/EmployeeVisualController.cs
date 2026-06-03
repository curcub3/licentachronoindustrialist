using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Core.Simulation.Data;
using Core.Simulation.Logic;

namespace Client.Scripts.Visuals.Store
{
    public sealed class EmployeeVisualController
    {
        private readonly Control _shopView;
        private readonly StoreLayoutManager _layout;
        private readonly CustomerVisualController _customerController;
        private readonly Control _layer;
        private readonly Dictionary<string, EmployeeActor> _actors = new();

        private GameManager? _game;

        public EmployeeVisualController(Control shopView, StoreLayoutManager layout, CustomerVisualController customerController)
        {
            _shopView = shopView;
            _layout = layout;
            _customerController = customerController;
            _layer = EnsureLayer("EmployeeVisualLayer", 50);
        }

        public void Refresh(GameManager? game)
        {
            _game = game;
            _layout.RefreshLayout();

            if (_game == null)
            {
                foreach (EmployeeActor actor in _actors.Values)
                    actor.Root.Visible = false;
                return;
            }

            var desiredKeys = new HashSet<string>();
            int cashierSlot = 0;
            int stockerSlot = 0;
            int salesSlot = 0;
            int securitySlot = 0;
            int managerSlot = 0;

            foreach (EmployeeProfile employee in _game.Employees.Employees)
            {
                EmployeeRole role = employee.RoleType;
                int roleSlot = role switch
                {
                    EmployeeRole.Cashier => cashierSlot++,
                    EmployeeRole.Stocker => stockerSlot++,
                    EmployeeRole.SalesAssociate => salesSlot++,
                    EmployeeRole.Security => securitySlot++,
                    _ => managerSlot++
                };

                string key = $"{employee.Name}|{employee.Role}|{roleSlot}";
                desiredKeys.Add(key);
                if (!_actors.TryGetValue(key, out EmployeeActor? actor))
                {
                    actor = CreateActor(key);
                    _actors[key] = actor;
                }

                actor.Root.Visible = true;
                actor.NameLabel.Text = employee.Name;
                actor.RoleLabel.Text = RoleToRomanian(role);
                actor.Role = role;
                actor.RoleSlot = roleSlot;
                actor.Speed = GetRoleSpeed(role);
                actor.Body.Color = GetRoleColor(role);
                if (!actor.Initialized)
                {
                    actor.Initialized = true;
                    actor.PathIndex = 0;
                    actor.TargetProductId = PickRestockProduct(_game, roleSlot);
                    actor.Position = GetInitialPosition(actor);
                    actor.FinalTarget = actor.Position;
                }
            }

            foreach (string key in _actors.Keys.ToList())
            {
                if (desiredKeys.Contains(key))
                    continue;

                _actors[key].Root.QueueFree();
                _actors.Remove(key);
            }
        }

        public void Advance(double delta)
        {
            if (_game == null)
                return;

            foreach (EmployeeActor actor in _actors.Values)
            {
                if (!actor.Root.Visible)
                    continue;

                Vector2 target = GetTarget(actor);
                if (MoveActor(actor, target, actor.Speed, (float)delta) && actor.Position.DistanceTo(target) <= 3f)
                {
                    actor.HoldTimer += (float)delta;
                    if (actor.HoldTimer >= GetHoldDuration(actor.Role))
                    {
                        actor.HoldTimer = 0f;
                        actor.PathStep += 1;
                        if (actor.Role == EmployeeRole.Stocker && actor.PathStep % 2 == 1)
                            actor.TargetProductId = PickRestockProduct(_game, actor.RoleSlot + actor.PathStep);
                    }
                }
                else
                {
                    actor.HoldTimer = 0f;
                }

                actor.Root.Position = _layout.ClampPosition(actor.Position - new Vector2(actor.Root.Size.X * 0.5f, 0f), actor.Root.Size);
            }
        }

        private EmployeeActor CreateActor(string key)
        {
            var root = new Control
            {
                Name = key.Replace('|', '_'),
                Size = new Vector2(92f, 54f),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            var body = new ColorRect
            {
                Name = "Body",
                Position = new Vector2(37f, 0f),
                Size = new Vector2(18f, 18f),
                Color = new Color(0.45f, 0.95f, 0.62f, 1f),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            var labelBackdrop = new ColorRect
            {
                Name = "LabelBackdrop",
                Position = new Vector2(4f, 20f),
                Size = new Vector2(84f, 30f),
                Color = new Color(0.05f, 0.06f, 0.08f, 0.72f),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            var nameLabel = new Label
            {
                Name = "NameLabel",
                Position = new Vector2(6f, 22f),
                Size = new Vector2(80f, 12f),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ClipText = true,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 11);

            var roleLabel = new Label
            {
                Name = "RoleLabel",
                Position = new Vector2(6f, 34f),
                Size = new Vector2(80f, 12f),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ClipText = true,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            roleLabel.AddThemeFontSizeOverride("font_size", 10);

            root.AddChild(body);
            root.AddChild(labelBackdrop);
            root.AddChild(nameLabel);
            root.AddChild(roleLabel);
            _layer.AddChild(root);

            return new EmployeeActor(root, body, nameLabel, roleLabel);
        }

        private Vector2 GetInitialPosition(EmployeeActor actor)
        {
            return actor.Role switch
            {
                EmployeeRole.Cashier => _layout.GetCashierAnchorPoint(actor.RoleSlot),
                EmployeeRole.Stocker => _layout.GetStockerStoragePoint(actor.RoleSlot),
                EmployeeRole.SalesAssociate => _layout.GetSalesAssociatePoint(actor.RoleSlot),
                EmployeeRole.Security => _layout.GetSecurityPatrolPoint(actor.RoleSlot),
                _ => _layout.GetManagerPoint(actor.RoleSlot)
            };
        }

        private Vector2 GetTarget(EmployeeActor actor)
        {
            return actor.Role switch
            {
                EmployeeRole.Cashier => GetCashierTarget(actor),
                EmployeeRole.Stocker => GetStockerTarget(actor),
                EmployeeRole.SalesAssociate => GetSalesAssociateTarget(actor),
                EmployeeRole.Security => _layout.GetSecurityPatrolPoint(actor.PathStep + actor.RoleSlot),
                _ => _layout.GetManagerPoint(actor.PathStep + actor.RoleSlot)
            };
        }

        private Vector2 GetCashierTarget(EmployeeActor actor)
        {
            bool queueAssist = _game != null && _game.CurrentPhase == DayPhase.Business && _game.CurrentQueueLength > 1 && actor.PathStep % 2 == 1;
            if (queueAssist)
                return _layout.GetQueuePoint(Math.Min(actor.RoleSlot, 1)) + new Vector2(0f, -28f);

            return _layout.GetCashierAnchorPoint(actor.RoleSlot);
        }

        private Vector2 GetStockerTarget(EmployeeActor actor)
        {
            if (actor.PathStep % 2 == 0)
                return _layout.GetStockerStoragePoint(actor.RoleSlot);

            return _layout.GetStockerShelfPoint(actor.TargetProductId, actor.RoleSlot, _game);
        }

        private Vector2 GetSalesAssociateTarget(EmployeeActor actor)
        {
            IReadOnlyList<Vector2> customerFocusPoints = _customerController.GetCustomerFocusPoints();
            if (customerFocusPoints.Count > 0 && actor.PathStep % 3 == 2)
            {
                Vector2 focusPoint = customerFocusPoints[(actor.PathStep + actor.RoleSlot) % customerFocusPoints.Count];
                return _layout.ClampPoint(focusPoint + new Vector2(26f, -8f));
            }

            return _layout.GetSalesAssociatePoint(actor.PathStep + actor.RoleSlot);
        }

        private bool MoveActor(EmployeeActor actor, Vector2 finalTarget, float speed, float delta)
        {
            if (actor.Path.Count == 0 || actor.PathIndex >= actor.Path.Count || actor.FinalTarget.DistanceTo(finalTarget) > 3f)
            {
                actor.Path = _layout.BuildPath(actor.Position, finalTarget, _game);
                actor.PathIndex = 0;
                actor.FinalTarget = finalTarget;
            }

            Vector2 waypoint = actor.PathIndex < actor.Path.Count ? actor.Path[actor.PathIndex] : finalTarget;
            actor.Position = actor.Position.MoveToward(waypoint, speed * delta);
            if (actor.Position.DistanceTo(waypoint) <= 3f)
            {
                actor.PathIndex += 1;
                if (actor.PathIndex >= actor.Path.Count)
                    return true;
            }

            return false;
        }

        private int PickRestockProduct(GameManager game, int seed)
        {
            var shelvesNeedingAttention = game.Inventory.Shelves
                .Where(shelf => shelf.State != ShelfState.Full)
                .OrderBy(shelf => shelf.State == ShelfState.Empty ? 0 : shelf.State == ShelfState.LowStock ? 1 : 2)
                .ThenBy(shelf => shelf.Id)
                .ToList();

            if (shelvesNeedingAttention.Count == 0)
            {
                return seed % 3 switch
                {
                    0 => 1,
                    1 => 2,
                    _ => 3
                };
            }

            return shelvesNeedingAttention[seed % shelvesNeedingAttention.Count].ProductId;
        }

        private static float GetRoleSpeed(EmployeeRole role)
        {
            return role switch
            {
                EmployeeRole.Cashier => 48f,
                EmployeeRole.Stocker => 60f,
                EmployeeRole.SalesAssociate => 55f,
                EmployeeRole.Security => 58f,
                _ => 52f
            };
        }

        private static float GetHoldDuration(EmployeeRole role)
        {
            return role switch
            {
                EmployeeRole.Cashier => 0.35f,
                EmployeeRole.Stocker => 0.45f,
                EmployeeRole.SalesAssociate => 0.40f,
                EmployeeRole.Security => 0.55f,
                _ => 0.60f
            };
        }

        private static Color GetRoleColor(EmployeeRole role)
        {
            return role switch
            {
                EmployeeRole.Cashier => new Color(0.95f, 0.90f, 0.35f, 1f),
                EmployeeRole.Stocker => new Color(0.80f, 0.65f, 0.95f, 1f),
                EmployeeRole.SalesAssociate => new Color(0.96f, 0.62f, 0.35f, 1f),
                EmployeeRole.Security => new Color(0.92f, 0.38f, 0.38f, 1f),
                _ => new Color(0.45f, 0.95f, 0.62f, 1f)
            };
        }

        private static string RoleToRomanian(EmployeeRole role)
        {
            return role switch
            {
                EmployeeRole.Cashier => "Casier",
                EmployeeRole.Stocker => "Stoc",
                EmployeeRole.SalesAssociate => "Vânzări",
                EmployeeRole.Security => "Pază",
                _ => "Manager"
            };
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

        private sealed class EmployeeActor
        {
            public EmployeeActor(Control root, ColorRect body, Label nameLabel, Label roleLabel)
            {
                Root = root;
                Body = body;
                NameLabel = nameLabel;
                RoleLabel = roleLabel;
            }

            public Control Root { get; }
            public ColorRect Body { get; }
            public Label NameLabel { get; }
            public Label RoleLabel { get; }
            public EmployeeRole Role { get; set; }
            public int RoleSlot { get; set; }
            public int PathStep { get; set; }
            public int TargetProductId { get; set; }
            public float Speed { get; set; }
            public float HoldTimer { get; set; }
            public Vector2 Position { get; set; }
            public bool Initialized { get; set; }
            public Vector2 FinalTarget { get; set; }
            public List<Vector2> Path { get; set; } = new();
            public int PathIndex { get; set; }
        }
    }
}
