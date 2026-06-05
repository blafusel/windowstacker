# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Publish

```
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Single-file self-contained exe (primary deliverable)
dotnet publish -c Release
# Output: bin\Release\net8.0-windows\win-x64\publish\WindowStacker.exe
```

No test project exists. Verification = run the exe and test hotkeys manually.

## Architecture

Five files, no dependencies beyond WinForms:

| File | Role |
|------|------|
| `Program.cs` | Entry point. STAThread, runs message loop via `Application.Run()` (no main form). |
| `TrayApplication.cs` | Owns `NotifyIcon` + `HotkeyWindow`. Pause/resume state lives here. Draws tray icon programmatically (no embedded resources). |
| `HotkeyWindow.cs` | Invisible `NativeWindow` (real HWND needed for `RegisterHotKey`). Receives `WM_HOTKEY` messages, fires `BringForward`/`SendBack` events. |
| `WindowZOrder.cs` | Stateless. Gets window under cursor via `GetCursorPos` -> `WindowFromPoint` -> `GetAncestor(GA_ROOTOWNER)`, then calls `SetWindowPos`. |
| `NativeMethods.cs` | All P/Invoke declarations and Win32 constants. |

## Key design constraints

- **No focus stealing** - `SetWindowPos` with `SWP_NOACTIVATE`. Intentional.
- **`SWP_ASYNCWINDOWPOS`** used to avoid deadlocks when target window is slow to respond.
- **Elevated windows** - `SetWindowPos(...HWND_BOTTOM...)` silently fails against admin-level processes when app runs at normal privilege. Known limitation, not a bug.
- **Alt+F3 conflict** - globally consumed, breaks Firefox "find previous". Documented in README.
- Tray icon is drawn in code (`BuildTrayIcon`) - no `.ico` file or resource needed.
- `WS_EX_TOOLWINDOW` on the hotkey receiver keeps it out of taskbar and Alt-Tab.

## Project config

- Target: `net8.0-windows`, WinForms, `win-x64` RID
- `PublishSingleFile=true`, `SelfContained=true` - publish bundles the runtime
- `AllowUnsafeBlocks=false` - P/Invoke via `DllImport` only
