using System.Linq;

public class ChromeApp : BaseProcessAppModule
{
    public override string Type => "chrome";

    public override string DisplayName => "Google Chrome / Microsoft Edge";

    protected override string ProcessName => "chrome";

    protected override string BuildLaunchArguments(AppConfig app)
    {
        string args = "--new-window";

        if (app.Urls?.Any() == true)
            args += " " + string.Join(" ", app.Urls);

        return args;
    }

    public override void EnrichCaptured(AppConfig app)
    {
        app.Urls = new List<string>();
    }
}