using System.Diagnostics;
using System.Linq;
using System.Threading;

public class VisualStudioApp : IAppModule
{
    public string Type => "visualstudio";

    public string DisplayName => "Visual Studio";

    // 🔥 Detect if Visual Studio is running
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

        app = new AppConfig
        {
            Type = Type,
            X = 100,
            Y = 100,
            Width = 1400,
            Height = 900,
            Maximized = false,

            // 🔥 future support
            Path = GetSolutionPath(proc)
        };

        return true;
    }

    public void EnrichCaptured(AppConfig app)
    {
        // nothing extra yet
        // later: solution parsing improvements
    }

    public void Launch(AppConfig app)
    {
        string args = "";

        if (!string.IsNullOrWhiteSpace(app.Path))
        {
            args = $"\"{app.Path}\"";
        }

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

    // 🔥 Attempt to extract solution path (basic version)
    private string? GetSolutionPath(Process process)
    {
        try
        {
            // Basic fallback — real extraction is complex
            // You can improve later with window title parsing
            return null;
        }
        catch
        {
            return null;
        }
    }
}