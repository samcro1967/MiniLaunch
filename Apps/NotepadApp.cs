using System.Diagnostics;
using System.Linq;
using MiniLaunch.Core;

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
        {
            Log.WriteCategory("CAPTURE", "notepad | no window found");
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
        app.Session = "";
    }

    // ----------------- LAUNCH -----------------

    public void Launch(AppConfig app)
    {
        string exe = ResolveNotepadPath();

        string args = string.IsNullOrWhiteSpace(app.Session)
            ? ""
            : $"-openSession \"{app.Session}\"";

        WindowLaunchHelper.LaunchAndPosition(
            type: Type,

            getExistingWindows: () =>
                Process.GetProcessesByName("notepad++")
                    .Where(p => p.MainWindowHandle != IntPtr.Zero)
                    .Select(p => p.MainWindowHandle)
                    .ToHashSet(),

            getCurrentWindows: () =>
                Process.GetProcessesByName("notepad++")
                    .Where(p => p.MainWindowHandle != IntPtr.Zero)
                    .Select(p => p.MainWindowHandle)
                    .ToList(),

            startProcess: () =>
            {
                var process = Process.Start(exe, args);

                if (process == null)
                {
                    Log.WriteCategory("LAUNCH", "notepad | failed to start process");
                }
            },

            app: app
        );
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