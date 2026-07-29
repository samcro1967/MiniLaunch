using System.Diagnostics;
using System.Linq;
using System.Threading;

public class ExplorerApp : IAppModule
{
    public string Type => "explorer";

    public string DisplayName => "Windows Explorer";

    // 🔥 Explorer decides if it exists
    public bool TryCapture(out AppConfig? app)
    {
        app = null;

        var explorerWindows = Process.GetProcessesByName("explorer")
            .Where(p => p.MainWindowHandle != IntPtr.Zero)
            .ToList();

        if (explorerWindows.Count == 0)
            return false;

        var proc = explorerWindows
            .OrderByDescending(p =>
            {
                try { return p.StartTime; }
                catch { return DateTime.MinValue; }
            })
            .First();

        var handle = proc.MainWindowHandle;

        if (handle == IntPtr.Zero)
            return false;

        app = new AppConfig
        {
            Type = Type,
            Path = GetExplorerPath(proc) ?? "",
            X = 100,
            Y = 100,
            Width = 1200,
            Height = 800
        };

        return true;
    }

    public void EnrichCaptured(AppConfig app)
    {
        // 🔥 Nothing to enrich yet
        // Future: path refinement, multi-window support, etc.
    }

    private string? GetExplorerPath(Process process)
    {
        try
        {
            // 🔥 Placeholder (safe fallback)
            return null;
        }
        catch
        {
            return null;
        }
    }

    public void Launch(AppConfig app)
    {
        var path = string.IsNullOrWhiteSpace(app.Path)
            ? ""
            : app.Path;

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = path,
            UseShellExecute = true
        });

        IntPtr handle = IntPtr.Zero;

        if (process != null)
        {
            handle = WindowHelpers.WaitForMainWindow(process);
        }

        // 🔥 Explorer fallback
        if (handle == IntPtr.Zero)
        {
            Thread.Sleep(500);

            var explorer = Process.GetProcessesByName("explorer")
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .OrderByDescending(p =>
                {
                    try { return p.StartTime; }
                    catch { return DateTime.MinValue; }
                })
                .FirstOrDefault();

            if (explorer != null)
            {
                handle = explorer.MainWindowHandle;
            }
        }

        if (handle != IntPtr.Zero)
        {
            WindowHelpers.MoveWindow(
                handle,
                app.X,
                app.Y,
                app.Width,
                app.Height
            );
        }
    }
}