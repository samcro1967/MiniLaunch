using System.Diagnostics;
using System.Linq;
using MiniLaunch.Core;

public class TerminalApp : BaseProcessAppModule
{
    public override string Type => "terminal";

    public override string DisplayName => "Windows Terminal";

    protected override string ProcessName => "wt"; // used for launch only

    // ----------------- CAPTURE -----------------

    public override bool TryCapture(out AppConfig? app)
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

    public override void EnrichCaptured(AppConfig app)
    {
        app.Tabs = new List<string>();
    }

    // ----------------- LAUNCH ARGS -----------------

    protected override string BuildLaunchArguments(AppConfig app)
    {
        if (app.Tabs?.Any() != true)
            return "";

        return string.Join(" ; ",
            app.Tabs.Select(t => $"new-tab {t}"));
    }

    // ----------------- LAUNCH -----------------

    public override void Launch(AppConfig app)
    {
        string args = BuildLaunchArguments(app);

        WindowLaunchHelper.LaunchAndPosition(
            type: Type,

            // 🔥 custom process detection (required)
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
                    Arguments = args,
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