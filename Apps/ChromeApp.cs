using System.Diagnostics;
using System.Linq;
using System.Threading;

public class ChromeApp : IAppModule
{
    public string Type => "chrome";

    public string DisplayName => "Google Chrome / Microsoft Edge";

    // 🔥 NEW: determine if Chrome should be captured
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

        // 🔥 Build base config (position will be overwritten by ProfileService later if needed)
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
        // 🔥 future: capture URLs if you want
        app.Urls = new List<string>();
    }

    public void Launch(AppConfig app)
    {
        string args = "--new-window";

        if (app.Urls?.Any() == true)
            args += " " + string.Join(" ", app.Urls);

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "chrome.exe",
            Arguments = args,
            UseShellExecute = true
        });

        if (process == null) return;

        var handle = WindowHelpers.WaitForMainWindow(process);

        // Chrome fallback
        if (handle == IntPtr.Zero)
        {
            Thread.Sleep(1000);
            handle = WindowHelpers.FindWindowByProcessName("chrome");
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
}