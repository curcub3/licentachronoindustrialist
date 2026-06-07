# E2E First Run Checklist

Last updated: 2026-06-05

Implementation status: applied. The first-run sequence is now covered by regression tests and follows the E2E path: Relaxed default, empty shelves, stock first shelf, set first price with an explicit target, open the store, observe checkout feedback, then progress into shelf/staff/stability goals before finishing the first day.

Use this checklist in Godot with a fresh run. The tester should not need source code or outside explanation.

## 1. Launch Game

Action: Start the project.

Expected UI: Main menu shows `Joc nou`, `Încarcă joc`, `Setări`, `Ieșire`.

Expected result: No errors or folder picker.

Failure signs: Missing menu, English player-facing labels, load opens an OS file picker.

## 2. Start New Relaxed Game

Action: Press `Joc nou`, keep `Relaxat`, confirm.

Expected UI: Setup asks for store name, difficulty, and run duration.

Expected result: Game enters day 1 with HUD showing day, money, reputation, and a tutorial/objective.

Failure signs: No setup, unclear first objective, or the run does not start in `Relaxat`.

Status: Covered by `DefaultNewGameStartsInRelaxedGuidedMode` and startup UI regression checks.

## 3. Observe First Objective

Action: Read the HUD/checklist.

Expected UI: The visible objective is `Aprovizionează primul raft`.

Expected result: Hint says to open `Comenzi` and refill a shelf.

Failure signs: Objective is already completed, too many future tasks shown, or only generic text like “Administrează magazinul.”

## 4. Stock First Shelf

Action: Open `Comenzi`, select a shelf, use `Realimentează` or `Realimentează Toate Rafturile`.

Expected UI: Romanian feedback confirms products moved to the shelf.

Expected result: `Aprovizionează primul raft` completes and next objective appears.

Failure signs: No shelf controls, no confirmation, no objective update.

Status: Covered by onboarding objective completion tests.

## 5. Set First Price

Action: Open `Prețuri`, select one product, enter a valid price, and confirm.

Expected UI: The price popup clearly shows the selected product name.

Expected result: Feedback says `Preț actualizat pentru [produs].`, only that product changes, and the popup closes.

Failure signs: Product name is missing, the popup stays open after confirm, or another product changes.

Status: Implemented. Confirm applies to `_activePriceProductId`, completes the price tutorial step, closes the price popup, and clears stale target state.

## 6. Confirm Price Cancel And Errors

Action: Reopen `Prețuri`, change the text, press `Închide` or Escape, then reopen and try empty/non-number/negative/unreasonable input.

Expected UI: Cancel closes without changing the product. Invalid input shows `Introdu un preț valid.` Missing selection shows `Nu există produs selectat.`

Expected result: No crash, no stale target, and no second product is affected.

Failure signs: Invalid input crashes, cancel applies a price, or repeated opens apply to an old product.

Status: Covered by price validation and static price-menu binding regression checks.

## 7. Open Store

Action: Press `Deschide`.

Expected UI: Business phase begins, progress bar appears, store view remains readable.

Expected result: Customer flow starts slowly in Relaxed mode.

Failure signs: Harsh penalties before first sale, no visible customer path, no hint if shelves are empty.

Status: Covered by Relaxed grace and first-checkout feedback tests. Empty-shelf opening now shows an in-game Romanian warning.

## 8. Watch First Customer Enter

Action: Observe the store view.

Expected UI: Customer appears at entrance and moves toward shelves.

Expected result: Customer browses before checkout.

Failure signs: Customer teleports with no context, walks through shelves/walls, or never appears.

## 9. Complete Checkout

Action: Wait for the first customer to reach the cashier.

Expected UI: Queue/cashier state is visible.

Expected result: Checkout completes and feedback appears, such as `Casă: client servit. Venit +...`.

Failure signs: No checkout feedback, money does not change, customer gets stuck.

## 10. Confirm Money Increases

Action: Compare HUD projected money/profit after checkout.

Expected UI: HUD/report shows revenue impact.

Expected result: Money or daily revenue increases from the sale.

Failure signs: No visible cause/effect for money earned.

## 11. Confirm Objective Advances

Action: Read checklist after first customer.

Expected UI: `Servește primul client` is complete and a new objective is visible.

Expected result: Next objective is about buying/placing a shelf or hiring when appropriate.

Failure signs: Checklist does not update or shows unrelated advanced systems first.

## 12. Confirm Reputation Feedback

Action: Create or observe a queue/stock issue, or serve customers successfully.

Expected UI: Reputation feedback explains the cause in Romanian.

Expected result: Examples: `Reputație -1: clienții au așteptat prea mult la casă.` or `Reputație +...: client servit cu succes.`

Failure signs: Reputation changes silently or with no cause.

## 13. Buy Or Place Furniture

Action: In `Comenzi`, buy/place a shelf if enough money is available.

Expected UI: Feedback confirms the shop purchase or says `Nu ai suficienți bani.`

Expected result: No stale menu remains open unexpectedly, and bought shelves use existing shelf visuals.

Failure signs: Generic failure, invisible furniture, or a menu action affects the wrong product.

## 14. Continue Until 5 Customers Served

Action: Keep shelves stocked and let customers buy.

Expected UI: Objective `Servește 5 clienți` appears and completes.

Expected result: Relaxed mode stays forgiving; reputation does not collapse during learning.

Failure signs: Customer pressure overwhelms the player in the first minutes.

## 15. Save Game

Action: Open `Salv.`, choose a slot.

Expected UI: Slot menu stays inside the game UI.

Expected result: Feedback says the game was saved to that slot.

Failure signs: OS folder picker opens, no feedback, or slot text does not update.

## 16. Load Game

Action: Open `Încarcă`, choose the saved slot.

Expected UI: If no saves exist, the menu says `Nu există salvări.`

Expected result: The save loads, transient menus close, and the game returns to a playable state.

Failure signs: Old price/orders/staff popup remains open or the load leaves the player in a confusing state.

## 17. Complete First Day

Action: Let business finish, open/read report, advance to next day.

Expected UI: Report explains sales, profit, stockouts, queue, reputation, and next steps.

Expected result: `Finalizează prima zi` completes or is clearly achievable.

Failure signs: Player cannot tell why the day succeeded/failed or what to do next.

Status: Report, save/load, feedback, and onboarding order are covered by regression tests. A live Godot pass is still recommended for visual feel.

Verification: `dotnet test ChronoIndustrialist.sln` passes with 48 tests.
