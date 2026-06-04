# Project Audit Report

Date: 2026-06-05

## 1. Critical issues found

- The first-run tutorial popup could open immediately after starting a game and grow far beyond the viewport, covering the runtime UI and making normal controls appear broken.
- Runtime refresh logic hid `_modalRoot` unconditionally when leaving startup mode, which could break popup-driven workflows and modal visibility state.
- `LoopManager` depended on an editor-assigned `TickManager` reference; fresh scene loads could miss the sibling `TickManager` and skip T=0 baseline capture.
- `StoreLayoutManager.BuildFallbackPath` could return a direct final target after path discovery failed, allowing a last-resort customer/employee movement segment to cross shelf obstacles.
- Root solution restore/build is sensitive to MSBuild solution-level parallelism with the Godot SDK project.

## 2. Build errors fixed

- Added `Directory.Solution.props` to disable parallel solution restore. `dotnet restore ChronoIndustrialist.sln` now succeeds.
- Verified the full C# solution builds with serialized MSBuild scheduling:
  - `dotnet build ChronoIndustrialist.sln -m:1`
  - Result: succeeded, 0 warnings, 0 errors.

Remaining build issue: plain `dotnet build ChronoIndustrialist.sln` still exits nonzero after restore with 0 warnings and 0 errors. Use `dotnet build ChronoIndustrialist.sln -m:1` until the Godot SDK/MSBuild solution scheduling issue is resolved.

## 3. Runtime errors fixed

- `LoopManager` now self-wires `../TickManager` when the exported reference is not assigned and logs a clear error if it still cannot be found.
- `UIManager.SetStartupMode(false)` now refreshes modal overlay visibility instead of forcibly hiding the modal root during runtime.
- The oversized automatic first-run tutorial overlay is disabled by default. The tutorial popup body is also bounded inside a scroll container for safer future re-enable work.
- `StoreLayoutManager.BuildFallbackPath` now returns the safe start point when no unobstructed path can be found.

## 4. UI issues fixed

- Removed the blocking runtime first-run tutorial overlay from the default new-game path.
- Preserved modal-root state for runtime popups instead of hiding it during every runtime refresh.
- Verified startup-to-new-game runtime UI at:
  - 1280x720
  - 1600x900
  - 1920x1080
- Smoke validation confirmed `TopMenu`, `LeftMenu`, and `MainGameArea` are visible and do not overlap after starting a game.

## 5. Store visualization issues fixed

- Customers and employees no longer receive a fallback direct path through shelf obstacles when pathfinding fails.
- Existing shelf visualization continues to duplicate original shelf scene controls through `CreateShelfVisualFromOriginal`.
- No generated shelf placeholder references were found in runtime store visualization code.
- Existing regression coverage verifies purchased shelf creation, original shelf visual reuse, employee name/role labels, furniture save/load, and customer/employee pathing contracts.

## 6. Files modified

- `Directory.Solution.props`
- `client.godot/Loop/LoopManager.cs`
- `client.godot/Visuals/UIManager.cs`
- `client.godot/Visuals/Store/StoreLayoutManager.cs`
- `Core.Simulation.Tests/RegressionCoverageTests.cs`
- `docs/ProjectAuditReport.md`

## 7. Files removed

- None from the repository by this audit pass.

Note: the worktree already contained deleted UI assets and unrelated edits before this pass. Those were not reverted.

## 8. Remaining warnings

- Plain `dotnet build ChronoIndustrialist.sln` still fails silently; serialized `-m:1` build succeeds.
- UI validation was performed through Godot headless scene execution, not GUI screenshot comparison.
- Full interactive customer purchase loops were not observed manually in the editor during this pass.
- The automatic tutorial remains disabled until it receives dedicated visual QA.

## 9. Remaining technical debt

- The first-run tutorial needs to be redesigned as a non-blocking or carefully bounded flow before re-enabling.
- `TriggerPlaceholderEvent` and the visible test-event workflow remain active and should be reviewed before MVP.
- Store furniture non-shelf upgrades still use simple placeholder visuals.
- Store layout still relies on runtime scaling for fixed-position shop children.
- The pre-existing dirty worktree should be split into reviewable changes before release hardening.

## 10. Recommendations for MVP completion

- Document `dotnet build ChronoIndustrialist.sln -m:1` as the reliable local/CI solution build command, or split client and simulation builds into separate CI steps.
- Add a committed Godot smoke test that starts a game, opens the shop, advances business ticks, saves, loads, and asserts no runtime errors.
- Add GUI screenshot regression checks for 1280x720, 1600x900, and 1920x1080.
- Re-enable the tutorial only after it passes viewport-bounds checks in all target resolutions.
- Add direct tests for maximum purchased shelf count and dynamic shelf slot visibility.
