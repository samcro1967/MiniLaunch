using MiniLaunch.Core;

public abstract class BaseWindowEnumAppModule : BaseAppModule
{
    protected abstract bool TryFindWindow(out IntPtr handle);

    protected abstract void LaunchInternal(AppConfig app);

    // ----------------- CAPTURE -----------------

    public override bool TryCapture(out AppConfig? app)
    {
        app = null;

        if (!TryFindWindow(out var handle))
        {
            Log.WriteCategory("CAPTURE", $"{Type} | no valid window found");
            return false;
        }

        return WindowCaptureHelper.TryCaptureWindow(Type, handle, out app);
    }

    // ----------------- LAUNCH -----------------

    public override void Launch(AppConfig app)
    {
        LaunchInternal(app);
    }
}