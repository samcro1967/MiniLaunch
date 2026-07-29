using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;

public static class WindowHelpers
{
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

    public static void MoveWindow(IntPtr handle, int x, int y, int width, int height, bool maximized = false)
    {
        SetWindowPos(handle, IntPtr.Zero, x, y, width, height, 0);

        if (maximized)
        {
            ShowWindow(handle, 3); // SW_MAXIMIZE
        }
    }

    // ---------------- FALLBACK ----------------

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

    // ---------------- NEW: WINDOW RECT SUPPORT ----------------

    public static bool TryGetWindowRect(IntPtr handle, out RECT rect)
    {
        return GetWindowRect(handle, out rect);
    }

    // 🔥 Optional cleaner version (future use)
    public static bool TryGetWindowBounds(IntPtr handle, out Rectangle bounds)
    {
        if (GetWindowRect(handle, out var r))
        {
            bounds = new Rectangle(
                r.Left,
                r.Top,
                r.Right - r.Left,
                r.Bottom - r.Top
            );
            return true;
        }

        bounds = Rectangle.Empty;
        return false;
    }

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // ---------------- WIN32 ----------------

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

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