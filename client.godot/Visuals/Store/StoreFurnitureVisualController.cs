using System.Collections.Generic;
using System.Linq;
using Godot;
using Core.Simulation.Data;
using Core.Simulation.Logic;
using Client.Localization;

namespace Client.Scripts.Visuals.Store
{
    public sealed class StoreFurnitureVisualController
    {
        private readonly Control _shopView;
        private readonly StoreLayoutManager _layout;
        private readonly Control _layer;
        private readonly Dictionary<string, FurnitureVisual> _visuals = new();

        public StoreFurnitureVisualController(Control shopView, StoreLayoutManager layout)
        {
            _shopView = shopView;
            _layout = layout;
            _layer = EnsureLayer("StoreFurnitureVisualLayer", 30);
        }

        public void Refresh(GameManager? game)
        {
            _layout.RefreshLayout();

            if (game == null)
            {
                foreach (FurnitureVisual visual in _visuals.Values)
                    visual.Root.Visible = false;
                return;
            }

            var descriptors = BuildDescriptors(game);
            foreach (string key in _visuals.Keys.ToList())
            {
                if (descriptors.ContainsKey(key))
                    continue;

                _visuals[key].Root.QueueFree();
                _visuals.Remove(key);
            }

            foreach ((string key, FurnitureDescriptor descriptor) in descriptors)
            {
                if (!_visuals.TryGetValue(key, out FurnitureVisual? visual))
                {
                    visual = descriptor.Kind == FurnitureKind.Shelf
                        ? CreateShelfVisual(key, descriptor.ShelfVariant ?? ShelfDisplayType.Basic)
                        : CreatePlaceholderVisual(key);
                    _visuals[key] = visual;
                }

                visual.Root.Visible = true;
                visual.Root.Position = _layout.ClampPosition(descriptor.Rect.Position, descriptor.Rect.Size);
                visual.Root.Size = descriptor.Rect.Size;

                if (visual.Background != null)
                {
                    visual.Background.Visible = descriptor.Kind != FurnitureKind.Shelf;
                    visual.Background.Size = descriptor.Rect.Size;
                    visual.Background.Color = descriptor.Color;
                }

                if (visual.Label != null)
                {
                    visual.Label.Text = descriptor.Label;
                    visual.Label.Size = descriptor.Rect.Size;
                }
            }
        }

        private Dictionary<string, FurnitureDescriptor> BuildDescriptors(GameManager game)
        {
            var descriptors = new Dictionary<string, FurnitureDescriptor>();

            foreach (var placement in _layout.GetPurchasedShelfPlacements(game))
            {
                var shelf = game.Inventory.GetShelf(placement.ShelfId);
                string label = BuildShelfLabel(game, placement.ProductId, shelf?.CurrentStock ?? 0, shelf?.Capacity ?? 0);
                descriptors[$"shelf-{placement.ShelfId}"] = new FurnitureDescriptor(
                    FurnitureKind.Shelf,
                    placement.Rect,
                    label,
                    Colors.Transparent,
                    placement.DisplayType);
            }

            int storageSlot = 0;
            int decorationSlot = 0;
            int scannerSlot = 0;
            int cartSlot = 0;
            foreach (int catalogItemId in game.PurchasedCatalogItemIds)
            {
                switch (catalogItemId)
                {
                    case 4:
                        descriptors[$"storage-{storageSlot}"] = new FurnitureDescriptor(
                            FurnitureKind.Placeholder,
                            _layout.GetStorageFurnitureSlot(storageSlot),
                            "Depozit",
                            new Color(0.78f, 0.28f, 0.29f, 0.95f),
                            null);
                        storageSlot++;
                        break;
                    case 5:
                        descriptors[$"decor-{decorationSlot}"] = new FurnitureDescriptor(
                            FurnitureKind.Placeholder,
                            _layout.GetDecorationFurnitureSlot(decorationSlot),
                            "Neon",
                            new Color(0.86f, 0.52f, 0.48f, 0.95f),
                            null);
                        decorationSlot++;
                        break;
                    case 6:
                        descriptors[$"scanner-{scannerSlot}"] = new FurnitureDescriptor(
                            FurnitureKind.Placeholder,
                            _layout.GetHardwareFurnitureSlot(catalogItemId, scannerSlot),
                            "Scanner",
                            new Color(0.92f, 0.58f, 0.5f, 0.95f),
                            null);
                        scannerSlot++;
                        break;
                    case 7:
                        descriptors[$"cart-{cartSlot}"] = new FurnitureDescriptor(
                            FurnitureKind.Placeholder,
                            _layout.GetHardwareFurnitureSlot(catalogItemId, cartSlot),
                            "Cărucior",
                            new Color(0.82f, 0.3f, 0.29f, 0.95f),
                            null);
                        cartSlot++;
                        break;
                }
            }

            return descriptors;
        }

        private FurnitureVisual CreateShelfVisual(string key, ShelfDisplayType displayType)
        {
            Control shelfRoot = _layout.CreateShelfVisualFromOriginal(displayType) ?? new Control();
            shelfRoot.Name = key;
            shelfRoot.MouseFilter = Control.MouseFilterEnum.Ignore;

            Label? label = FindFirstLabel(shelfRoot);
            if (label != null)
            {
                label.ClipText = false;
                label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            }

            _layer.AddChild(shelfRoot);
            return new FurnitureVisual(shelfRoot, null, label);
        }

        private FurnitureVisual CreatePlaceholderVisual(string key)
        {
            var root = new Control
            {
                Name = key,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            var background = new ColorRect
            {
                Name = "Background",
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            var label = new Label
            {
                Name = "Label",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            label.AddThemeFontSizeOverride("font_size", 10);

            root.AddChild(background);
            root.AddChild(label);
            _layer.AddChild(root);
            return new FurnitureVisual(root, background, label);
        }

        private static Label? FindFirstLabel(Node node)
        {
            if (node is Label label)
                return label;

            foreach (Node child in node.GetChildren())
            {
                Label? found = FindFirstLabel(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static string BuildShelfLabel(GameManager game, int productId, int currentStock, int capacity)
        {
            string productName = Localizer.ProductName(productId);
            string shelfName = capacity > 0 ? $"{currentStock}/{capacity}" : $"{currentStock}";
            return $"{productName}\nRaft {shelfName}";
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

        private enum FurnitureKind
        {
            Shelf,
            Placeholder
        }

        private sealed record FurnitureDescriptor(
            FurnitureKind Kind,
            Rect2 Rect,
            string Label,
            Color Color,
            ShelfDisplayType? ShelfVariant);

        private sealed class FurnitureVisual
        {
            public FurnitureVisual(Control root, ColorRect? background, Label? label)
            {
                Root = root;
                Background = background;
                Label = label;
            }

            public Control Root { get; }
            public ColorRect? Background { get; }
            public Label? Label { get; }
        }
    }
}
