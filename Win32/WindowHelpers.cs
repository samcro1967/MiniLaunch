using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

public static class WindowHelpers
{
    // ---------------- WAIT FOR WINDOW ----------------

    public static IntPtr WaitForMainWindow(Process process)
    {
        for (int i = 0; i < 15; i++)
        {
            process.Refresh();

            if (process.MainWindowHandle != IntPtr.Zero)
                return process.MainWindowHandle;

            Thread.Sleep(200);
        }

        return IntPtr.Zero;
    }

    // ---------------- MOVE WINDOW ----------------

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    public static void MoveWindow(IntPtr handle, int x, int y, int width, int height, bool maximized = false)
    {
        SetWindowPos(
            handle,
            IntPtr.Zero,
            x,
            y,
            width,
            height,
            SWP_NOZORDER | SWP_NOACTIVATE
        );

        if (maximized)
        {
            ShowWindow(handle, 3); // SW_MAXIMIZE
        }
    }

    // ---------------- FOREGROUND ----------------

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    public static bool IsForeground(IntPtr hWnd)
    {
        return hWnd == GetForegroundWindow();
    }

    public static void DebugForegroundWindow()
    {
        Trace.WriteLine("==== CAPTURE START ====");
        DebugWindow("FOREGROUND", GetForegroundWindow());
    }

    // ---------------- CAPTURE DEBUG ----------------

    public static void DebugWindow(string label, IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
        {
            Trace.WriteLine($"{label} → NULL HANDLE");
            return;
        }

        GetWindowThreadProcessId(hWnd, out uint pid);

        string procName = "UNKNOWN";

        try
        {
            var proc = Process.GetProcessById((int)pid);
            procName = proc.ProcessName;
        }
        catch { }

        var className = GetWindowClassName(hWnd);

        if (!TryGetWindowRect(hWnd, out var r))
        {
            Trace.WriteLine($"{label} → HWND={hWnd} | FAILED RECT");
            return;
        }

        int x = r.Left;
        int y = r.Top;
        int w = r.Right - r.Left;
        int h = r.Bottom - r.Top;

        int monitor = GetMonitorIndexFromWindow(hWnd);

        string fgTag = IsForeground(hWnd) ? " [FOREGROUND]" : "";

        Trace.WriteLine(
            $"{label}{fgTag} → HWND={hWnd} | PID={pid} | PROC={procName} | CLASS={className} | MON={monitor} | {w}x{h} @ ({x},{y})"
        );
    }

    // ---------------- LAUNCH DEBUG ----------------

    public static void DebugLaunchAttempt(string appType, int attempt)
    {
        Trace.WriteLine($"{appType.ToUpper()} scan attempt {attempt}");
    }

    public static void DebugLaunchFailure(string appType)
    {
        Trace.WriteLine($"❌ {appType.ToUpper()} window NOT found after launch");
    }

    public static void DebugBeforeCount(string appType, int count)
    {
        Trace.WriteLine($"{appType.ToUpper()} BEFORE count = {count}");
    }

    // ---------------- APPLY DEBUG ----------------

    public static void DebugApply(string appType, int monitor, int x, int y, int width, int height, bool maximized)
    {
        Trace.WriteLine(
            $"APPLY → {appType} | MON={monitor} | {width}x{height} @ ({x},{y}) | MAX={maximized}"
        );
    }

    // ---------------- WINDOW RECT ----------------

    public static bool TryGetWindowRect(IntPtr handle, out RECT rect)
    {
        return GetWindowRect(handle, out rect);
    }

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // ---------------- MONITOR DETECTION ----------------

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    public static int GetMonitorIndexFromWindow(IntPtr handle)
    {
        var monitor = MonitorFromWindow(handle, MONITOR_DEFAULTTONEAREST);

        var info = new MONITORINFO();
        info.cbSize = Marshal.SizeOf(info);

        if (!GetMonitorInfo(monitor, ref info))
            return 0;

        for (int i = 0; i < Screen.AllScreens.Length; i++)
        {
            var screen = Screen.AllScreens[i];

            if (screen.Bounds.Left == info.rcMonitor.Left &&
                screen.Bounds.Top == info.rcMonitor.Top)
            {
                return i;
            }
        }

        return 0;
    }

    // ---------------- ENUM + HELPERS ----------------

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "IsWindowVisible")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisibleNative(IntPtr hWnd);

    public static bool IsWindowVisible(IntPtr handle)
    {
        return IsWindowVisibleNative(handle);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    public static string GetWindowClassName(IntPtr handle)
    {
        var sb = new StringBuilder(256);
        GetClassName(handle, sb, sb.Capacity);
        return sb.ToString();
    }

    public static IntPtr FindWindowByProcessName(string processName)
    {
        IntPtr found = IntPtr.Zero;

        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd))
                return true;

            GetWindowThreadProcessId(hWnd, out uint pid);

            try
            {
                var proc = Process.GetProcessById((int)pid);

                if (proc.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    found = hWnd;
                    return false;
                }
            }
            catch { }

            return true;

        }, IntPtr.Zero);

        return found;
    }

    // ---------------- WIN32 ----------------

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags
    );

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
}