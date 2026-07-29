public interface IAppModule
{
    string Type { get; }

    string DisplayName { get; }   // 🔥 NEW

    bool TryCapture(out AppConfig? app);

    void EnrichCaptured(AppConfig app);

    void Launch(AppConfig app);
}