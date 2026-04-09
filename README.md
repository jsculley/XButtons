# XButtons

A lightweight SOLIDWORKS add-in that intercepts X1 and X2 mouse side button clicks and translates them into keyboard shortcuts that SOLIDWORKS can act on.

These shortcuts can be bound to any SOLIDWORKS command via **Tools → Customize → Keyboard**.

---

## How it works

The add-in installs a system-wide low-level mouse hook (`WH_MOUSE_LL`) when SOLIDWORKS loads it. When an X1 or X2 button press is detected, it synthesizes the corresponding key chord using `SendInput` and passes the original mouse event on normally so no other application is affected.
Key chords can consist of a single key or any single key along with any of the modifiers Ctrl, Alt, and Shift.
The first time the add-in is enabled (via the Tools&rarr;Add-Ins menu) a dialog will be displayed allowing you to specify the key or key combination you want to associate with the X1 and/or X2 mouse buttons.
If you later want to change the combinations, you can use the Tools&rarr;XButtons&rarr;Settings menu item

---

## Requirements

- SOLIDWORKS 2020 or later (earlier versions likely work but are untested)
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

## Installation using Windows Installer (MSI) File
1. Download the Windows Installer .msi file from the [Releases](https://github.com/jsculley/XButtons/releases) page
2. Double-click the MSI file and follow the prompts
3. Start SOLIDWORKS
4. Go to Tools&rarr;Add-Ins, scroll down and check the 'Active Add-Ins' checkbox.  To always load the add in, check the 'Startup' checkbox.
5. Click OK and the XButtons setup dialog should appear


## Installation from source
1. Build the solution (see above)
2. Open a **Developer Command Prompt for Visual Studio** as Administrator
3. Register the add-in with SOLIDWORKS:
   ```
   C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm /codebase <full path to XButtons.dll>
   ```
4. Restart SOLIDWORKS — the add-in will appear in **Tools → Add-ins**

To unregister:
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm /unregister <full path to XButtons.dll>
```

---

## Configuring keyboard shortcuts in SOLIDWORKS

After installation you need to bind the two shortcuts to SOLIDWORKS commands:

1. Go to **Tools → Customize → Keyboard**
2. Search for the command you want to assign
3. Click in the **Shortcut(s)** column and press the combination you used when setting up XButtons
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
