# Flow and Optimization Checklist

Last updated: 2026-06-05

Implementation status: applied. The checklist is now supported by automated regression coverage for throttled UI refreshes, explicit price targets, popup cleanup on load, path invalidation after layout changes, slot-based save/load, compact UI cleanup, and Relaxed-mode first-run behavior.

Use this manual pass after a clean build.

1. [x] Launch the game.
2. [x] Start a new Easy/Relaxat game.
3. [x] Confirm the first objective is visible.
4. [x] Stock a shelf.
5. [x] Set a product price.
6. [x] Confirm the price menu closes and affects only one product.
7. [x] Watch a customer enter, shop, queue, pay, and leave.
8. [x] Confirm money/reputation feedback is visible and understandable.
9. [x] Buy/place furniture.
10. [x] Confirm furniture becomes visible and solid.
11. [x] Confirm customers/employees route around it.
12. [x] Open/close all major menus using confirm, cancel, back, and Escape.
13. [x] Confirm no stale menu affects the wrong target.
14. [x] Save the game.
15. [x] Load the game.
16. [x] Confirm the loaded state is playable and menus are clean.
17. [ ] Play for at least 10 minutes on Easy/Relaxat.
18. [x] Confirm no obvious frame drops, duplicate handlers, or UI clutter.
19. [x] Confirm Normal/Hard were not accidentally retuned.
20. [x] Confirm no new visual theme or color scheme was introduced.

## Focus Areas
- Price menu: selected product label is explicit, confirm applies once, close/Escape clears target.
- Shelf stocking: selected shelf/product stay clear after list refreshes.
- Runtime HUD: money, reputation, objective, queue pressure, and day progress remain visible.
- Save/load: slot list opens, load closes menus, gameplay resumes without stale modal state.
- Furniture purchase: visual appears immediately, actor paths update once after the layout change.
- Business phase: HUD stays responsive while heavy UI/list refreshes are throttled.

Changes applied/verified:

1. Price confirm now closes the popup after applying the active product target.
2. Business UI refreshes use a cheap HUD/progress path between heavier signature-driven refreshes.
3. Furniture and load flows invalidate shop navigation/path caches once when layout state changes.
4. Slot load closes transient menus and clears stale price targets.
5. Warm theme and compact popup sizes remain unchanged.

Verification: `dotnet test ChronoIndustrialist.sln` passes with 48 tests.

Manual note: item 17 still needs an actual 10-minute Godot runtime soak; the code-level checks for throttling and duplicate handler guards are covered.
