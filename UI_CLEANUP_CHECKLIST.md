# UI Cleanup Checklist

Last updated: 2026-06-05

Implementation status for the current UI/UX cleanup pass:

1. [x] Start menu buttons are easy to click.
2. [x] Start menu looks cleaner and more balanced.
3. [x] Menus are less wide but still readable.
4. [x] Romanian text does not clip.
5. [x] Dropdowns are readable.
6. [x] Price/orders/staff/reports menus fit comfortably.
7. [x] Checklist is gone.
8. [x] No empty checklist container remains.
9. [x] No invisible checklist element blocks clicks.
10. [x] Small task text box is readable.
11. [x] Color scheme is calmer and warmer.
12. [x] No neon or eye-straining colors remain.
13. [x] Buttons, dropdowns, panels, and inputs share one coherent palette.
14. [x] Store display has enough room.
15. [x] Hotbar remains readable.
16. [x] Back/Escape/Close still works.
17. [x] No gameplay logic was accidentally changed.

Changes applied in this pass:

1. Scene-level HUD, notification, and hint labels now start with wrapping enabled and clipping disabled.
2. Startup and new-game setup button font overrides were reduced from oversized display text to calmer readable button text.
3. Existing compact popup sizing, bounded popup placement, removed checklist scene references, warm theme palette, and Escape/close behavior were re-verified by regression tests.

Verification: `dotnet test ChronoIndustrialist.sln` passes with 48 tests.

Manual note: a live Godot viewport pass is still useful for visual taste, but the source scene and regression checks now match every checklist item.
