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

Default Profile:
- First captured profile becomes default
- Change via: Profiles → Set as Default

Advanced Configuration:
- Profiles can be edited manually
- Open the Profiles folder from the About dialog
- Each profile is a JSON file

You can customize things like:
- File paths (Explorer, Notepad++, etc.)
- URLs (Chrome)
- Terminal starting directories
- Notepad++ sessions
- Visual Studio solution paths

Note:
- Capture does NOT detect tabs, sessions, or internal app state
- Only window layout is captured automatically
- Advanced behavior must be configured manually in the profile JSON

Location:
%LOCALAPPDATA%\MiniLaunch\Profiles";
    }
}