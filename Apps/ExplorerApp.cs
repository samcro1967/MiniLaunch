using System.Diagnostics;
using System.Windows.Forms;
using MiniLaunch.Core;

public class ExplorerApp : BaseWindowEnumAppModule
{
    public override string Type => "explorer";

    public override string DisplayName => "Windows Explorer";

    // ----------------- FIND WINDOW -----------------

    protected override bool TryFindWindow(out IntPtr selected)
    {
        selected = IntPtr.Zero;

        IntPtr found = IntPtr.Zero; // ✅ local variable (NOT out param)

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

            found = hWnd; // ✅ SAFE
            return false;

        }, IntPtr.Zero);

        if (found == IntPtr.Zero)
            return false;

        selected = found; // ✅ assign OUTSIDE lambda

        WindowHelpers.DebugWindow("EXPLORER SELECTED", selected);

        return true;
    }

    // ----------------- ENRICH -----------------

    public override void EnrichCaptured(AppConfig app)
    {
        app.Path = "";
    }

    // ----------------- LAUNCH -----------------

    protected override void LaunchInternal(AppConfig app)
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

            doubleMove: true // 🔥 Explorer override protection
        );
    }
}