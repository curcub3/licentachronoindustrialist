# UI Readability And Overlap Fix Report

Date: 2026-06-05

## 1. Root Cause Of Overlaps Found

- The active root layout was close to the target structure, but it did not use the required node names and sizes. `TopMenu` was 78px tall instead of the requested 64px, and `LeftMenu` was 286px wide instead of the requested 260px.
- The shop floor placeholder used full-rect anchors with a positive bottom offset, which could extend the floor rectangle outside `Shop2DView`.
- Runtime code still knew the old root layout names and did not have a debug check to warn about major region overlap.
- Several store-world temporary visuals looked like plain colored blocks. Customers were unlabeled, non-shelf furniture had no border treatment, and missing shelf-asset fallback could become an empty control.

## 2. Files Modified

- `client.godot/Scenes/UIRoot.tscn`
- `client.godot/Visuals/UIManager.cs`
- `client.godot/Visuals/Store/CustomerVisualController.cs`
- `client.godot/Visuals/Store/EmployeeVisualController.cs`
- `client.godot/Visuals/Store/StoreFurnitureVisualController.cs`
- `Core.Simulation.Tests/RegressionCoverageTests.cs`
- `docs/UIReadabilityAndOverlapFixReport.md`

## 3. Runtime Layout Overrides Removed

- No gameplay root positioning code was added.
- The root gameplay regions are now container-owned: `TopMenu`, `LeftMenu`, and `MainGameArea` are positioned by `VBoxContainer`/`HBoxContainer`.
- `UIManager` still updates visibility, contents, shop-canvas scale, and popup bounds. It does not manually set `Position`, `Size`, anchors, or offsets for `TopMenu`, `LeftMenu`, or `MainGameArea`.
- The shop-canvas child scaling remains intentionally scoped to `Shop2DView` world objects, not to the root UI layout.

## 4. UI Hierarchy After Fix

```text
UIRoot
└── MarginContainer
    └── RootLayout (VBoxContainer)
        ├── TopMenu (PanelContainer, 64px min height)
        └── BodyLayout (HBoxContainer, fill + expand)
            ├── LeftMenu (PanelContainer, 260px min width)
            └── MainGameArea (HBoxContainer, fill + expand)
                ├── Shop2DView
                └── RightContextPanel
```

## 5. Placeholder Style Created

- Non-shelf furniture now uses a consistent temporary panel style:
  - neutral tinted panel background
  - visible light border
  - small radius
  - centered wrapped Romanian label
  - readable 10px label text inside the parent rect
- Missing shelf assets use a warning placeholder label:
  - `ASSET LIPSĂ: raft ...`
  - warning border color
  - no silent empty control fallback

## 6. Placeholders Cleaned Up

- Customer temporary visuals now show a readable `Client` label.
- Employee temporary visuals keep visible name and role text; long names are shortened before display instead of depending on clipping.
- Non-shelf purchased furniture now reads:
  - `Depozit / vizual temporar`
  - `Neon / vizual temporar`
  - `Scanner / vizual temporar`
  - `Cărucior / vizual temporar`
- Shelf visuals still prefer original shelf scene controls through `CreateShelfVisualFromOriginal`.
- If an expected shelf scene cannot be duplicated, the fallback is visibly labeled as a missing asset.

## 7. Remaining Placeholders

- Store non-shelf purchase visuals are still temporary art placeholders, but they are now labeled and styled intentionally.
- Line edit placeholder text remains in normal form fields: store name, price, quantity, and refill quantity.
- No generated shelf placeholder references were found in runtime store visualization code.

## 8. Resolution Tests Performed

- Static layout contract was updated for the required target resolutions:
  - 1280x720
  - 1600x900
  - 1920x1080
- The root layout now reserves:
  - 64px minimum top menu height
  - 260px minimum left menu width
  - fill/expand body and main game area
- `dotnet build ChronoIndustrialist.sln`: passed, 0 warnings, 0 errors.
- `dotnet test ChronoIndustrialist.sln`: passed, 48/48 tests.
- Static `res://` resource reference scan: passed.
- Static direct UI localization key scan: passed.
- Engine screenshot validation at the three target resolutions was not run because no `godot`, `godot4`, or `godot4.6` executable is available on `PATH` in this shell.

## 9. Remaining Layout Risks

- The new debug overlap checker has not been observed inside a running Godot session because the executable is unavailable here.
- `Shop2DView` still uses a designed 2D shop canvas with scaled absolute child positions. This is acceptable for the world visualization, but it should remain clipped inside `MainGameArea`.
- Popup positioning is bounded by `FitPopupToViewport`, but final visual confirmation still needs a Godot screenshot pass.
- Runtime UI is protected by regression tests and static scans, but true font rasterization and Romanian text wrapping should still be checked in-engine.

## 10. Recommended Next UI Cleanup Steps

- Install Godot 4.6.x in the local/CI environment and run automated screenshots at 1280x720, 1600x900, and 1920x1080.
- Enable `EnableLayoutDebug` during development smoke runs and treat any `UI layout overlap` or `UI popup bounds` warning as a bug unless documented as intentional.
- Replace temporary non-shelf furniture visuals with final art assets when available.
- Consider moving the temporary customer/employee/furniture visual style into a shared scene or reusable component once final art direction is stable.
- Add a Godot-level smoke test that starts a game, opens each popup, resizes the viewport, and asserts no root-region overlap warnings.
