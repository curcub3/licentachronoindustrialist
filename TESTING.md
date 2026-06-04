# ChronoIndustrialist Testing Guide

Run these checks before merging gameplay, UI, save/load, pathing, employee, or furniture changes.

## Automated Validation

From the repository root:

```bash
dotnet test ChronoIndustrialist.sln
dotnet build ChronoIndustrialist.sln -m:1
dotnet build client.godot/client.godot.csproj
/home/olga/Applications/Godot/Godot_v4.6.3-stable_mono_linux_x86_64/Godot_v4.6.3-stable_mono_linux.x86_64 --headless --path client.godot --quit
```

The Xunit suite covers:

- New game settings and initial store state.
- Relaxed, normal, and hard difficulty state where practical.
- Easy/relaxed reputation loss and recovery being softer than normal without changing normal/hard expectations.
- Save/load round trips for money, reputation, store name, difficulty, run duration, furniture, employees, orders, runtime phase, and purchased catalog IDs.
- Edited, missing, corrupt, and structurally invalid save behavior.
- Save slot flow using repo-local `Saves/slot_N.json` files instead of an OS file picker.
- Romanian startup/new-game/load UI text and validation for empty store names.
- Dropdown styling staying tied to the established button theme.
- Static viewport-bound popup/layout contracts for menus and modal panels.
- Pathfinding around solid simulation furniture tiles.
- Godot scene references resolving to existing files.
- Customer/employee visual controllers using pathing instead of direct teleport-to-target assignment.
- Employee name/role labels staying below the visual marker.
- Purchased shelf visuals reusing original scene controls and avoiding generated shelf assets.

## Latest Validation Log

Date: 2026-06-04

```bash
dotnet test ChronoIndustrialist.sln
```

Result: passed. `34` tests passed, `0` failed, `0` skipped.

```bash
dotnet build ChronoIndustrialist.sln -m:1
```

Result: passed. Build succeeded with `0` warnings and `0` errors.

```bash
dotnet build client.godot/client.godot.csproj
```

Result: passed. Build succeeded with `0` warnings and `0` errors.

```bash
/home/olga/Applications/Godot/Godot_v4.6.3-stable_mono_linux_x86_64/Godot_v4.6.3-stable_mono_linux.x86_64 --headless --path client.godot --quit
```

Result: passed. Godot initialized MessagePack AOT resolvers and printed `UIManager: runtime buttons wired.`

Note: if `dotnet test` fails in the sandbox with an MSBuild named-pipe permission error, rerun the same command with the approved elevated `dotnet test` permission.

Note: use `-m:1` for solution builds. In this environment, `dotnet build ChronoIndustrialist.sln` can fail during solution-level parallel orchestration of the Godot project without reporting compiler errors, while the serialized solution build and direct client build pass.

## Manual E2E Checklist

### A. Main Menu Flow

1. Launch the game at `1280x720`.
2. Confirm the first screen shows Romanian controls: `Joc nou`, `Încarcă joc`, `Setări`, `Ieșire`.
3. Press `Joc nou` and confirm the new game setup panel opens.
4. Return with `Înapoi`.
5. Press `Încarcă joc` and confirm a save slot popup opens. No generic OS file or folder picker should appear.
6. Press `Setări` and confirm the settings/options popup opens if present.
7. Press `Ieșire` and confirm the existing quit confirmation/behavior remains unchanged.

### B. New Game Setup

1. Confirm the setup menu includes store name input, difficulty selector, and run duration selector.
2. Clear the store name and press `Confirmă`; the game should not start and should show Romanian validation.
3. Confirm difficulty choices are visible/selectable: `Relaxat`, `Normal`, `Greu`.
4. Confirm run duration choices are visible/selectable.
5. Start a relaxed run and verify the HUD opens in `Administrare`, the store name is preserved, starting cash matches relaxed mode, and initial employees/shelves are present.
6. Check visible player-facing text is Romanian. Code identifiers/classes/methods should remain English.

### C. Load/Save Flow

1. Save a game to each available slot and confirm files appear under repo-local `Saves/slot_N.json`.
2. From the main menu, open `Încarcă joc` and confirm available saves appear as slots/list entries.
3. Load a valid save and verify money, reputation, store name, difficulty, run duration, furniture, employees, phase, orders, and relevant gameplay state restore.
4. Temporarily move or remove the `Saves` folder and confirm the load menu handles it safely.
5. Replace a slot file with invalid JSON and confirm the game does not crash and shows Romanian feedback such as `Salvare ilizibilă` or `Încărcarea a eșuat`.

### D. UI Layout Regression

Check `1280x720`, `1600x900`, and a narrow window around `960x540`.

1. Main menu, new game menu, load menu, HUD, pause/menu flow, task list, store panels, and dropdowns stay inside the viewport.
2. Text does not overlap other controls.
3. Buttons and labels are not outside screen bounds.
4. No duplicated search bars or redundant controls appear.
5. Button labels do not contain tooltip-like helper phrases.
6. Dropdown item colors, selected colors, hover colors, fonts, and spacing match the established menu/button style.

### E. Store Simulation

1. Start business and confirm customers spawn at the entrance.
2. Customers should walk toward shelves/stands before buying.
3. Buying customers should then move to the cashier queue/register.
4. Customers should not walk through shelves, stands, cashier/register, walls, or purchased furniture.
5. Customer movement should look continuous and natural. Teleporting is a regression unless intentionally triggered by an existing system state.

### F. Employee Simulation

1. Confirm employees are visible in the store.
2. Confirm each employee has a name and Romanian role label below the visual marker.
3. During business, cashiers should work around the register/queue, stockers should move between storage and shelves, and sales/manager roles should patrol/help.
4. Employees should not pass through shelves, register, walls, or purchased furniture.

### G. Furniture And Shelf Asset Consistency

1. Buy each shelf catalog item.
2. Confirm bought shelves appear in the store and use the original existing shelf visuals/style.
3. Confirm no newly generated shelf visual is used.
4. Confirm bought furniture is visible.
5. Confirm bought shelves/furniture affect collision/pathfinding and customers/employees route around them.
6. After testing, run `rg -i "generated|GeneratedShelf|Assets/UI/generated" client.godot` and confirm no generated shelf assets or references were added.

### H. Easy Mode Reputation

1. Start one relaxed run and one normal run.
2. Trigger comparable stockout/queue pressure in both.
3. Confirm relaxed mode loses reputation more slowly and recovers more easily.
4. Continue early gameplay long enough to confirm relaxed reputation does not collapse too quickly.
5. Spot-check normal and hard mode starting money, demand, and cost behavior for accidental changes.

## Remaining Manual Risk

- Full visual overlap detection still needs real viewport inspection or screenshot-based tooling.
- Customer and employee collision/pathing are partly covered by static/pathfinding tests, but real-time movement should still be watched manually after layout changes.
- Save compatibility with older external saves should be tested manually when the save schema changes.
