# Optimization and Flow Audit

## Current Performance Risks
- `TickManager._Process` can call `UIManager.Refresh()` once per rendered frame during the business phase. That refresh repopulates product/shelf/staff pickers, rebuilds checklist UI, refreshes shop labels, and refreshes furniture/customer/employee visual controllers.
- `StoreLayoutManager.BuildPath()` calls `RefreshLayout()` and rebuilds obstacle lists on each path request. Customer and employee controllers only request new paths on target changes, but layout/obstacle work is still repeated for multiple actors.
- Customer queue indexing uses LINQ sorting per active actor update. With the current small actor cap this is acceptable, but it is avoidable per-frame allocation/work.
- Employee sales-associate targeting calls `GetCustomerFocusPoints()`, which allocates a list every frame when that role is visible.
- Inventory totals and shelf summaries use repeated LINQ scans. This is low risk at current data sizes, but it should stay out of per-frame refresh paths.

## Confusing Flow Points
- Price editing needs one explicit product target. Existing code now tracks `_activePriceProductId`; the target must be cleared whenever the price popup closes or loading resets UI state.
- Save/load should stay slot-based. The current flow opens save slots instead of a generic file picker, but load should also close all open runtime menus and reset stale selected targets.
- New game tutorial and onboarding are visible, but the early sequence should keep the next concrete action visible: stock shelf, set price, open store, observe customer.
- Furniture purchase happens directly from the catalog rather than through a separate placement preview. Minimal safe fix is to make the purchased item visible immediately and force path/layout refresh once.

## Visual Clutter and Readability Issues
- HUD text is dense and can clip on smaller widths. Keep it short and update only when values change.
- Shop labels contain several lines of state. They are useful, but should be refreshed only when state changes or on a light throttle.
- Menus use established theme resources and should keep the same controls/colors. No new visual scheme is needed.
- Feedback should stay short and Romanian; price/shelf/furniture actions already notify, but stale modal state must not linger.

## Expensive Update Loops
- `UIManager.RefreshRuntimePanel()` repopulates all option lists and rewrites many labels every refresh.
- `UIManager.RefreshShop2DView()` refreshes furniture and visual controllers every refresh even when no layout, stock, employee, or phase state changed.
- `StoreLayoutManager.RefreshLayout()` runs from multiple public methods that are called by visual controllers.

## Repeated Signal/Event Connections
- `UIManager.WireRuntimeActions()` is protected by `_runtimeActionsWired`; this prevents most duplicate button/signal handlers.
- The task checklist connection is protected by `_taskChecklistWired`.
- Keep any new signal wiring inside existing guarded paths.

## Unnecessary Per-Frame Calculations
- Visual actor movement belongs in `_Process`, but pathfinding, list rebuilds, and full UI refreshes should not.
- HUD/progress can update on a short interval or when key state changes; picker/list rebuilds should happen on explicit actions, phase changes, load, and slower business refreshes.

## Pathfinding and Collision Bottlenecks
- Paths are recalculated only when actor target changes or a path completes, which is good.
- `BuildPath()` still rebuilds layout, obstacles, navigation nodes, and allocates collections each call.
- Furniture/shelf purchase changes obstacles; actor paths should be invalidated once after layout-affecting purchases/load rather than recalculated continuously.

## UI Menus That Feel Slow or Awkward
- Price menu: target must be explicit, confirm must close the menu, cancel/Escape must clear the target.
- Save/load: loading must close all popups and leave gameplay state playable.
- Shelf stocking: selected shelf/product should remain stable across refreshes where possible.
- Runtime tabs currently open full popups predictably; keep modal close behavior consistent through confirm, close buttons, Back/Escape.

## Proposed Minimal Fixes
- Throttle `TickManager` driven UI refreshes during business and add a forced-refresh path for direct user actions.
- Split refresh into cheap frequent HUD/progress updates and heavier list/shop/visual refreshes only when state signatures change.
- Cache layout and obstacle state in `StoreLayoutManager`; rebuild only when shop size or shelf/furniture signature changes.
- Add path invalidation hooks to customer/employee visual controllers and call them after furniture/shelf layout changes and load.
- Clear price target in all stale-menu paths: close price popup, hide all popups, load game, reset/main menu.
- Remove noisy production `GD.Print` calls from common button handlers.
- Add regression checks for throttled refresh support, price target clearing, path invalidation, and slot-based load flow.
