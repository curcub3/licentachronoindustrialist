# Player Experience Audit

Last updated: 2026-06-05

Implementation status: the first-run path now matches the full E2E audit set. New games default to `Relaxat`, Relaxed mode starts with empty shelves, the first tutorial teaches stocking and one explicit price edit before opening, empty-shelf opening attempts explain the stocking step, and regression tests cover the path.

## 1. First Screen

The first screen is the startup menu with `Joc Nou`, `Încarcă Jocul`, `Opțiuni`, and `Ieșire`.

## 2. First Action Expected

The game expects the player to press `Joc Nou`, choose a store name, difficulty, and run duration, then confirm the setup.

## 3. Is That Obvious?

Yes for starting a new game. The important part is that the new-game setup and core default settings now use `Relaxat`, so a first-time player lands in the guided path instead of a harder mode.

## 4. First 10 Player Decisions

1. Press `Joc Nou`.
2. Keep or edit the store name.
3. Leave `Relaxat` selected or choose another difficulty.
4. Confirm the run duration.
5. Start the game.
6. Read the first objective on the HUD.
7. Follow the tutorial prompt to open `Comenzi`.
8. Refill a shelf.
9. Open `Prețuri` and set one product price.
10. Open the store.

## 5. Where The Player Can Get Stuck

- Shelf stocking is now introduced by the first tutorial step and the task box.
- The task box now prioritizes the current early objective instead of showing several later goals at once.
- Opening the store with no stocked shelves now shows a Romanian warning that points to `Comenzi` and `Realimentează`.
- Save/load and purchase failures now use specific Romanian feedback where the UI can identify the cause.

## 6. Where The Game Gives No Feedback

- Empty shelves have a direct hint before opening.
- Queue pressure has an immediate reputation explanation.
- Checkout has a visible money/revenue message.
- Invalid placement or failed purchase gives a specific reason when available.
- Save/load uses Romanian success/failure messages.

## 7. Mechanics Introduced Too Early

Suppliers, staff management, events, and reports are still available early, but the guided tutorial now keeps attention on the core loop plus one safe price edit: stock a shelf, set one explicit product price, open the store, watch a customer, complete checkout, and read why money or reputation changed.

## 8. Unclear UI Elements

- `Comenzi` is the first important panel, because it contains shelf stocking and orders.
- The checklist must reveal only the current objective and completed steps.
- The top HUD must show day, money, reputation, and a short next-step hint.

## 9. Failure States Without Enough Explanation

- Reputation loss from empty shelves or long queues must always name the cause.
- Failed purchases and invalid placements must say why they failed.
- Load failures must not look like a broken UI.
- Early cash pressure must not trap the player before the first sale.

## 10. Smallest Fix Set Required

1. Keep `Relaxat` as the default first-run difficulty.
2. Start Relaxed runs with empty shelves so the first stocking step is real.
3. Keep the tutorial and checklist on a single guided path.
4. Show short Romanian feedback for money, reputation, checkout, shelf, placement, hiring, and save/load events.
5. Use a forgiving early grace period and recovery support in Relaxed mode only.
6. Keep the save/load flow slot-based inside the game, not a folder picker.
7. Validate the path with a manual first-run checklist.

## 11. Changes Applied

1. `GameStartSettings.Default` now uses `GameDifficulty.Relaxed`.
2. Relaxed default starts with empty shelves and preserves the forgiving early grace/recovery behavior.
3. The first-run tutorial is enabled by default again and starts after `Joc Nou`.
4. The tutorial introduces one explicit price edit after stocking, then follows open, observe, checkout, money, reputation, objective, and report.
5. The early task box prioritizes the current onboarding objective during the first three days.
6. Opening with no stocked shelves now shows an explicit Romanian warning pointing to `Comenzi` and `Realimentează`.
7. Failed startup loads clear the temporary session and return cleanly to startup.
8. Regression coverage was updated for the default Relaxed path, tutorial flow, objective order, and adjusted Relaxed payroll reporting.
9. Customer visual queue indexing and sales-associate focus-point updates were optimized to reduce per-frame allocation/sorting.

Verification: `dotnet test ChronoIndustrialist.sln` passes with 48 tests.

## 12. Potential Follow-Up Updates

1. Run the manual first-run checklist in the Godot client to confirm the tutorial popup positioning and first-sale visuals feel good at runtime.
2. Consider adding a small visual pulse on the `Comenzi` button during the first stocking objective.
3. Consider storing the tutorial-disabled choice in user settings if players should be able to keep it off across sessions.
