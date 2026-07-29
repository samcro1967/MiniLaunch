using Microsoft.Win32;

namespace MiniLaunch.Core;

public static class StartupManager
{
    private const string AppName = "MiniLaunch";
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);

        if (key == null) return false;

        return key.GetValue(AppName) != null;
    }

    public static void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);

        if (key == null) return;

        var exePath = Application.ExecutablePath;

        // Quote path in case of spaces
        key.SetValue(AppName, $"\"{exePath}\"");
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);

        if (key == null) return;

        if (key.GetValue(AppName) != null)
        {
            key.DeleteValue(AppName, false);
        }
    }
}
