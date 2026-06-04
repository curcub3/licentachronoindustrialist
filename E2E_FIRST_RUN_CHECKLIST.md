# E2E First Run Checklist

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

## 5. Open Store

Action: Press `Deschide`.

Expected UI: Business phase begins, progress bar appears, store view remains readable.

Expected result: Customer flow starts slowly in Relaxed mode.

Failure signs: Harsh penalties before first sale, no visible customer path, no hint if shelves are empty.

## 6. Watch First Customer Enter

Action: Observe the store view.

Expected UI: Customer appears at entrance and moves toward shelves.

Expected result: Customer browses before checkout.

Failure signs: Customer teleports with no context, walks through shelves/walls, or never appears.

## 7. Complete Checkout

Action: Wait for the first customer to reach the cashier.

Expected UI: Queue/cashier state is visible.

Expected result: Checkout completes and feedback appears, such as `Casă: client servit. Venit +...`.

Failure signs: No checkout feedback, money does not change, customer gets stuck.

## 8. Confirm Money Increases

Action: Compare HUD projected money/profit after checkout.

Expected UI: HUD/report shows revenue impact.

Expected result: Money or daily revenue increases from the sale.

Failure signs: No visible cause/effect for money earned.

## 9. Confirm Objective Advances

Action: Read checklist after first customer.

Expected UI: `Servește primul client` is complete and a new objective is visible.

Expected result: Next objective is about buying/placing a shelf or hiring when appropriate.

Failure signs: Checklist does not update or shows unrelated advanced systems first.

## 10. Confirm Reputation Feedback

Action: Create or observe a queue/stock issue, or serve customers successfully.

Expected UI: Reputation feedback explains the cause in Romanian.

Expected result: Examples: `Reputație -1: clienții au așteptat prea mult la casă.` or `Reputație +...: client servit cu succes.`

Failure signs: Reputation changes silently or with no cause.

## 11. Continue Until 5 Customers Served

Action: Keep shelves stocked and let customers buy.

Expected UI: Objective `Servește 5 clienți` appears and completes.

Expected result: Relaxed mode stays forgiving; reputation does not collapse during learning.

Failure signs: Customer pressure overwhelms the player in the first minutes.

## 12. Complete First Day

Action: Let business finish, open/read report, advance to next day.

Expected UI: Report explains sales, profit, stockouts, queue, reputation, and next steps.

Expected result: `Finalizează prima zi` completes or is clearly achievable.

Failure signs: Player cannot tell why the day succeeded/failed or what to do next.
