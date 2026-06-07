Known issues discovered during release prep

1) Placeholder/test features still active
- Files: [Core.Simulation/Logic/GameManager.cs](Core.Simulation/Logic/GameManager.cs), [client.godot/Visuals/Store/StoreFurnitureVisualController.cs](client.godot/Visuals/Store/StoreFurnitureVisualController.cs)
- Impact: Placeholder visuals and `TriggerPlaceholderEvent` may expose non-final assets or test workflows in public builds.

2) Noisy runtime error print
- File: [client.godot/Visuals/UIManager.cs](client.godot/Visuals/UIManager.cs#L490)
- Impact: `GD.PrintErr` reports missing LeftMenu. Not a blocker but noisy; better to log at debug level or guard behind a development flag.

3) Absolute local paths in documentation
- File: [TESTING.md](TESTING.md#L13)
- Impact: `TESTING.md` references a user-local Godot binary path (/home/olga/...). Replace with `GODOT_BIN` env var or instruct users to supply their Godot executable path.

4) Editor-generated absolute paths in build artifacts
- Paths in `obj/` and `.sourcelink.json` include `/home/olga/...`. These are build artifacts and expected; they do not affect exported game, but should not be committed to release tarballs if you want reproducible builds.

5) Potential stale UI state on Load
- Notes in audits: `OPTIMIZATION_AND_FLOW_AUDIT.md` and `E2E_PLAYER_EXPERIENCE_AUDIT.md` describe cases where loading a save can leave stale UI targets (price editor, order dialogs). Tests cover some cases, but recommend manual verification in exported builds.

6) Placeholder text fields in scenes
- Files: [client.godot/Scenes/NewGameSetupPanel.tscn](client.godot/Scenes/NewGameSetupPanel.tscn#L50), [client.godot/Scenes/UIRoot.tscn](client.godot/Scenes/UIRoot.tscn#L766)
- Impact: Visible placeholder_text properties exist; verify translations and replace if needed.

Actionability / Workarounds
- Remove or feature-flag placeholder APIs before public release.
- Replace hard-coded doc paths with environment variable instructions.
- Add a short Godot export smoke test and run it on each target platform.

Recent fixes (June 2026)

- First-run tutorial overlay (blocking/oversized): FIXED
	- Symptom: tutorial popup could open immediately after starting a game and grow beyond viewport, blocking UI.
	- Outcome: the automatic blocking tutorial is now disabled by default and will not be shown automatically on new games.
	- Files changed: [client.godot/Visuals/UIManager.cs](client.godot/Visuals/UIManager.cs)

- Modal overlay visibility when leaving startup mode: MITIGATED
	- Symptom: Refresh logic previously forced startup-mode changes which unconditionally hid the `_modalRoot`, breaking modal workflows.
	- Outcome: `Refresh()` no longer forces `SetStartupMode(false)`; modal overlay visibility is preserved and updated via `RefreshModalOverlayVisibility()`.
	- Files changed: [client.godot/Visuals/UIManager.cs](client.godot/Visuals/UIManager.cs)

- `LoopManager` missing sibling `TickManager` reference: FIXED (self-wiring + logging)
	- Symptom: fresh scene loads could miss an editor-assigned `TickManager` reference and skip T=0 baseline capture.
	- Outcome: `LoopManager` now self-wires `../TickManager` when the exported reference is not assigned and logs a clear error if still missing.
	- Files checked: [client.godot/Loop/LoopManager.cs](client.godot/Loop/LoopManager.cs)

- `StoreLayoutManager.BuildFallbackPath` safety: FIXED
	- Symptom: fallback path logic could return a direct final target after path discovery failed, allowing movement across obstacles.
	- Outcome: fallback now returns the safe start point when no unobstructed anchor-based path exists.
	- Files changed: [client.godot/Visuals/Store/StoreLayoutManager.cs](client.godot/Visuals/Store/StoreLayoutManager.cs)

- MSBuild solution-level parallelism sensitivity (Godot SDK): MITIGATED
	- Symptom: root solution restore/build was sensitive to MSBuild solution-level parallelism with the Godot SDK project.
	- Outcome: `Directory.Solution.props` sets `BuildInParallel=false` and `RestoreBuildInParallel=false` to avoid parallelism issues during restore/build.
	- Files changed: [Directory.Solution.props](Directory.Solution.props)

Validation

- Unit tests: `dotnet test Core.Simulation.Tests/Core.Simulation.Tests.csproj` — all tests passed locally (47/47).
- Manual checks: modal/tour behavior and T=0 capture verified in editor-scoped runs.

Notes

- Remaining items in the top list (placeholder/test features, noisy GD.PrintErr, absolute doc paths, save-load UI state, placeholder scene text) still need manual verification or follow-up changes before public release. The placeholders and hard-coded paths are still present and should be addressed as part of an immediate release checklist.

If you want, I can open a branch and create a PR with these changes, or continue addressing the remaining items (replace PrintErr, sanitize placeholder text, update docs to use env vars, and add an export smoke test).
