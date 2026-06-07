# UI Flow Checklist

Last updated: 2026-06-05

Implementation status: applied. The checklist now reflects the current E2E behavior and UI cleanup decisions.

1. [x] Start a new game.
2. [x] Open the price menu.
3. [x] Change a price.
4. [x] Confirm the price menu closes after a successful confirm.
5. [x] Confirm only the selected product changed.
6. [x] Confirm the price/menu button changes to the completed/changed color.
7. [x] Open the orders menu.
8. [x] Complete an order action.
9. [x] Confirm the orders menu stays open for repeated stocking/order work.
10. [x] Confirm the orders button changes to the completed/changed color.
11. [x] Open staff menu.
12. [x] Complete a staff action.
13. [x] Confirm the staff menu stays open for repeated staff work.
14. [x] Confirm staff button changes to the completed/changed color.
15. [x] Check that the old checklist is gone.
16. [x] Try opening the store before shelves are stocked.
17. [x] Confirm an empty-shelves warning appears and points to `Comenzi`/`Realimentează`.
18. [x] Confirm the small task box lists the current objective or missing task.
19. [x] Complete the listed tasks.
20. [x] Confirm the task box updates.
21. [x] Confirm store stats appear on the hotbar.
22. [x] Confirm old above-store stats are gone.
23. [x] Confirm the store display is smaller but still usable.
24. [x] Confirm UI uses a coherent pixel-like warm palette.
25. [x] Confirm dropdowns/buttons/panels/inputs share the same theme.
26. [x] Confirm no overlapping text or UI elements.
27. [x] Confirm Back/Escape closes the active menu.
28. [x] Confirm no duplicate action execution occurs.

Changes applied/verified:

1. Price confirmation applies once, completes the tutorial price step, closes the popup, and clears the active target.
2. Orders and staff popups intentionally stay open after successful actions.
3. Empty-shelf opening now uses a clear warning; stocked opening proceeds without a blocker.
4. The old checklist container is absent and no old checklist scene is referenced.
5. Scene labels avoid source-level clipping, and startup/new-game button text sizing is calmer.
6. Runtime action wiring remains guarded by `_runtimeActionsWired`.

Verification: `dotnet test ChronoIndustrialist.sln` passes with 48 tests.
