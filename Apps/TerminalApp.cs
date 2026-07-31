using System.Diagnostics;
using System.Linq;
using MiniLaunch.Core;

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
        {
            Log.WriteCategory("CAPTURE", "terminal | no window found");
            return false;
        }

        return WindowCaptureHelper.TryCaptureWindow(
            type: Type,
            handle: proc.MainWindowHandle,
            out app
        );
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

        WindowLaunchHelper.LaunchAndPosition(
            type: Type,

            getExistingWindows: () =>
                Process.GetProcesses()
                    .Where(p => IsTerminalProcess(p) && p.MainWindowHandle != IntPtr.Zero)
                    .Select(p => p.MainWindowHandle)
                    .ToHashSet(),

            getCurrentWindows: () =>
                Process.GetProcesses()
                    .Where(p => IsTerminalProcess(p) && p.MainWindowHandle != IntPtr.Zero)
                    .Select(p => p.MainWindowHandle)
                    .ToList(),

            startProcess: () =>
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "wt",
                    Arguments = args ?? "",
                    UseShellExecute = true
                });

                if (process == null)
                {
                    Log.WriteCategory("LAUNCH", "terminal | failed to start process");
                }
            },

            app: app,

            doubleMove: true // 🔥 REQUIRED for Terminal
        );
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