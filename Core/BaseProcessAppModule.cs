using System.Diagnostics;
using MiniLaunch.Core;

public abstract class BaseProcessAppModule : BaseAppModule
{
    protected abstract string ProcessName { get; }

    protected virtual string BuildLaunchArguments(AppConfig app) => "";

    protected virtual string GetExecutable() => ProcessName + ".exe";

    // ----------------- CAPTURE -----------------

    public override bool TryCapture(out AppConfig? app)
    {
        app = null;

        if (!WindowProcessHelper.TryGetMainWindow(ProcessName, out var handle))
            return false;

        return WindowCaptureHelper.TryCaptureWindow(Type, handle, out app);
    }

    // ----------------- LAUNCH -----------------

    public override void Launch(AppConfig app)
    {
        string args = BuildLaunchArguments(app);

        WindowLaunchHelper.LaunchAndPosition(
            type: Type,

            getExistingWindows: () => WindowProcessHelper.GetWindowSet(ProcessName),
            getCurrentWindows: () => WindowProcessHelper.GetWindowList(ProcessName),

            startProcess: () =>
                Process.Start(new ProcessStartInfo
                {
                    FileName = GetExecutable(),
                    Arguments = args,
                    UseShellExecute = true
                }),

            app: app
        );
    }
}