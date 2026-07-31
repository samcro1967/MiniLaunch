using System.Diagnostics;
using System.Windows.Forms;
using MiniLaunch.Core;

public static class WindowCaptureHelper
{
    public static bool TryCaptureWindow(
        string type,
        IntPtr handle,
        out AppConfig? app)
    {
        app = null;

        if (handle == IntPtr.Zero)
        {
            Log.WriteCategory("CAPTURE", $"{type} | invalid handle");
            return false;
        }

        if (!WindowHelpers.TryGetWindowRect(handle, out var rect))
        {
            WindowHelpers.DebugWindow($"{type.ToUpper()} RECT FAILED", handle);
            return false;
        }

        WindowHelpers.DebugWindow($"{type.ToUpper()} FINAL", handle);

        // RELATIVE conversion
        var screen = Screen.FromHandle(handle);
        int monitorIndex = Array.IndexOf(Screen.AllScreens, screen);

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;

        int relativeX = rect.Left - screen.Bounds.Left;
        int relativeY = rect.Top - screen.Bounds.Top;

        // ✅ FIXED: structured + consistent naming
        Log.WriteCategory("CAPTURE",
            $"{type} | MON={monitorIndex} | REL_POS=({relativeX},{relativeY}) | SIZE={width}x{height}");

        app = new AppConfig
        {
            Type = type,
            X = relativeX,
            Y = relativeY,
            Width = width,
            Height = height,
            Maximized = false,
            Monitor = monitorIndex
        };

        return true;
    }
}