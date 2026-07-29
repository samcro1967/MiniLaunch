public class AppConfig
{
    public string Type { get; set; } = "";

    public int Monitor { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public bool Maximized { get; set; } = false; // 🔥 ADD THIS

    public List<string>? Urls { get; set; }
    public List<string>? Tabs { get; set; }
    public List<string>? Files { get; set; }
    public string? Session { get; set; }
    public string? Path { get; set; }

    public int Delay { get; set; } = 0;
}

public class Profile
{
    public List<AppConfig> Apps { get; set; } = new();
}
