# E2E Player Experience Audit

Last updated: 2026-06-05

Implementation status: applied. The E2E first-run flow now teaches stocking, one explicit price edit, opening the store, checkout feedback, expansion/staff goals, and first-day reporting with slot-based save/load.

## Current Player Flow

1. Player launches the game and sees the startup menu.
2. `Joc nou` opens store setup: name, difficulty, duration.
3. A Relaxed first run starts in day 1 Management with empty shelves.
4. The HUD/task box and tutorial tell the player to stock the first shelf.
5. `Comenzi` contains product orders, shop purchases, shelf assignment, and shelf refill.
6. `Prețuri` contains product price editing.
7. `Deschide` enters the Business phase; customers browse shelves, queue, and generate revenue/reputation feedback.
8. Closing shows the daily report; save/load uses repo-local slots under `Saves/slot_N.json`.

## Where The Player Gets Stuck

- The first objective skipped price setting, so a player could stock shelves and open the store without learning where product pricing lives.
- Price editing was implicitly bound to the product picker at confirmation time. A refreshed picker or accidental selection change could apply the typed price to a different product than the one that opened the flow.
- The price popup did not close after confirmation, leaving stale mental state and encouraging accidental repeated edits.
- Closing or loading did not consistently clear transient menu target state.
- Escape/back behavior was not guaranteed to close the current modal before returning to gameplay.

## Unclear UI Interactions

- `Meniu Prețuri` did not state which product the input was editing.
- `Comenzi` mixes supplier orders, shop purchases, shelf assignment, and shelf refill, so feedback must name the affected target.
- Save/load was already slot based, but loading from an active run needed to leave the game in a clean playable state.

## Menus And Target Binding

- Price menu: needs one active product id, visible product name, focused input, validation, confirm close, cancel close, and stale target clearing.
- Shelf refill/assignment: uses one selected shelf and one selected product; feedback names the shelf and product id. The menu may stay open because repeated shelf operations are a normal management flow.
- Shop purchase: uses one selected catalog item and product picker only when buying a shelf. Failed purchase says if money is insufficient.
- Staff: uses one selected candidate/employee; feedback names the candidate or employee.
- Save/load: uses one selected slot; invalid slots are disabled and no OS picker is used.

## Fixes Applied

1. Added `Setează prețul unui produs` as a real onboarding objective after stocking the first shelf.
2. Added explicit price target state in `UIManager`: opening or selecting a product starts a single active price edit.
3. Added `RuntimePriceTargetLabel` to show `Produs selectat: [produs]`.
4. Confirm now updates only the active product, shows `Preț actualizat pentru [produs].`, closes the popup, and clears stale target state.
5. Cancel/close/Escape closes the price popup without changing data and clears stale target state.
6. Invalid, empty, negative, and unreasonable prices show `Introdu un preț valid.`
7. Missing target shows `Nu există produs selectat.`
8. Loading a save hides transient modals and refreshes the HUD/checklist.
9. Added regression coverage for price validation, onboarding order/completion, and static price menu binding contracts.
10. Restored the price tutorial step after stocking so the E2E path teaches pricing before the first store opening.
11. Kept the old checklist removed; the compact task box now shows only the current concrete action.

## Manual Follow-Up Checks

- Verify in Godot that the price input receives focus when `Prețuri` opens.
- Verify Escape closes the current modal first in startup setup, save/load, price, orders, staff, event, report, and confirmation flows.
- Verify Relaxed mode still starts with empty shelves and Normal/Hard behavior is unchanged.

Verification: `dotnet test ChronoIndustrialist.sln` passes with 48 tests.
