public abstract class BaseAppModule : IAppModule
{
    public abstract string Type { get; }
    public abstract string DisplayName { get; }

    public abstract bool TryCapture(out AppConfig? app);

    public virtual void EnrichCaptured(AppConfig app)
    {
        // optional
    }

    public abstract void Launch(AppConfig app);
}