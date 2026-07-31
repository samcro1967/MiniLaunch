using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

public class ExplorerApp : IAppModule
{
    public string Type => "explorer";

    public string DisplayName => "Windows Explorer";

    // ----------------- CAPTURE -----------------

    public bool TryCapture(out AppConfig? app)
    {
        app = null;

        IntPtr selected = IntPtr.Zero;

        WindowHelpers.EnumWindows((hWnd, lParam) =>
        {
            if (!WindowHelpers.IsWindowVisible(hWnd))
                return true;

            if (!WindowHelpers.TryGetWindowRect(hWnd, out var r))
                return true;

            int width = r.Right - r.Left;
            int height = r.Bottom - r.Top;

            if (width < 300 || height < 300)
                return true;

            WindowHelpers.GetWindowThreadProcessId(hWnd, out uint pid);

            try
            {
                var proc = Process.GetProcessById((int)pid);

                if (!proc.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            catch
            {
                return true;
            }

            var className = WindowHelpers.GetWindowClassName(hWnd);

            if (!className.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
                return true;

            WindowHelpers.DebugWindow("EXPLORER CANDIDATE", hWnd);

            selected = hWnd;
            return false;

        }, IntPtr.Zero);

        if (selected == IntPtr.Zero)
        {
            WindowHelpers.DebugWindow("EXPLORER SELECTED", selected);
            return false;
        }

        WindowHelpers.DebugWindow("EXPLORER SELECTED", selected);

        if (!WindowHelpers.TryGetWindowRect(selected, out var rect))
        {
            WindowHelpers.DebugWindow("EXPLORER FINAL FAILED", selected);
            return false;
        }

        WindowHelpers.DebugWindow("EXPLORER FINAL", selected);

        // 🔥 NEW: RELATIVE CAPTURE
        var screen = Screen.FromHandle(selected);
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
        var before = new HashSet<IntPtr>();

        WindowHelpers.EnumWindows((hWnd, lParam) =>
        {
            if (!WindowHelpers.IsWindowVisible(hWnd))
                return true;

            var className = WindowHelpers.GetWindowClassName(hWnd);

            if (className.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
            {
                before.Add(hWnd);
            }

            return true;

        }, IntPtr.Zero);

        WindowHelpers.DebugBeforeCount(Type, before.Count);

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = string.IsNullOrWhiteSpace(app.Path)
                ? "/n"
                : $"/n, \"{app.Path}\"",
            UseShellExecute = true
        });

        IntPtr handle = IntPtr.Zero;

        for (int i = 0; i < 30; i++)
        {
            WindowHelpers.DebugLaunchAttempt(Type, i);

            WindowHelpers.EnumWindows((hWnd, lParam) =>
            {
                if (!WindowHelpers.IsWindowVisible(hWnd))
                    return true;

                var className = WindowHelpers.GetWindowClassName(hWnd);

                if (!className.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
                    return true;

                if (!before.Contains(hWnd))
                {
                    handle = hWnd;
                    return false;
                }

                return true;

            }, IntPtr.Zero);

            if (handle != IntPtr.Zero)
                break;

            Thread.Sleep(100);
        }

        if (handle != IntPtr.Zero)
        {
            WindowHelpers.DebugWindow("EXPLORER LAUNCH HANDLE", handle);

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

            // 🔥 Explorer override protection
            WindowHelpers.MoveWindow(
                handle,
                finalX,
                finalY,
                app.Width,
                app.Height,
                app.Maximized
            );

            Thread.Sleep(100);

            WindowHelpers.DebugWindow("EXPLORER AFTER MOVE", handle);
        }
        else
        {
            WindowHelpers.DebugLaunchFailure(Type);
        }
    }
}