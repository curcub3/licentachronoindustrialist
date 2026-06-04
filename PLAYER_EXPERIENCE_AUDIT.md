# Player Experience Audit

## 1. First Screen

The first screen is the startup menu with `Joc Nou`, `Încarcă Jocul`, `Opțiuni`, and `Ieșire`.

## 2. First Action Expected

The game expects the player to press `Joc Nou`, choose a store name, difficulty, and run duration, then confirm the setup.

## 3. Is That Obvious?

Yes for starting a new game. The important part is that the new-game setup now defaults to `Relaxat`, so a first-time player lands in the guided path instead of a harder mode.

## 4. First 10 Player Decisions

1. Press `Joc Nou`.
2. Keep or edit the store name.
3. Leave `Relaxat` selected or choose another difficulty.
4. Confirm the run duration.
5. Start the game.
6. Read the first objective on the HUD.
7. Open `Comenzi`.
8. Refill a shelf.
9. Open the store.
10. Watch the first customer move from entrance to shelf to checkout.

## 5. Where The Player Can Get Stuck

- The player can stall if they do not notice that shelf stocking happens in `Comenzi`.
- The player can stall if the checklist shows too many later goals at once.
- The player can stall if the store opens with empty shelves and no hint says to stock first.
- The player can stall if a save/load or purchase failure is shown as a generic error.

## 6. Where The Game Gives No Feedback

- Empty shelves need a direct hint.
- Queue pressure needs an immediate reputation explanation.
- Checkout needs a visible money/revenue message.
- Invalid placement or failed purchase needs an exact reason.
- Save/load needs a Romanian success/failure message.

## 7. Mechanics Introduced Too Early

Prices, suppliers, staff management, events, and reports are still available early, but the guided path now keeps attention on only the core loop: stock a shelf, open the store, watch a customer, complete checkout, and read why money or reputation changed.

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

