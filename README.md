# WindowStacker

A minimal Windows tray application that adds two global hotkeys for Z-order window management.

| Hotkey   | Action                                          |
|----------|-------------------------------------------------|
| Alt+F1   | Bring the window under the mouse to the front   |
| Alt+F3   | Send the window under the mouse to the back     |
| Alt+Esc  | Close the window under the mouse                |

The tray icon shows a "W". Double-click it to pause/resume. Right-click for the menu.

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

- **Alt+F3** is used by Firefox (find previous match) and a few other apps. Because the
  hotkey is registered globally, those apps will stop receiving it while WindowStacker is
  running. Use the tray "Pause" option or exit if you need it back temporarily.

- `SetWindowPos` with `HWND_BOTTOM` does not work on windows that belong to elevated
  (administrator-level) processes when WindowStacker itself is running at normal privilege.
  The call will silently do nothing in that case. Running WindowStacker as administrator
  fixes it, but that is usually not worth the tradeoff.

- The "bring forward" action does not forcibly focus the window, only raises it in Z-order.
  This is intentional: focus stealing is disruptive.
