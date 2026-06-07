Release checklist for ChronoIndustrialist

Basic validation
- **Build**: `dotnet build ChronoIndustrialist.sln -c Release` — Passed locally.
- **Tests**: `dotnet test Core.Simulation.Tests -c Release` — All tests passed (47/47).
- **Godot headless smoke**: Start with the project's Godot binary (see `TESTING.md`) using `--headless --path client.godot --quit` — headless start succeeded in this environment.

Critical items to fix before public release
- **Remove or gate placeholder/test features**: `TriggerPlaceholderEvent` and placeholder visuals are present and surfaced by tests and UI. See `Core.Simulation/Logic/GameManager.cs` and client visuals under `client.godot/Visuals/Store/StoreFurnitureVisualController.cs`.
- **Remove noisy debug prints**: `GD.PrintErr` in `client.godot/Visuals/UIManager.cs` reports missing LeftMenu at runtime; convert to non-noisy logging or guard behind development checks.
- **Avoid absolute local paths in docs**: `TESTING.md` contains a hard-coded path to a local Godot binary (/home/olga/...). Replace with an instruction to set `GODOT_BIN` or use a relative path.

Localization
- All player-facing text is in Romanian via `client.godot/Localization/ro.csv`. Confirm translators review copy.

Saves
- Saves are repo-local under `Saves/slot_N.json` and example saves exist. Ensure the `Saves` folder is included in release packaging or created at first run via `Directory.CreateDirectory` (code already does this per tests).

Release export checklist
- Ensure Godot export templates installed for target platforms.
- In Godot editor: set `run/main_scene` to `res://Scenes/GameMain.tscn` and export templates for Linux/Windows/macOS as required.
- Verify export by running exported binary and performing: New Game → play one day → Save → Quit → Re-open exported build → Load → confirm state restores.

Recommended follow-ups
- Add a committed Godot smoke test that starts headless, advances a few ticks, saves, loads, and asserts no runtime errors.
- Remove or flag debug/test helpers and placeholder assets.
- Replace absolute paths in docs with environment variables or instructions.

Files created during this check
- RELEASE_CHECKLIST.md (this file)
- KNOWN_ISSUES.md
