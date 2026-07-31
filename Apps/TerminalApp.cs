using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

public class TerminalApp : IAppModule
{
    public string Type => "terminal";

    public string DisplayName => "Windows Terminal";

    // ----------------- CAPTURE -----------------

    public bool TryCapture(out AppConfig? app)
    {
        app = null;

        var proc = Process.GetProcesses()
            .FirstOrDefault(p => IsTerminalProcess(p) && p.MainWindowHandle != IntPtr.Zero);

        if (proc == null)
            return false;

        var handle = proc.MainWindowHandle;

        if (handle == IntPtr.Zero)
            return false;

        WindowHelpers.DebugWindow("TERMINAL CAPTURE", handle);

        if (!WindowHelpers.TryGetWindowRect(handle, out var rect))
        {
            WindowHelpers.DebugWindow("TERMINAL RECT FAILED", handle);
            return false;
        }

        WindowHelpers.DebugWindow("TERMINAL FINAL", handle);

        // 🔥 NEW: RELATIVE CAPTURE
        var screen = Screen.FromHandle(handle);
        int monitorIndex = Array.IndexOf(Screen.AllScreens, screen);

        int relativeX = rect.Left - screen.Bounds.Left;
        int relativeY = rect.Top - screen.Bounds.Top;

        app = new AppConfig
        {
            Type = Type,
            X = relativeX,
            Y = relativeY,
            Width = rect.Right - rect.Left,
            Height = rect.Bottom - rect.Top,
            Maximized = false,
            Monitor = monitorIndex
        };

        return true;
    }

    // ----------------- ENRICH -----------------

    public void EnrichCaptured(AppConfig app)
    {
        app.Tabs = new List<string>();
    }

    // ----------------- LAUNCH -----------------

    public void Launch(AppConfig app)
    {
        string? args = null;

        if (app.Tabs?.Any() == true)
        {
            args = string.Join(" ; ",
                app.Tabs.Select(t => $"new-tab {t}"));
        }

        var before = Process.GetProcesses()
            .Where(p => IsTerminalProcess(p) && p.MainWindowHandle != IntPtr.Zero)
            .Select(p => p.MainWindowHandle)
            .ToHashSet();

        WindowHelpers.DebugBeforeCount(Type, before.Count);

        Process.Start(new ProcessStartInfo
        {
            FileName = "wt",
            Arguments = args ?? "",
            UseShellExecute = true
        });

        IntPtr handle = IntPtr.Zero;

        for (int i = 0; i < 30; i++)
        {
            WindowHelpers.DebugLaunchAttempt(Type, i);

            var after = Process.GetProcesses()
                .Where(p => IsTerminalProcess(p) && p.MainWindowHandle != IntPtr.Zero)
                .Select(p => p.MainWindowHandle)
                .ToList();

            handle = after.FirstOrDefault(h => !before.Contains(h));

            if (handle != IntPtr.Zero)
                break;

            Thread.Sleep(100);
        }

        if (handle == IntPtr.Zero)
        {
            var existing = Process.GetProcesses()
                .FirstOrDefault(p => IsTerminalProcess(p) && p.MainWindowHandle != IntPtr.Zero);

            if (existing != null)
                handle = existing.MainWindowHandle;
        }

        if (handle != IntPtr.Zero)
        {
            WindowHelpers.DebugWindow("TERMINAL LAUNCH HANDLE", handle);

            // 🔥 NEW: CONVERT TO ABSOLUTE
            var screen = Screen.AllScreens[app.Monitor];

            int finalX = screen.Bounds.Left + app.X;
            int finalY = screen.Bounds.Top + app.Y;

            WindowHelpers.DebugApply(
                app.Type,
                app.Monitor,
                app.X,
                app.Y,
                app.Width,
                app.Height,
                app.Maximized
            );

            WindowHelpers.MoveWindow(
                handle,
                finalX,
                finalY,
                app.Width,
                app.Height,
                app.Maximized
            );

            Thread.Sleep(200);

            // 🔥 Terminal override protection
            WindowHelpers.MoveWindow(
                handle,
                finalX,
                finalY,
                app.Width,
                app.Height,
                app.Maximized
            );

            Thread.Sleep(100);

            WindowHelpers.DebugWindow("TERMINAL AFTER MOVE", handle);
        }
        else
        {
            WindowHelpers.DebugLaunchFailure(Type);
        }
    }

    // ----------------- HELPERS -----------------

    private bool IsTerminalProcess(Process p)
    {
        try
        {
            return p.ProcessName.Equals("WindowsTerminal", StringComparison.OrdinalIgnoreCase)
                || p.ProcessName.Equals("wt", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}