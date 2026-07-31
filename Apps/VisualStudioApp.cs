using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MiniLaunch.Core;

public class VisualStudioApp : BaseProcessAppModule
{
    public override string Type => "visualstudio";

    public override string DisplayName => "Visual Studio";

    protected override string ProcessName => "devenv";

    // ----------------- ENRICH -----------------

    public override void EnrichCaptured(AppConfig app)
    {
        app.Path = "";
    }

    // ----------------- LAUNCH ARGS -----------------

    protected override string BuildLaunchArguments(AppConfig app)
    {
        return string.IsNullOrWhiteSpace(app.Path)
            ? ""
            : $"\"{app.Path}\"";
    }

    // ----------------- LAUNCH (CUSTOM) -----------------

    public override void Launch(AppConfig app)
    {
        string args = BuildLaunchArguments(app);

        var before = WindowProcessHelper.GetWindowSet(ProcessName);

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

            var after = WindowProcessHelper.GetWindowList(ProcessName);

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
            Log.WriteCategory("LAUNCH", "visualstudio | fallback triggered");

            var fallback = WindowProcessHelper
                .GetWindowList(ProcessName)
                .FirstOrDefault(h =>
                    !WindowHelpers.GetWindowClassName(h).Contains("VSSplash"));

            if (fallback != IntPtr.Zero)
                handle = fallback;
        }

        if (handle != IntPtr.Zero)
        {
            WindowHelpers.DebugWindow("VS LAUNCH HANDLE", handle);

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