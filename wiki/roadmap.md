# Roadmap

A summary of every FancyMouse release so far, followed by planned future work.

Releases and future work are tagged with the long-term **initiatives** they advance, so individual changes (and the PowerToys Mouse Jump PRs that follow them) can be read as steps toward a stated goal rather than in isolation.

## Initiatives

| Tag | Initiative |
| --- | --- |
| `winforms` | Decouple from WinForms |
| `winui` | Migrate to WinUI |
| `visual` | Visual improvements |
| `perf` | Performance improvements |
| `platform` | Decouple from Windows (platform-neutral core) |
| `mwb` | Mouse Without Borders integration |
| `features` | Features / usability |
| `fixes` | Bug fixes |
| `maint` | Maintenance (upgrades, refactoring, PowerToys sync) |

## Releases

| Release | Date | Highlights | Initiatives |
| --- | --- | --- | --- |
| [v0.0.3-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.3-preview) | 2022-11-15 | First pre-release, for demo purposes | `features` |
| [v0.0.4-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.4-preview) | 2022-11-20 | Minor updates | `maint` |
| [v0.0.5-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.5-preview) | 2022-12-28 | GitHub Actions build | `maint` |
| [v0.0.6-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.6-preview) | 2023-01-13 | Rewrite of P/Invoke classes | `maint` |
| [v0.0.7-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.7-preview) | 2023-01-19 | Logging; FancyMouse.Lib folded into the main project | `maint` |
| [v0.0.8-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.8-preview) | 2023-02-06 | Mixed-DPI scaling fix; screens above/left of origin; WinForms cursor workaround | `fixes` |
| [v0.0.9-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.9-preview) | 2023-02-21 | Per-screen capture and compose - up to 7x faster thumbnails on jagged layouts | `perf` |
| [v0.0.10-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.10-preview) | 2023-03-03 | Faster preview loading | `perf` |
| [v0.0.11-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.11-preview) | 2023-03-09 | Background texture; progressive activation *(withdrawn - see v0.0.12)* | `visual` `perf` |
| [v0.0.12-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.12-preview) | 2023-03-10 | v0.0.11 features, plus fix for wrong jump target when primary isn't top-left | `fixes` |
| [v0.0.13-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.13-preview) | 2023-03-14 | Keyboard shortcuts to jump to screens (1-9, P, arrows, Home/End); scaling rounding fix | `features` `fixes` |
| [v0.0.14-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.14-preview) | 2023-04-13 | Reduced WinForms dependency; screenshots drawn as each screen completes | `winforms` `perf` |
| [v0.0.15-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.15-preview) | 2023-07-18 | Customisable appearance (margins, borders, padding, background); 3D bezel default | `visual` `features` |
| [v0.0.16-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.16-preview) | 2023-07-25 | Config file watcher - settings reload without restart | `features` |
| [v0.0.17-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.17-preview) | 2024-04-25 | .NET 8; shared code moved into "Common" for PowerToys sync; rendering tests | `maint` |
| [v0.0.18-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.18-preview) | 2024-05-10 | Number pad shortcuts; preview scaling rounding fix | `fixes` |
| [v0.0.19-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.19-preview) | 2024-07-14 | Common code split into its own library for PowerToys sync | `maint` |
| [v0.0.20-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.20-preview) | 2024-07-31 | Background bleed and off-by-one preview size fixes | `fixes` |
| [v0.0.21-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.21-preview) | 2025-03-11 | .NET 9; initial multi-device support (MWB groundwork) | `maint` `mwb` |
| [v0.0.22-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.22-preview) | 2025-08-17 | Full rewrite in WinUI (WinForms version kept alongside) | `winui` |
| [v0.0.23-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.23-preview) | 2026-03-28 | WinUI tray icon crash fix; .NET 10 | `fixes` `maint` |
| [v0.0.24-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.24-preview) | 2026-08-25 | Rounded, shaded bezels; preview split into WinUI components; local telemetry; WinForms version removed | `visual` `perf` `winforms` |
| [v0.0.25-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.25-preview) | 2026-09-05 | Bezel rendering clean-up and lighting/curvature fixes | `visual` |
| [v0.0.26-preview](https://github.com/mikeclayton/FancyMouse/releases/tag/v0.0.26-preview) | 2026-09-08 | Code clean-up; unbounded preview layouts (fixes PowerToys #50436) | `maint` |

## Future work

### Sooner

| Work | Notes | Initiatives |
| --- | --- | --- |
| Sync FancyMouse &rarr; PowerToys Mouse Jump | Bring Mouse Jump up to date with FancyMouse | `maint` `winui` |
| Settings UI in FancyMouse | Mirrors PowerToys Settings structure so it can be ported | `features` |

### Later

These don't add features themselves - each removes an obstacle to a long-term goal.

| Work | Notes | Initiatives |
| --- | --- | --- |
| `FancyMouse.Drawing` | Replaces Windows-only `System.Drawing.Common` with SkiaSharp so rendering (and its tests) are platform-neutral | `platform` |
| Refine WinUI3 controls | Makes PreviewPane self-contained (no dependencies on app code) so it can move to its own library once a second host needs it | `winui` |
| `FancyMouse.Platform.Abstractions` / `FancyMouse.Platform.Windows` | Moves Win32 calls out of shared code behind platform interfaces | `platform` |
| `FancyMouse.Integrations.MWB` | Implements the device interfaces (screens, thumbnails, cursor moves) for Mouse Without Borders peers, so remote machines plug into the same seam as the local one | `mwb` |

### Ideas

| Idea | Summary | Initiatives |
| --- | --- | --- |
| Platform neutrality | Core code depends only on platform abstractions, with Windows as one implementation | `platform` |
| Remote Jump | Mouse Jump across Mouse Without Borders machines, using MWB as a proxy | `mwb` `features` |
| Mouse Finder | Hotkey popup on every screen of every MWB machine, with the cursor's screen highlighted and a live cursor marker | `mwb` `features` |
| Mouse Lock | Confine the cursor to the current monitor (enabled / hotkey-held / disabled), coordinated with MWB | `mwb` `features` |
| Remote control | Phone or always-on-top minimap showing the MWB layout and live cursor | `mwb` `features` |
