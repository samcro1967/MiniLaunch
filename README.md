# MiniLaunch

MiniLaunch is a lightweight Windows tray application that lets you capture and launch multi-app workspace profiles.

It restores your entire workspace — apps, windows, and layout — in seconds.

---

## Features

- Capture your current workspace into a profile
- Launch saved profiles instantly
- Manage profiles (Run / Rename / Delete)
- Tray-based UI (no window clutter)
- Auto-refresh when profile files change
- Optional "Start with Windows"

---

## Installation

1. Download the latest `MiniLaunchSetup.exe`
2. Run the installer
3. Launch MiniLaunch (runs in system tray)

---

## Example Profiles

See the `/examples` folder for sample configurations:

- [captured_profile.json](examples/captured_profile.json) – Generated after capturing your current layout
- [configured_profile.json](examples/configured_profile.json) – Edited version with apps, paths, and tabs defined

You can copy and modify these to create your own profiles.

---

## Usage

### Tray Menu

Right-click the tray icon:

**Profiles**
- Capture Profile
- Run >
- Rename >
- Delete >

**Settings**
- Startup With Windows
  - Currently Enabled/Disabled
  - Enable/Disable

**About**  
**Exit**

---

### Capture a Profile

1. Arrange your apps/windows
2. Tray → Profiles → Capture Profile
3. Enter a name

---

### Run a Profile

- Tray → Profiles → Run → [Profile Name]
- Or double-click tray icon to run first profile

---

## Supported Applications

MiniLaunch uses a modular launcher system. Currently supported:

- Google Chrome (new window + tabs)
- Windows Terminal (tabs / profiles)
- Windows Explorer (single folder)
- Notepad++ (session)
- Visual Studio

Support is implemented via internal modules (`IAppModule`).

Additional apps can be added by implementing new modules.

---

## Profile Storage

Profiles are stored as JSON files:

```
%LOCALAPPDATA%\MiniLaunch\Profiles
```

Each profile represents a captured workspace.

---

## Settings

### Location

Settings are stored here:

```
%LOCALAPPDATA%\MiniLaunch\settings.json
```

---

### Editing Settings

You can edit this file manually:

1. Close MiniLaunch
2. Open `settings.json` in a text editor
3. Modify values
4. Restart MiniLaunch

---

### Example

```json
{
  "StartWithWindows": true
}
```

---

## Startup Behavior

MiniLaunch can start automatically with Windows.

- Controlled via tray menu:
  - Settings → Startup With Windows

- Uses registry:
  ```
  HKCU\Software\Microsoft\Windows\CurrentVersion\Run
  ```

---

## Architecture

- Tray-based `ApplicationContext`
- Modular launcher system (`IAppModule`)
- JSON-based profiles
- `FileSystemWatcher` for live updates

---

## Troubleshooting

### App does not appear

- Check system tray
- Restart the app

---

### Startup toggle not working

Check registry:

```
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
```

---

### Profiles not updating

Verify files exist in:

```
%LOCALAPPDATA%\MiniLaunch\Profiles
```

---

## License

See `LICENSE.txt` included with the application.

---

## Notes

MiniLaunch is part of the MiniSuite utilities.
MiniLaunch is part of the MiniSuite utilities.