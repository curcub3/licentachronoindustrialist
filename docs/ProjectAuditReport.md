# Project Audit Report

Date: 2026-06-05

## 1. Critical issues found

- The root solution fails under default MSBuild max-cpu scheduling with no surfaced compiler diagnostics when the Godot C# project and `Core.Simulation` are built concurrently.
- The store visual path fallback could return a direct final target after route discovery failed, which allowed a last-resort movement segment to cross shelf obstacles.
- `docs/` was missing, so the requested audit report location did not exist.

## 2. Build errors fixed

- Added `Directory.Solution.props` to disable parallel solution restore.
- Added explicit solution dependencies from `client.godot` and `Core.Simulation.Tests` to `Core.Simulation`.
- Verified the full C# solution builds with serialized MSBuild scheduling:
  - `dotnet build ChronoIndustrialist.sln -m:1`
  - Result: succeeded, 0 warnings, 0 errors.

Remaining build warning: plain `dotnet build ChronoIndustrialist.sln` still exits nonzero under MSBuild max-cpu scheduling with 0 warnings and 0 errors. Use `-m:1` for reliable root solution builds unless the Godot SDK scheduling issue is resolved.

## 3. Runtime errors fixed

- Hardened `StoreLayoutManager.BuildFallbackPath` so failed route discovery returns the current safe start point instead of a blocked direct target. This prevents customers and employees from being instructed to walk through shelves in fallback conditions.
- Verified Godot headless launch succeeds:
  - `--resolution 1280x720`
  - `--resolution 1600x900`
  - `--resolution 1920x1080`

## 4. UI issues fixed

- No UI scene edits were required in this pass.
- Static audit found the top menu, left menu, and main game area are container-driven and within minimum-size limits for 1280x720 and larger.
- Headless scene loads at 1280x720, 1600x900, and 1920x1080 completed without runtime errors.

## 5. Store visualization issues fixed

- Path fallback now avoids shelf-crossing movement when no safe path can be found.
- Existing shelf visualization already duplicates original shelf scene controls via `CreateShelfVisualFromOriginal`.
- No generated shelf placeholder references were found in runtime store visualization code.
- Existing tests cover purchased shelf creation, shelf visual reuse, employee labels/roles, save/load of furniture, and customer/employee pathing contracts.

## 6. Files modified

- `ChronoIndustrialist.sln`
- `Directory.Solution.props`
- `client.godot/Visuals/Store/StoreLayoutManager.cs`
- `Core.Simulation.Tests/RegressionCoverageTests.cs`
- `docs/ProjectAuditReport.md`

## 7. Files removed

- None by this audit pass.

Note: the worktree already contained many deleted UI asset files before this pass. I did not revert or remove those user-side changes.

## 8. Remaining warnings

- Default parallel `dotnet build ChronoIndustrialist.sln` still fails without diagnostics; serialized build succeeds.
- UI validation was performed through scene inspection and headless launch, not interactive screenshot inspection.
- Runtime store behavior was smoke-validated by startup and covered by tests, but full customer purchase loops were not observed interactively.

## 9. Remaining technical debt

- `TriggerPlaceholderEvent` and the visible test-event workflow remain active. They appear intentional in existing tests but should be reviewed before MVP.
- Store furniture non-shelf upgrades still use simple placeholder visuals.
- Store layout still uses runtime scaling for fixed-position shop children.
- The root worktree contains substantial pre-existing edits and deleted assets; those should be reviewed as a separate change set.

## 10. Recommendations for MVP completion

- Make `dotnet build ChronoIndustrialist.sln -m:1` the documented CI/build command for now, or split Godot client and simulation test builds into separate CI steps.
- Add an automated Godot smoke scene that starts a relaxed game, opens the shop, advances several business ticks, saves, loads, and asserts no runtime errors.
- Add visual regression screenshots for 1280x720, 1600x900, and 1920x1080 once a GUI-capable test runner is available.
- Replace or formalize the active placeholder event/test button before MVP.
- Add direct tests for maximum purchased shelf count and dynamic shelf slot visibility.
