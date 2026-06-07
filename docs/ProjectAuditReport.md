# Project Audit Report

Date: 2026-06-05

## Verification Summary

- `dotnet build ChronoIndustrialist.sln`: passed, 0 warnings, 0 errors.
- `dotnet test ChronoIndustrialist.sln`: passed, 48/48 tests.
- Static `res://` resource reference scan: passed, no missing direct resource paths found.
- Static direct UI localization key scan against `client.godot/Localization/ro.csv`: passed, no missing direct `T(...)`/`TF(...)` keys found.
- Godot editor/headless launch and screenshot validation could not be automated in this shell because no `godot`, `godot4`, or `godot4.6` executable is available on `PATH`.

## 1. Critical Issues Found

- Obsolete GDScript task checklist assets were still present after the UI moved to the runtime task box. They were unused by the current scene, but they created accidental reactivation risk and stale UI drift.
- The runtime event popup still exposed a manual event trigger path during normal play. It is now gated behind `EnableLayoutDebug` and ignored unless layout debug is explicitly enabled.
- Internal runtime event code still used `Placeholder` naming for an intentional debug event path. It has been renamed to `TriggerDebugEvent` to make the production/debug boundary clear.
- Store furniture code still used placeholder naming for non-shelf purchase visuals. The behavior is preserved, but the implementation now treats them as simple furniture visuals; shelf visuals continue to use original shelf assets.
- Prior checklist/audit issues in this worktree were verified as addressed: relaxed guided default, empty starting shelves for relaxed mode, price tutorial restoration, popup bounds contracts, stale price target clearing on load, customer focus allocation cleanup, and shelf-obstacle pathing contracts.

## 2. Build Errors Fixed

- No current C# build errors were found during this pass.
- The previous stale report warning that plain `dotnet build ChronoIndustrialist.sln` fails is no longer true in this environment.
- The solution builds successfully with the Godot C# client, simulation core, and test project included.

## 3. Runtime Errors Fixed

- Debug-only event triggering is now hidden and disabled unless `EnableLayoutDebug` is true.
- The debug event handler now exits immediately when debug mode is disabled.
- Save/load coverage verifies store name, difficulty, cash, reputation, satisfaction, furniture purchases, purchased shelves, hired employees, pending orders, active business phase, and business runtime state.
- Invalid, missing, and corrupt save behavior is covered so failed loads do not mutate the active game state.
- Placeholder/runtime naming was cleaned up without changing event behavior.

## 4. UI Issues Fixed

- Removed obsolete task checklist scenes/scripts from the repository so the active UI has one task-progress source.
- Regression coverage now asserts the runtime task box is used and old checklist assets stay removed.
- The runtime test-event button is still present in the scene for debug use, but normal UI refresh hides and disables it unless layout debug is enabled.
- Existing UI cleanup remains verified by tests: compact popup sizes, no scene-level `clip_text = true`, reduced startup/new-game font overrides, bounded popup fitting, startup/new-game Romanian flow, and menu progress indicators.
- Automated viewport screenshots at 1280x720, 1600x900, and 1920x1080 remain blocked until Godot is available in the environment.

## 5. Store Visualization Issues Fixed

- Original shelf visuals remain in use through `CreateShelfVisualFromOriginal` and template duplication.
- No generated shelf placeholder references remain in runtime store visualization code.
- Purchased shelf catalog items create simulation shelves with the expected product, capacity, stock, and display type.
- Customer and employee movement contracts still route through `BuildPath(...)` and avoid direct final-target snapping.
- Store layout tests verify solid shelf obstacle contracts, dynamic shelf slot usage, customer browse/queue/register points, and employee name/role labels.
- Non-shelf purchases render through simple furniture visuals. Final art for those upgrades remains technical debt, not a blocking shelf-visualization issue.

## 6. Files Modified

- `Core.Simulation.Tests/Program.cs`
- `Core.Simulation.Tests/RegressionCoverageTests.cs`
- `Core.Simulation/Data/GameData.cs`
- `Core.Simulation/Logic/EventManager.cs`
- `Core.Simulation/Logic/GameManager.cs`
- `Core.Simulation/Logic/SimulationLoop.cs`
- `E2E_FIRST_RUN_CHECKLIST.md`
- `E2E_PLAYER_EXPERIENCE_AUDIT.md`
- `FLOW_AND_OPTIMIZATION_CHECKLIST.md`
- `GAMEPLAY_ONBOARDING.md`
- `OPTIMIZATION_AND_FLOW_AUDIT.md`
- `PLAYER_EXPERIENCE_AUDIT.md`
- `UI_CLEANUP_CHECKLIST.md`
- `UI_FLOW_CHECKLIST.md`
- `client.godot/Scenes/NewGameSetupPanel.tscn`
- `client.godot/Scenes/UIRoot.tscn`
- `client.godot/Visuals/Store/CustomerVisualController.cs`
- `client.godot/Visuals/Store/StoreFurnitureVisualController.cs`
- `client.godot/Visuals/UIManager.cs`
- `docs/ProjectAuditReport.md`

## 7. Files Removed

- `client.godot/Assets/UI/components/TaskChecklist.tscn`
- `client.godot/Assets/UI/components/TaskChecklistItem.tscn`
- `client.godot/Assets/UI/scripts/task_checklist.gd`
- `client.godot/Assets/UI/scripts/task_checklist.gd.uid`
- `client.godot/Assets/UI/scripts/task_checklist_item.gd`
- `client.godot/Assets/UI/scripts/task_checklist_item.gd.uid`

## 8. Remaining Warnings

- Godot is not available on `PATH`, so actual project launch, main-scene load, resource load through the engine, and viewport screenshot checks could not be executed here.
- UI overlap status is covered by scene/static contracts and regression tests, but not by fresh screenshot comparison in this pass.
- Full interactive customer/employee movement was not observed in the editor during this pass because the Godot runtime could not be launched.
- The worktree contains pre-existing untracked files: `KNOWN_ISSUES.md`, `RELEASE_CHECKLIST.md`, and `doc_metadata.json`.

## 9. Remaining Technical Debt

- Add Godot 4.6.x headless/editor availability to local scripts or CI so launch and screenshot smoke tests can run consistently.
- Replace simple non-shelf furniture visuals with final art assets when those assets exist.
- Add an engine-level smoke test that starts a relaxed game, stocks a shelf, sets a price, opens the shop, advances ticks, saves, loads, and asserts no runtime errors.
- Add screenshot regression checks for 1280x720, 1600x900, and 1920x1080.
- Split the current dirty worktree into reviewable changes before release hardening.

## 10. Recommendations For MVP Completion

- Treat `dotnet build ChronoIndustrialist.sln` and `dotnet test ChronoIndustrialist.sln` as required gates.
- Add a Godot launch gate once the executable is installed: project load, `Scenes/GameMain.tscn` load, first-run startup, new-game start, save, load, and business tick smoke.
- Keep debug-only flows behind explicit exported debug flags.
- Keep shelf collision and purchased-shelf visual contracts under regression coverage as store layout expands.
- Prioritize final non-shelf store assets only after the current compile, save/load, tutorial, and shop loop remain stable under automated smoke checks.
