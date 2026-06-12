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
| `TrayApplication.cs` | Owns `NotifyIcon` + `HotkeyWindow` + low-level hooks. Pause/resume state and Alt+Esc mode state live here. Draws tray icon programmatically. |
| `HotkeyWindow.cs` | Invisible `NativeWindow` (real HWND needed for `RegisterHotKey`). Receives `WM_HOTKEY` messages, fires `BringForward`/`SendBack`/`CloseWindow` events. |
| `WindowZOrder.cs` | Stateless. Gets window under cursor via `GetCursorPos` → `WindowFromPoint` → `GetAncestor(GA_ROOTOWNER)`, then calls `SetWindowPos` or `PostMessage(WM_CLOSE)`. |
| `NativeMethods.cs` | All P/Invoke declarations and Win32 constants. |

## Two-tier input handling

There are two separate Win32 input mechanisms active simultaneously:

**1. `RegisterHotKey` (via `HotkeyWindow`)** handles Alt+F1, Alt+F3, Alt+Esc. These arrive as `WM_HOTKEY` on the hidden HWND. `MOD_NOREPEAT` is used for F1/F3 but intentionally omitted for Alt+Esc, because that flag can silently conflict with the system hook for `VK_ESCAPE` on some machines.

**2. `SetWindowsHookEx` (via `TrayApplication`)** installs two global low-level hooks:
- `WH_MOUSE_LL` — detects LMB+RMB chord (calls `SendActiveWindowToBack`) and tracks `_lastActionWasMouseMove`
- `WH_KEYBOARD_LL` — clears `_lastActionWasMouseMove` on any keydown except Alt and Esc themselves (those are excluded so the hotkey invocation doesn't corrupt the flag)

The hook delegate references are stored as fields (`_mouseHookProc`, `_keyboardHookProc`) to prevent GC collection while the hooks are active.

## Smart Alt+Esc dual-mode

`_lastActionWasMouseMove` (bool in `TrayApplication`) determines what Alt+Esc does:
- `true` (last input was mouse movement) → `CloseWindowUnderCursor()` — uses `GetCursorPos`
- `false` (last input was a keypress) → `CloseActiveWindow()` — uses `GetForegroundWindow()`

Close is always `PostMessage(WM_CLOSE)` — graceful, not force-kill.

## Key design constraints

- **No focus stealing** — `SetWindowPos` with `SWP_NOACTIVATE`. Intentional.
- **`SWP_ASYNCWINDOWPOS`** — avoids deadlocks when the target window is slow to respond.
- **Elevated windows** — `SetWindowPos(...HWND_BOTTOM...)` silently fails against admin-level processes when the app runs at normal privilege. Known limitation, not a bug.
- **Alt+F3 conflict** — globally consumed, breaks Firefox "find previous". Documented in README.
- **Tray icon** — drawn in code (`BuildTrayIcon`). `app.ico` exists but is the exe file icon shown in Explorer, not the tray icon.
- `WS_EX_TOOLWINDOW` on the hotkey receiver keeps it out of taskbar and Alt-Tab.

## Project config

- Target: `net8.0-windows`, WinForms, `win-x64` RID
- `PublishSingleFile=true`, `SelfContained=true` — publish bundles the runtime
- `AllowUnsafeBlocks=false` — P/Invoke via `DllImport` only
- Version is in `WindowStacker.csproj` (`<Version>`, `<AssemblyVersion>`, `<FileVersion>`)
- `app.manifest` sets `asInvoker` (no UAC elevation) and `PerMonitorV2` DPI awareness
