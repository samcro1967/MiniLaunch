using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using MiniLaunch.Core;

public class ExplorerApp : IAppModule
{
    public string Type => "explorer";

    public string DisplayName => "Windows Explorer";

    // ----------------- CAPTURE -----------------

    public bool TryCapture(out AppConfig? app)
    {
        app = null;

        IntPtr selected = IntPtr.Zero;

        WindowHelpers.EnumWindows((hWnd, lParam) =>
        {
            if (!WindowHelpers.IsWindowVisible(hWnd))
                return true;

            if (!WindowHelpers.TryGetWindowRect(hWnd, out var r))
                return true;

            int width = r.Right - r.Left;
            int height = r.Bottom - r.Top;

            if (width < 300 || height < 300)
                return true;

            WindowHelpers.GetWindowThreadProcessId(hWnd, out uint pid);

            try
            {
                var proc = Process.GetProcessById((int)pid);

                if (!proc.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            catch
            {
                return true;
            }

            var className = WindowHelpers.GetWindowClassName(hWnd);

            if (!className.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
                return true;

            WindowHelpers.DebugWindow("EXPLORER CANDIDATE", hWnd);

            selected = hWnd;
            return false;

        }, IntPtr.Zero);

        if (selected == IntPtr.Zero)
        {
            Log.WriteCategory("CAPTURE", "explorer | no valid window found");
            return false;
        }

        WindowHelpers.DebugWindow("EXPLORER SELECTED", selected);

        // 🔥 Delegate to shared helper
        return WindowCaptureHelper.TryCaptureWindow(
            type: Type,
            handle: selected,
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
        WindowLaunchHelper.LaunchAndPosition(
            type: Type,

            getExistingWindows: () =>
            {
                var set = new HashSet<IntPtr>();

                WindowHelpers.EnumWindows((hWnd, lParam) =>
                {
                    if (!WindowHelpers.IsWindowVisible(hWnd))
                        return true;

                    var className = WindowHelpers.GetWindowClassName(hWnd);

                    if (className.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
                        set.Add(hWnd);

                    return true;

                }, IntPtr.Zero);

                return set;
            },

            getCurrentWindows: () =>
            {
                var list = new List<IntPtr>();

                WindowHelpers.EnumWindows((hWnd, lParam) =>
                {
                    if (!WindowHelpers.IsWindowVisible(hWnd))
                        return true;

                    var className = WindowHelpers.GetWindowClassName(hWnd);

                    if (className.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
                        list.Add(hWnd);

                    return true;

                }, IntPtr.Zero);

                return list;
            },

            startProcess: () =>
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = string.IsNullOrWhiteSpace(app.Path)
                        ? "/n"
                        : $"/n, \"{app.Path}\"",
                    UseShellExecute = true
                }),

            app: app,

            doubleMove: true // 🔥 Explorer needs this
        );
    }
}