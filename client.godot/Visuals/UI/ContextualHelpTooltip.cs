using System;
using System.Collections.Generic;
using Godot;

namespace Client.Scripts.Visuals.UI
{
    public sealed class ContextualHelpTooltip
    {
        private sealed record Entry(string ShortText, string ExpandedText);

        private readonly Control _root;
        private readonly Dictionary<Control, Entry> _entries = new();
        private readonly PanelContainer _panel;
        private readonly Label _bodyLabel;
        private readonly Theme? _theme;
        private Control? _activeTarget;
        private bool _expanded;

        public bool Enabled { get; private set; } = true;

        public ContextualHelpTooltip(Control root)
        {
            _root = root;
            _theme = GD.Load<Theme>("res://Themes/ChronoTheme.tres");

            _panel = new PanelContainer
            {
                Name = "ContextualHelpTooltip",
                Visible = false,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                CustomMinimumSize = new Vector2(280, 0),
                Theme = _theme,
                ThemeTypeVariation = "ChronoTooltipPanel"
            };
            _panel.SetAnchorsPreset(Control.LayoutPreset.TopLeft);

            var margin = new MarginContainer { Name = "ContextualHelpMargin" };
            margin.AddThemeConstantOverride("margin_left", 12);
            margin.AddThemeConstantOverride("margin_top", 10);
            margin.AddThemeConstantOverride("margin_right", 12);
            margin.AddThemeConstantOverride("margin_bottom", 10);
            _panel.AddChild(margin);

            var stack = new VBoxContainer { Name = "ContextualHelpRoot" };
            stack.AddThemeConstantOverride("separation", 4);
            margin.AddChild(stack);

            _bodyLabel = new Label
            {
                Name = "ContextualHelpBody",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Theme = _theme,
                ThemeTypeVariation = "ChronoTooltipBody"
            };
            stack.AddChild(_bodyLabel);

            _root.AddChild(_panel);
            _panel.MoveToFront();
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            if (!Enabled)
                Hide();
        }

        public void Dismiss()
        {
            Hide();
        }

        public void Register(Control? target, string shortText, string expandedText)
        {
            if (target == null)
                return;

            _entries[target] = new Entry(shortText, expandedText);
            target.FocusMode = Control.FocusModeEnum.All;
            target.MouseEntered += () => ShowFor(target, expanded: false);
            target.MouseExited += () =>
            {
                if (!target.HasFocus())
                    HideIfActive(target);
            };
            target.FocusEntered += () => ShowFor(target, expanded: true);
            target.FocusExited += () => HideIfActive(target);
        }

        public void Refresh()
        {
            if (_activeTarget == null || !_entries.ContainsKey(_activeTarget))
                return;

            ShowFor(_activeTarget, _expanded);
        }

        private void ShowFor(Control target, bool expanded)
        {
            if (!Enabled || !_entries.TryGetValue(target, out var entry))
                return;

            _activeTarget = target;
            _expanded = expanded;
            _bodyLabel.Text = expanded && !string.IsNullOrWhiteSpace(entry.ExpandedText)
                ? entry.ExpandedText
                : entry.ShortText;
            _panel.Visible = true;
            _panel.MoveToFront();
            PositionNear(target);
        }

        private void PositionNear(Control target)
        {
            var viewportSize = _root.GetViewportRect().Size;
            var targetRect = target.GetGlobalRect();
            var desired = targetRect.Position + new Vector2(0, targetRect.Size.Y + 8);
            var maxWidth = Math.Min(360.0f, Math.Max(260.0f, viewportSize.X - 32.0f));

            _panel.CustomMinimumSize = new Vector2(maxWidth, 0);
            _panel.Size = new Vector2(maxWidth, 0);
            _panel.ResetSize();

            var panelSize = _panel.Size;
            if (panelSize.Y <= 0)
                panelSize = _panel.GetCombinedMinimumSize();

            if (desired.X + panelSize.X > viewportSize.X - 12)
                desired.X = viewportSize.X - panelSize.X - 12;
            if (desired.Y + panelSize.Y > viewportSize.Y - 12)
                desired.Y = targetRect.Position.Y - panelSize.Y - 8;

            desired.X = Math.Max(12.0f, desired.X);
            desired.Y = Math.Max(12.0f, desired.Y);
            _panel.Position = desired;
        }

        private void HideIfActive(Control target)
        {
            if (_activeTarget == target)
                Hide();
        }

        private void Hide()
        {
            _activeTarget = null;
            _expanded = false;
            _panel.Visible = false;
        }
    }
}
