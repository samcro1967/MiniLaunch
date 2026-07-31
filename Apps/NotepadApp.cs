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

        WindowHelpers.DebugWindow("NOTEPAD CAPTURE", handle);

        if (!WindowHelpers.TryGetWindowRect(handle, out var rect))
        {
            WindowHelpers.DebugWindow("NOTEPAD RECT FAILED", handle);
            return false;
        }

        WindowHelpers.DebugWindow("NOTEPAD FINAL", handle);

        // 🔥 NEW: RELATIVE CAPTURE
        var screen = Screen.FromHandle(handle);
        int monitorIndex = Array.IndexOf(Screen.AllScreens, screen);

        int relativeX = rect.Left - screen.Bounds.Left;
        int relativeY = rect.Top - screen.Bounds.Top;

        app = new AppConfig
        {
            Type = Type,
            X = relativeX,
            Y = relativeY,
            Width = rect.Right - rect.Left,
            Height = rect.Bottom - rect.Top,
            Maximized = false,
            Monitor = monitorIndex
        };

        return true;
    }

    // ----------------- ENRICH -----------------

    public void EnrichCaptured(AppConfig app)
    {
        app.Session = "";
    }

    // ----------------- LAUNCH -----------------

    public void Launch(AppConfig app)
    {
        string exe = ResolveNotepadPath();

        var before = Process.GetProcessesByName("notepad++")
            .Where(p => p.MainWindowHandle != IntPtr.Zero)
            .Select(p => p.MainWindowHandle)
            .ToHashSet();

        WindowHelpers.DebugBeforeCount(Type, before.Count);

        Process? process;

        if (!string.IsNullOrWhiteSpace(app.Session))
        {
            process = Process.Start(exe, $"-openSession \"{app.Session}\"");
        }
        else
        {
            process = Process.Start(exe);
        }

        if (process == null)
            return;

        IntPtr handle = IntPtr.Zero;

        for (int i = 0; i < 30; i++)
        {
            WindowHelpers.DebugLaunchAttempt(Type, i);

            var after = Process.GetProcessesByName("notepad++")
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .Select(p => p.MainWindowHandle)
                .ToList();

            handle = after.FirstOrDefault(h => !before.Contains(h));

            if (handle != IntPtr.Zero)
                break;

            Thread.Sleep(100);
        }

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
            WindowHelpers.DebugWindow("NOTEPAD LAUNCH HANDLE", handle);

            // 🔥 NEW: CONVERT TO ABSOLUTE
            var screen = Screen.AllScreens[app.Monitor];

            int finalX = screen.Bounds.Left + app.X;
            int finalY = screen.Bounds.Top + app.Y;

            WindowHelpers.DebugApply(
                app.Type,
                app.Monitor,
                app.X,
                app.Y,
                app.Width,
                app.Height,
                app.Maximized
            );

            WindowHelpers.MoveWindow(
                handle,
                finalX,
                finalY,
                app.Width,
                app.Height,
                app.Maximized
            );

            Thread.Sleep(100);

            WindowHelpers.DebugWindow("NOTEPAD AFTER MOVE", handle);
        }
        else
        {
            WindowHelpers.DebugLaunchFailure(Type);
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