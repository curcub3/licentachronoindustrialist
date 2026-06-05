# Flow and Optimization Checklist

Use this manual pass after a clean build.

1. Launch the game.
2. Start a new Easy/Relaxat game.
3. Confirm the first objective is visible.
4. Stock a shelf.
5. Set a product price.
6. Confirm the price menu closes and affects only one product.
7. Watch a customer enter, shop, queue, pay, and leave.
8. Confirm money/reputation feedback is visible and understandable.
9. Buy/place furniture.
10. Confirm furniture becomes visible and solid.
11. Confirm customers/employees route around it.
12. Open/close all major menus using confirm, cancel, back, and Escape.
13. Confirm no stale menu affects the wrong target.
14. Save the game.
15. Load the game.
16. Confirm the loaded state is playable and menus are clean.
17. Play for at least 10 minutes on Easy/Relaxat.
18. Confirm no obvious frame drops, duplicate handlers, or UI clutter.
19. Confirm Normal/Hard were not accidentally retuned.
20. Confirm no new visual theme or color scheme was introduced.

## Focus Areas
- Price menu: selected product label is explicit, confirm applies once, close/Escape clears target.
- Shelf stocking: selected shelf/product stay clear after list refreshes.
- Runtime HUD: money, reputation, objective, queue pressure, and day progress remain visible.
- Save/load: slot list opens, load closes menus, gameplay resumes without stale modal state.
- Furniture purchase: visual appears immediately, actor paths update once after the layout change.
- Business phase: HUD stays responsive while heavy UI/list refreshes are throttled.
