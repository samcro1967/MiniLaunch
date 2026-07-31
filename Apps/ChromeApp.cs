using System.Diagnostics;
using System.Linq;
using MiniLaunch.Core;


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
        {
            Log.WriteCategory("CAPTURE", "chrome | no window found");
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
        app.Urls = new List<string>();
    }

    // ----------------- LAUNCH -----------------

    public void Launch(AppConfig app)
    {
        string args = "--new-window";

        if (app.Urls?.Any() == true)
            args += " " + string.Join(" ", app.Urls);

        WindowLaunchHelper.LaunchAndPosition(
            type: Type,

            getExistingWindows: () =>
                Process.GetProcessesByName("chrome")
                    .Where(p => p.MainWindowHandle != IntPtr.Zero)
                    .Select(p => p.MainWindowHandle)
                    .ToHashSet(),

            getCurrentWindows: () =>
                Process.GetProcessesByName("chrome")
                    .Where(p => p.MainWindowHandle != IntPtr.Zero)
                    .Select(p => p.MainWindowHandle)
                    .ToList(),

            startProcess: () =>
                Process.Start(new ProcessStartInfo
                {
                    FileName = "chrome.exe",
                    Arguments = args,
                    UseShellExecute = true
                }),

            app: app
        );
    }
}