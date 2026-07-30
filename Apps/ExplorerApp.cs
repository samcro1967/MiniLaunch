using System.Diagnostics;

public class ExplorerApp : IAppModule
{
    public string Type => "explorer";

    public string DisplayName => "Windows Explorer";

    public bool TryCapture(out AppConfig? app)
    {
        app = null;

        Trace.WriteLine("==== Explorer Capture START ====");

        IntPtr selected = IntPtr.Zero;
        int bestArea = 0;

        // 🔥 Correct detection pipeline
        WindowHelpers.EnumWindows((hWnd, lParam) =>
        {
            if (!WindowHelpers.IsWindowVisible(hWnd))
                return true;

            if (!WindowHelpers.TryGetWindowRect(hWnd, out var r))
                return true;

            int width = r.Right - r.Left;
            int height = r.Bottom - r.Top;

            // 🔥 Filter junk FIRST (before process check)
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

            int area = width * height;

            Trace.WriteLine($"Candidate → {width}x{height} @ ({r.Left},{r.Top}) PID={pid}");

            if (area > bestArea)
            {
                bestArea = area;
                selected = hWnd;
            }

            return true;

        }, IntPtr.Zero);

        if (selected == IntPtr.Zero)
        {
            Trace.WriteLine("❌ Explorer capture FAILED");
            return false;
        }

        if (!WindowHelpers.TryGetWindowRect(selected, out var rect))
        {
            Trace.WriteLine("❌ RECT FAILED");
            return false;
        }

        app = new AppConfig
        {
            Type = Type,
            X = rect.Left,
            Y = rect.Top,
            Width = rect.Right - rect.Left,
            Height = rect.Bottom - rect.Top,
            Maximized = false,
            Monitor = WindowHelpers.GetMonitorIndexFromWindow(selected)
        };

        Trace.WriteLine("✔ Explorer captured (ENUM method)");

        return true;
    }

    public void EnrichCaptured(AppConfig app)
    {
        app.Path = "";
    }

    public void Launch(AppConfig app)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = app.Path ?? "",
            UseShellExecute = true
        });

        if (process == null)
            return;

        var handle = WindowHelpers.WaitForMainWindow(process);

        if (handle != IntPtr.Zero)
        {
            WindowHelpers.MoveWindow(
                handle,
                app.X,
                app.Y,
                app.Width,
                app.Height,
                app.Maximized
            );
        }
    }
}