using System.Diagnostics;

namespace MiniLaunch.Core;

public static class WindowProcessHelper
{
    // ----------------- CAPTURE -----------------

    public static bool TryGetMainWindow(string processName, out IntPtr handle)
    {
        handle = Process.GetProcessesByName(processName)
            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero)?
            .MainWindowHandle ?? IntPtr.Zero;

        if (handle == IntPtr.Zero)
        {
            Log.WriteCategory("CAPTURE", $"{processName} | no window found");
            return false;
        }

        return true;
    }

    // ----------------- LAUNCH HELPERS -----------------

    public static HashSet<IntPtr> GetWindowSet(string processName)
    {
        return Process.GetProcessesByName(processName)
            .Where(p => p.MainWindowHandle != IntPtr.Zero)
            .Select(p => p.MainWindowHandle)
            .ToHashSet();
    }

    public static List<IntPtr> GetWindowList(string processName)
    {
        return Process.GetProcessesByName(processName)
            .Where(p => p.MainWindowHandle != IntPtr.Zero)
            .Select(p => p.MainWindowHandle)
            .ToList();
    }
}