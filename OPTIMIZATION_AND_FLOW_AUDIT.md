# Optimization and Flow Audit

Last updated: 2026-06-05

Implementation status: applied. The audit’s minimal fixes are now covered by code changes and regression tests: throttled UI refreshes, heavy/light refresh split, cached layout/obstacle state, path invalidation hooks, price target cleanup, slot-based load cleanup, and reduced per-frame customer visual allocations.

## Current Performance Risks
- `TickManager._Process` throttles full business UI refreshes and uses a cheap HUD/progress refresh between full passes.
- `StoreLayoutManager` caches layout, obstacle signatures, and static navigation anchors; furniture/load changes invalidate paths once.
- Customer queue indexing no longer sorts active actors per update.
- Employee sales-associate targeting reuses a customer focus list instead of allocating a new list each frame.
- Inventory totals and shelf summaries use repeated LINQ scans. This is low risk at current data sizes, but it should stay out of per-frame refresh paths.

## Confusing Flow Points
- Price editing needs one explicit product target. Existing code now tracks `_activePriceProductId`; the target must be cleared whenever the price popup closes or loading resets UI state.
- Save/load should stay slot-based. The current flow opens save slots instead of a generic file picker, but load should also close all open runtime menus and reset stale selected targets.
- New game tutorial and onboarding are visible, and the early sequence keeps the next concrete action visible: stock shelf, set price, open store, observe customer.
- Furniture purchase happens directly from the catalog rather than through a separate placement preview. Minimal safe fix is to make the purchased item visible immediately and force path/layout refresh once.

## Visual Clutter and Readability Issues
- HUD text is compact, wrapping is enabled, and scene-level clipping is disabled on key labels.
- Shop labels contain several lines of state. They are useful, but should be refreshed only when state changes or on a light throttle.
- Menus use established theme resources and should keep the same controls/colors. No new visual scheme is needed.
- Feedback should stay short and Romanian; price/shelf/furniture actions already notify, but stale modal state must not linger.

## Expensive Update Loops
- `UIManager.RefreshRuntimePanel()` uses full-refresh signatures so option lists and shop visuals are not rebuilt every light business update.
- `UIManager.RefreshShop2DView()` runs only during full refreshes.
- `StoreLayoutManager.RefreshLayout()` exits early when the shop size has not changed.

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

## Changes Applied

1. Price confirm closes the price popup and clears the active target after applying one product update.
2. Customer focus points are stored in a reusable list.
3. Queue index calculation now uses a direct sequence comparison loop instead of per-update LINQ sorting.
4. Regression coverage now guards the focus-list reuse and queue-index allocation fix.
5. Existing refresh throttling, path invalidation, and slot-load cleanup checks remain green.

Verification: `dotnet test ChronoIndustrialist.sln` passes with 48 tests.
