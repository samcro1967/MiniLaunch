using System.Diagnostics;
using System.Linq;
using System.Threading;
using MiniLaunch.Core;

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
        {
            Log.WriteCategory("CAPTURE", "visualstudio | no window found");
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
        app.Path = "";
    }

    // ----------------- LAUNCH -----------------

    public void Launch(AppConfig app)
    {
        string args = string.IsNullOrWhiteSpace(app.Path)
            ? ""
            : $"\"{app.Path}\"";

        var before = Process.GetProcessesByName("devenv")
            .Where(p => p.MainWindowHandle != IntPtr.Zero)
            .Select(p => p.MainWindowHandle)
            .ToHashSet();

        WindowHelpers.DebugBeforeCount(Type, before.Count);

        Log.WriteCategory("LAUNCH", "visualstudio | starting");

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "devenv.exe",
            Arguments = args,
            UseShellExecute = true
        });

        if (process == null)
        {
            Log.WriteCategory("LAUNCH", "visualstudio | failed to start process");
            return;
        }

        IntPtr handle = IntPtr.Zero;

        // 🔥 Custom detection loop (skip splash)
        for (int i = 0; i < 40; i++)
        {
            WindowHelpers.DebugLaunchAttempt(Type, i);

            var after = Process.GetProcessesByName("devenv")
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .Select(p => p.MainWindowHandle)
                .ToList();

            foreach (var h in after)
            {
                if (before.Contains(h))
                    continue;

                var className = WindowHelpers.GetWindowClassName(h);

                // ❌ Skip splash screen
                if (className.Contains("VSSplash"))
                    continue;

                handle = h;
                break;
            }

            if (handle != IntPtr.Zero)
                break;

            Thread.Sleep(150);
        }

        // 🔥 fallback (non-splash)
        if (handle == IntPtr.Zero)
        {
            Log.WriteCategory("LAUNCH", "visualstudio | no new window found, using fallback");

            var existing = Process.GetProcessesByName("devenv")
                .FirstOrDefault(p =>
                    p.MainWindowHandle != IntPtr.Zero &&
                    !WindowHelpers.GetWindowClassName(p.MainWindowHandle).Contains("VSSplash"));

            if (existing != null)
                handle = existing.MainWindowHandle;
        }

        if (handle != IntPtr.Zero)
        {
            WindowHelpers.DebugWindow("VS LAUNCH HANDLE", handle);

            // 🔥 REL → ABS conversion (standardized)
            var screen = Screen.AllScreens[app.Monitor];

            int finalX = screen.Bounds.Left + app.X;
            int finalY = screen.Bounds.Top + app.Y;

            WindowHelpers.DebugApply(
                app.Type,
                app.Monitor,
                app.X,
                app.Y,
                finalX,
                finalY,
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

            WindowHelpers.DebugWindow("VS AFTER MOVE", handle);
        }
        else
        {
            WindowHelpers.DebugLaunchFailure(Type);
        }
    }
}