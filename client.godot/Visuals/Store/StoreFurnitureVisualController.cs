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
                        : CreateSimpleFurnitureVisual(key);
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

                if (visual.Panel != null && descriptor.Kind != FurnitureKind.Shelf)
                    visual.Panel.AddThemeStyleboxOverride("panel", CreateTemporaryFurnitureStyle(descriptor.Color));

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
                            FurnitureKind.Simple,
                            _layout.GetStorageFurnitureSlot(storageSlot),
                            "Depozit\nvizual temporar",
                            new Color(0.78f, 0.28f, 0.29f, 0.95f),
                            null);
                        storageSlot++;
                        break;
                    case 5:
                        descriptors[$"decor-{decorationSlot}"] = new FurnitureDescriptor(
                            FurnitureKind.Simple,
                            _layout.GetDecorationFurnitureSlot(decorationSlot),
                            "Neon\nvizual temporar",
                            new Color(0.86f, 0.52f, 0.48f, 0.95f),
                            null);
                        decorationSlot++;
                        break;
                    case 6:
                        descriptors[$"scanner-{scannerSlot}"] = new FurnitureDescriptor(
                            FurnitureKind.Simple,
                            _layout.GetHardwareFurnitureSlot(catalogItemId, scannerSlot),
                            "Scanner\nvizual temporar",
                            new Color(0.92f, 0.58f, 0.5f, 0.95f),
                            null);
                        scannerSlot++;
                        break;
                    case 7:
                        descriptors[$"cart-{cartSlot}"] = new FurnitureDescriptor(
                            FurnitureKind.Simple,
                            _layout.GetHardwareFurnitureSlot(catalogItemId, cartSlot),
                            "Cărucior\nvizual temporar",
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
            Control? originalShelf = _layout.CreateShelfVisualFromOriginal(displayType);
            if (originalShelf == null)
                return CreateMissingAssetVisual(key, $"ASSET LIPSĂ: raft {displayType}");

            Control shelfRoot = originalShelf;
            shelfRoot.Name = key;
            shelfRoot.MouseFilter = Control.MouseFilterEnum.Ignore;

            Label? label = FindFirstLabel(shelfRoot);
            if (label != null)
            {
                label.ClipText = false;
                label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            }

            _layer.AddChild(shelfRoot);
            return new FurnitureVisual(shelfRoot, null, null, label);
        }

        private FurnitureVisual CreateSimpleFurnitureVisual(string key)
        {
            var root = new PanelContainer
            {
                Name = key,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            var label = new Label
            {
                Name = "Label",
                LayoutMode = 2,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                ClipText = true,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            label.AddThemeFontSizeOverride("font_size", 10);

            root.AddChild(label);
            _layer.AddChild(root);
            return new FurnitureVisual(root, root, null, label);
        }

        private FurnitureVisual CreateMissingAssetVisual(string key, string labelText)
        {
            var visual = CreateSimpleFurnitureVisual(key);
            if (visual.Label != null)
                visual.Label.Text = labelText;
            visual.Panel?.AddThemeStyleboxOverride("panel", CreateWarningMissingAssetStyle());
            return visual;
        }

        private static StyleBoxFlat CreateTemporaryFurnitureStyle(Color background)
        {
            var style = new StyleBoxFlat
            {
                BgColor = new Color(background.R * 0.62f, background.G * 0.62f, background.B * 0.62f, 0.92f),
                BorderColor = new Color(0.96f, 0.86f, 0.72f, 1f),
                BorderWidthLeft = 2,
                BorderWidthTop = 2,
                BorderWidthRight = 2,
                BorderWidthBottom = 2,
                CornerRadiusTopLeft = 2,
                CornerRadiusTopRight = 2,
                CornerRadiusBottomRight = 2,
                CornerRadiusBottomLeft = 2
            };
            return style;
        }

        private static StyleBoxFlat CreateWarningMissingAssetStyle()
        {
            var style = CreateTemporaryFurnitureStyle(new Color(0.52f, 0.08f, 0.08f, 1f));
            style.BorderColor = new Color(1f, 0.82f, 0.24f, 1f);
            return style;
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
            Simple
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
                : this(root, null, background, label)
            {
            }

            public FurnitureVisual(Control root, PanelContainer? panel, ColorRect? background, Label? label)
            {
                Root = root;
                Panel = panel;
                Background = background;
                Label = label;
            }

            public Control Root { get; }
            public PanelContainer? Panel { get; }
            public ColorRect? Background { get; }
            public Label? Label { get; }
        }
    }
}
