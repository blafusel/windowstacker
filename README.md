# WindowStacker

A minimal Windows tray application for Z-order window management via global hotkeys and mouse gestures.

| Input            | Action                                                      | Default |
|------------------|-------------------------------------------------------------|---------|
| Alt+F1           | Bring the window under the mouse to the front               | Off     |
| Alt+F3           | Send the window under the mouse to the back                 | Off     |
| Alt+Esc          | Close the window under the mouse (or active window if keyboard was last used) | On |
| Ctrl+RMB         | Send the window under the mouse to the back                 | On      |

The tray icon shows a "W". Left-click to pause/resume. Right-click for the menu.

Individual features can be toggled from the tray menu. **Alt+F1 and Alt+F3 are off by default** — they have known reliability issues with some applications (see Known limitations) and need more work before being on by default.

---

## Requirements

- Windows 10 or 11
- .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

---

## Build

Open a terminal in the project folder and run:

```
dotnet build -c Release
```

The output lands in `bin\Release\net8.0-windows\`.

### Single-file self-contained executable (recommended)

This produces one .exe with no runtime dependency:

```
dotnet publish -c Release
```

Output: `bin\Release\net8.0-windows\win-x64\publish\WindowStacker.exe`

---

## Run at Windows startup

1. Press `Win+R`, type `shell:startup`, press Enter.
2. Drop a shortcut to `WindowStacker.exe` into the folder that opens.

That is all. Windows will launch it silently on login.

---

## Known limitations

- **Alt+F1 and Alt+F3 are experimental and off by default.** They have known reliability
  issues with GPU-composited windows (e.g. Chromium-based browsers): the revealed window
  may not repaint or respond correctly in all cases. Enable them from the tray menu if you
  want to try them, but expect rough edges.

- **Alt+F3** is also used by Firefox (find previous match) and a few other apps. Because
  the hotkey is registered globally, those apps will stop receiving it while the option is
  enabled in WindowStacker.

- `SetWindowPos` with `HWND_BOTTOM` does not work on windows that belong to elevated
  (administrator-level) processes when WindowStacker itself is running at normal privilege.
  The call will silently do nothing in that case. Running WindowStacker as administrator
  fixes it, but that is usually not worth the tradeoff.

- The "bring forward" action does not forcibly focus the window, only raises it in Z-order.
  This is intentional: focus stealing is disruptive.
