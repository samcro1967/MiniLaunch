using System.Linq;

public class NotepadApp : BaseProcessAppModule
{
    public override string Type => "notepad";

    public override string DisplayName => "Notepad++";

    protected override string ProcessName => "notepad++";

    // ----------------- ENRICH -----------------

    public override void EnrichCaptured(AppConfig app)
    {
        app.Session = "";
    }

    // ----------------- LAUNCH ARGS -----------------

    protected override string BuildLaunchArguments(AppConfig app)
    {
        return string.IsNullOrWhiteSpace(app.Session)
            ? ""
            : $"-openSession \"{app.Session}\"";
    }

    // ----------------- EXECUTABLE -----------------

    protected override string GetExecutable()
    {
        var paths = new[]
        {
            @"C:\Program Files\Notepad++\notepad++.exe",
            @"C:\Program Files (x86)\Notepad++\notepad++.exe"
        };

        return paths.FirstOrDefault(File.Exists) ?? "notepad++.exe";
    }
}