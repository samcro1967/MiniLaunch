using System.Diagnostics;
using System.Linq;
using System.Threading;

public class NotepadApp : IAppModule
{
    public string Type => "notepad";

    public string DisplayName => "Notepad++";

    // 🔥 NEW: determine if Notepad++ should be captured
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

        app = new AppConfig
        {
            Type = Type,
            X = 100,
            Y = 100,
            Width = 1200,
            Height = 800
        };

        return true;
    }

    public void EnrichCaptured(AppConfig app)
    {
        app.Files = new List<string>();
        app.Session = null; // 🔥 better than ""
    }

    public void Launch(AppConfig app)
    {
        string exe = ResolveNotepadPath();

        Process? process = null;

        if (!string.IsNullOrWhiteSpace(app.Session))
        {
            process = Process.Start(exe, $"-openSession \"{app.Session}\"");
        }
        else if (app.Files?.Any() == true)
        {
            process = Process.Start(
                exe,
                string.Join(" ", app.Files.Select(f => $"\"{f}\""))
            );
        }
        else
        {
            process = Process.Start(exe);
        }

        if (process == null) return;

        var handle = WindowHelpers.WaitForMainWindow(process);

        // 🔥 fallback (rare but safe)
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
                app.Height
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