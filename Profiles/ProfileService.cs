using System.Diagnostics;
using System.Text.Json;
using System.Windows.Forms;
using MiniLaunch.Profiles;

public class ProfileService
{
    private readonly List<IAppModule> _modules;

    public ProfileService(List<IAppModule> modules)
    {
        _modules = modules;
    }

    // ----------------- GET PROFILE NAMES -----------------

    public List<string> GetProfileNames()
    {
        ProfilePaths.Ensure();

        return Directory.GetFiles(ProfilePaths.ProfilesDir, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(n => n)
            .ToList();
    }

    public List<string> GetSupportedAppNames()
    {
        return _modules
            .Select(m => m.DisplayName)
            .OrderBy(n => n)
            .ToList();
    }

    // ----------------- CAPTURE -----------------

    public Profile CaptureProfile()
    {
        var profile = new Profile();

        foreach (var module in _modules)
        {
            try
            {
                if (module.TryCapture(out var app) && app != null)
                {
                    // 🔥 Enrich app-specific settings
                    module.EnrichCaptured(app);

                    // 🔥 Get real window position (best effort)
                    var handle = WindowHelpers.FindWindowByProcessName(app.Type);

                    if (handle != IntPtr.Zero &&
                        WindowHelpers.TryGetWindowRect(handle, out var rect))
                    {
                        app.X = rect.Left;
                        app.Y = rect.Top;
                        app.Width = rect.Right - rect.Left;
                        app.Height = rect.Bottom - rect.Top;

                        var center = new System.Drawing.Point(
                            (rect.Left + rect.Right) / 2,
                            (rect.Top + rect.Bottom) / 2
                        );

                        var screen = Screen.FromPoint(center);
                        app.Monitor = Array.IndexOf(Screen.AllScreens, screen);
                    }

                    profile.Apps.Add(app);

                    Console.WriteLine($"Captured: {app.Type}");
                }
            }
            catch
            {
                // never let one app break capture
            }
        }

        return profile;
    }

    // ----------------- SAVE -----------------

    public void SaveProfile(Profile profile, string name)
    {
        ProfilePaths.Ensure();

        var path = GetProfilePath(name);

        var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);

        Console.WriteLine($"Profile saved: {path}");
    }

    // ----------------- LOAD -----------------

    public Profile LoadProfile(string name)
    {
        var path = GetProfilePath(name);

        if (!File.Exists(path))
        {
            Console.WriteLine($"Profile not found: {path}");
            return new Profile();
        }

        var json = File.ReadAllText(path);

        var profile = JsonSerializer.Deserialize<Profile>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return profile ?? new Profile();
    }

    // ----------------- RUN -----------------

    public void RunProfile(Profile profile)
    {
        foreach (var app in profile.Apps)
        {
            Console.WriteLine($"Launching {app.Type}");

            var module = _modules.FirstOrDefault(m => m.Type == app.Type);

            if (module != null)
            {
                module.Launch(app);
            }
            else
            {
                Console.WriteLine($"No module for {app.Type}");
            }

            if (app.Delay > 0)
                Thread.Sleep(app.Delay);
        }
    }

    // ----------------- RENAME -----------------

    public void RenameProfile(string oldName, string newName)
    {
        ProfilePaths.Ensure();

        var oldPath = GetProfilePath(oldName);
        var newPath = GetProfilePath(newName);

        if (!File.Exists(oldPath))
            throw new Exception("Profile not found.");

        if (File.Exists(newPath))
            throw new Exception("A profile with that name already exists.");

        File.Move(oldPath, newPath);
    }

    // ----------------- DELETE -----------------

    public void DeleteProfile(string name)
    {
        ProfilePaths.Ensure();

        var path = GetProfilePath(name);

        if (!File.Exists(path))
            throw new Exception("Profile not found.");

        File.Delete(path);
    }

    // ----------------- PATH HELPER -----------------

    private string GetProfilePath(string name)
    {
        return Path.Combine(ProfilePaths.ProfilesDir, $"{name}.json");
    }
}