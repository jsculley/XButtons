# XButtons

A lightweight SolidWorks add-in that intercepts X1 and X2 mouse side button clicks and translates them into keyboard shortcuts that SolidWorks can act on.

| Mouse button | Keystroke sent |
|---|---|
| X1 (back) | Ctrl + Alt + Shift + B |
| X2 (forward) | Ctrl + Alt + Shift + F |

These shortcuts can be bound to any SolidWorks command via **Tools → Customize → Keyboard**.

---

## How it works

The add-in installs a system-wide low-level mouse hook (`WH_MOUSE_LL`) when SolidWorks loads it. When an X1 or X2 button press is detected, it synthesizes the corresponding key chord using `SendInput` and passes the original mouse event on normally so no other application is affected.

---

## Requirements

- SolidWorks 2020 or later (earlier versions likely work but are untested)
- .NET Framework 4.x
- Windows 10/11 64-bit

---

## Building

1. Clone the repository
   ```
   git clone https://github.com/your-username/xbuttons.git
   ```
2. Open `xbuttons.sln` in Visual Studio 2019 or later
3. Restore NuGet packages if prompted
4. Set the build configuration to **Release / x64**
5. Build the solution — the output DLL will be in `bin\x64\Release\`

---

## Installation

1. Build the solution (see above)
2. Open a **Developer Command Prompt for Visual Studio** as Administrator
3. Register the add-in with SolidWorks:
   ```
   regasm /codebase XButtons.dll
   ```
4. Restart SolidWorks — the add-in will appear in **Tools → Add-ins**

To unregister:
```
regasm /unregister XButtons.dll
```

---

## Configuring keyboard shortcuts in SolidWorks

After installation you need to bind the two shortcuts to SolidWorks commands:

1. Go to **Tools → Customize → Keyboard**
2. Search for the command you want to assign
3. Click in the **Shortcut(s)** column and press the desired combination:
   - X1 button → press **Ctrl + Alt + Shift + B**
   - X2 button → press **Ctrl + Alt + Shift + F**
4. Click **OK**

Common assignments:
- Previous/Next view
- Undo/Redo
- Rotate/Pan/Zoom mode toggle

---

## Project structure

```
xbuttons/
├── XButtonsAddin.cs      # ISwAddin implementation, add-in lifecycle
├── MouseHook.cs          # WH_MOUSE_LL hook, SendInput keystroke synthesis
├── XButtons.csproj
└── XButtons.sln
```

---

## License

MIT
