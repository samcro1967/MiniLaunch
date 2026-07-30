using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

public class NotepadApp : IAppModule
{
    public string Type => "notepad";

    public string DisplayName => "Notepad++";

    // ----------------- CAPTURE -----------------

    public bool TryCapture(out AppConfig? app)
    {
        app = null;

        var proc = Process.GetProcessesByName("notepad++")
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
        // ✅ Session-only placeholder
        app.Session = "";
    }

    // ----------------- LAUNCH -----------------

    public void Launch(AppConfig app)
    {
        string exe = ResolveNotepadPath();

        Process? process;

        if (!string.IsNullOrWhiteSpace(app.Session))
        {
            process = Process.Start(exe, $"-openSession \"{app.Session}\"");
        }
        else
        {
            process = Process.Start(exe);
        }

        if (process == null) return;

        var handle = WindowHelpers.WaitForMainWindow(process);

        // 🔥 fallback
        if (handle == IntPtr.Zero)
        {
            Thread.Sleep(500);

            var np = Process.GetProcessesByName("notepad++")
                .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);

            if (np != null)
                handle = np.MainWindowHandle;
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

    private string ResolveNotepadPath()
    {
        var paths = new[]
        {
            @"C:\Program Files\Notepad++\notepad++.exe",
            @"C:\Program Files (x86)\Notepad++\notepad++.exe"
        };

        return paths.FirstOrDefault(File.Exists) ?? "notepad++.exe";
    }
}