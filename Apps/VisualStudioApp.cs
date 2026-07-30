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

        // 🔥 REAL WINDOW CAPTURE (fix)
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
        // ✅ Placeholder for solution path
        app.Path = "";
    }

    // ----------------- LAUNCH -----------------

    public void Launch(AppConfig app)
    {
        string args = string.IsNullOrWhiteSpace(app.Path)
            ? ""
            : $"\"{app.Path}\"";

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "devenv.exe",
            Arguments = args,
            UseShellExecute = true
        });

        if (process == null)
            return;

        var handle = WindowHelpers.WaitForMainWindow(process);

        if (handle == IntPtr.Zero)
        {
            Thread.Sleep(1000);

            var vs = Process.GetProcessesByName("devenv")
                .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);

            if (vs != null)
                handle = vs.MainWindowHandle;
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