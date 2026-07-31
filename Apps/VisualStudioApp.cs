using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

public class VisualStudioApp : IAppModule
{
    public string Type => "visualstudio";

    public string DisplayName => "Visual Studio";

    // ----------------- CAPTURE -----------------

    public bool TryCapture(out AppConfig? app)
    {
        app = null;

        var proc = Process.GetProcessesByName("devenv")
            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);

        if (proc == null)
            return false;

        var handle = proc.MainWindowHandle;

        if (handle == IntPtr.Zero)
            return false;

        WindowHelpers.DebugWindow("VS CAPTURE", handle);

        if (!WindowHelpers.TryGetWindowRect(handle, out var rect))
        {
            WindowHelpers.DebugWindow("VS RECT FAILED", handle);
            return false;
        }

        WindowHelpers.DebugWindow("VS FINAL", handle);

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
        app.Path = "";
    }

    // ----------------- LAUNCH -----------------

    public void Launch(AppConfig app)
    {
        string args = string.IsNullOrWhiteSpace(app.Path)
            ? ""
            : $"\"{app.Path}\"";

        var before = Process.GetProcessesByName("devenv")
            .Where(p => p.MainWindowHandle != IntPtr.Zero)
            .Select(p => p.MainWindowHandle)
            .ToHashSet();

        WindowHelpers.DebugBeforeCount(Type, before.Count);

        Process.Start(new ProcessStartInfo
        {
            FileName = "devenv.exe",
            Arguments = args,
            UseShellExecute = true
        });

        IntPtr handle = IntPtr.Zero;

        // 🔥 detection loop (skip splash)
        for (int i = 0; i < 40; i++)
        {
            WindowHelpers.DebugLaunchAttempt(Type, i);

            var after = Process.GetProcessesByName("devenv")
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .Select(p => p.MainWindowHandle)
                .ToList();

            foreach (var h in after)
            {
                if (before.Contains(h))
                    continue;

                var className = WindowHelpers.GetWindowClassName(h);

                if (className.Contains("VSSplash"))
                    continue;

                handle = h;
                break;
            }

            if (handle != IntPtr.Zero)
                break;

            Thread.Sleep(150);
        }

        // 🔥 fallback (non-splash)
        if (handle == IntPtr.Zero)
        {
            var existing = Process.GetProcessesByName("devenv")
                .FirstOrDefault(p =>
                    p.MainWindowHandle != IntPtr.Zero &&
                    !WindowHelpers.GetWindowClassName(p.MainWindowHandle).Contains("VSSplash"));

            if (existing != null)
                handle = existing.MainWindowHandle;
        }

        if (handle != IntPtr.Zero)
        {
            WindowHelpers.DebugWindow("VS LAUNCH HANDLE", handle);

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

            WindowHelpers.DebugWindow("VS AFTER MOVE", handle);
        }
        else
        {
            WindowHelpers.DebugLaunchFailure(Type);
        }
    }
}