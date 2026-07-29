using System.Diagnostics;
using System.Linq;
using System.Threading;

public class TerminalApp : IAppModule
{
    public string Type => "terminal";

    public string DisplayName => "Windows Terminal";

    // 🔥 NEW: determine if Terminal should be captured
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

        app = new AppConfig
        {
            Type = Type,
            X = 100,
            Y = 100,
            Width = 1200,
            Height = 800,
            Maximized = false
        };

        return true;
    }

    public void EnrichCaptured(AppConfig app)
    {
        app.Tabs = new List<string>();
    }

    public void Launch(AppConfig app)
    {
        string args = "";

        if (app.Tabs?.Any() == true)
        {
            args = string.Join(" ; ",
                app.Tabs.Select(t => $"new-tab {t}"));
        }

        // capture existing terminal windows BEFORE launch
        var before = Process.GetProcesses()
            .Where(p => IsTerminalProcess(p) && p.MainWindowHandle != IntPtr.Zero)
            .Select(p => p.MainWindowHandle)
            .ToHashSet();

        Process.Start(new ProcessStartInfo
        {
            FileName = "wt",
            Arguments = args,
            UseShellExecute = true
        });

        Thread.Sleep(800);

        IntPtr handle = IntPtr.Zero;

        // find NEW window
        for (int i = 0; i < 30; i++)
        {
            var after = Process.GetProcesses()
                .Where(p => IsTerminalProcess(p) && p.MainWindowHandle != IntPtr.Zero)
                .Select(p => p.MainWindowHandle)
                .ToList();

            handle = after.FirstOrDefault(h => !before.Contains(h));

            if (handle != IntPtr.Zero)
                break;

            Thread.Sleep(100);
        }

        // fallback: grab ANY terminal window
        if (handle == IntPtr.Zero)
        {
            var existing = Process.GetProcesses()
                .FirstOrDefault(p => IsTerminalProcess(p) && p.MainWindowHandle != IntPtr.Zero);

            if (existing != null)
                handle = existing.MainWindowHandle;
        }

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