using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

public class ChromeApp : IAppModule
{
    public string Type => "chrome";

    public string DisplayName => "Google Chrome / Microsoft Edge";

    // ----------------- CAPTURE -----------------

    public bool TryCapture(out AppConfig? app)
    {
        app = null;

        var proc = Process.GetProcessesByName("chrome")
            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);

        if (proc == null)
            return false;

        var handle = proc.MainWindowHandle;

        if (handle == IntPtr.Zero)
            return false;

        WindowHelpers.DebugWindow("CHROME CAPTURE", handle);

        if (!WindowHelpers.TryGetWindowRect(handle, out var rect))
        {
            WindowHelpers.DebugWindow("CHROME RECT FAILED", handle);
            return false;
        }

        WindowHelpers.DebugWindow("CHROME FINAL", handle);

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
        app.Urls = new List<string>();
    }

    // ----------------- LAUNCH -----------------

    public void Launch(AppConfig app)
    {
        string args = "--new-window";

        if (app.Urls?.Any() == true)
            args += " " + string.Join(" ", app.Urls);

        var before = Process.GetProcessesByName("chrome")
            .Where(p => p.MainWindowHandle != IntPtr.Zero)
            .Select(p => p.MainWindowHandle)
            .ToHashSet();

        WindowHelpers.DebugBeforeCount(Type, before.Count);

        Process.Start(new ProcessStartInfo
        {
            FileName = "chrome.exe",
            Arguments = args,
            UseShellExecute = true
        });

        IntPtr handle = IntPtr.Zero;

        for (int i = 0; i < 30; i++)
        {
            WindowHelpers.DebugLaunchAttempt(Type, i);

            var after = Process.GetProcessesByName("chrome")
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .Select(p => p.MainWindowHandle)
                .ToList();

            handle = after.FirstOrDefault(h => !before.Contains(h));

            if (handle != IntPtr.Zero)
                break;

            Thread.Sleep(100);
        }

        if (handle == IntPtr.Zero)
        {
            Thread.Sleep(500);
            handle = WindowHelpers.FindWindowByProcessName("chrome");
        }

        if (handle != IntPtr.Zero)
        {
            WindowHelpers.DebugWindow("CHROME LAUNCH HANDLE", handle);

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

            Thread.Sleep(100);

            WindowHelpers.DebugWindow("CHROME AFTER MOVE", handle);
        }
        else
        {
            WindowHelpers.DebugLaunchFailure(Type);
        }
    }
}