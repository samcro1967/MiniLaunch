namespace MiniLaunch;

public static class HelpContent
{
    public static string Get(string supportedApps)
    {
        return $@"MiniLaunch Help

{supportedApps}

Capture a Profile:
- Open the apps you want to include
- Right-click tray → Profiles → Capture Profile
- Only currently running supported apps are captured
- Window size, position, and monitor are saved

Run a Profile:
- Right-click tray → Profiles → Run
- Or double-click the tray icon (runs default profile)

Manage Profiles:
- Profiles → Edit / Rename / Delete / Set Default

Default Profile:
- A default profile is used when double-clicking the tray icon
- Set or change it via: Profiles → Set as Default

Advanced Configuration:
- Profiles can be edited manually (JSON)
- Right-click tray → Profiles → Edit

You can customize the following for each app:
- File paths (Explorer, Notepad++, etc.)
- URLs (Chrome)
- Terminal starting directories or profiles
- Notepad++ sessions
- Visual Studio solution paths

Note:
- Capture does NOT detect tabs, sessions, or internal app state
- Only window layout is captured automatically
- Advanced behavior must be configured manually in the profile JSON
- Manual edits override captured values
- Changes take effect the next time the profile is run

Location:
%LOCALAPPDATA%\MiniLaunch\Profiles";
    }
}