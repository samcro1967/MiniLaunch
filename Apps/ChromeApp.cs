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

        // 🔥 REAL WINDOW CAPTURE (this is the fix)
        if (!WindowHelpers.TryGetWindowRect(handle, out var rect))
            return false;

        app = new AppConfig
        {
            Type = Type,
            X = rect.Left,
            Y = rect.Top,
            Width = rect.Right - rect.Left,
            Height = rect.Bottom - rect.Top,
            Maximized = false,
            Monitor = WindowHelpers.GetMonitorIndexFromWindow(handle)
        };

        return true;
    }

    // ----------------- ENRICH -----------------

    public void EnrichCaptured(AppConfig app)
    {
        // ✅ Placeholder for user configuration
        app.Urls = new List<string>();
    }

    // ----------------- LAUNCH -----------------

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