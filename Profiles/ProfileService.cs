using MiniLaunch.Profiles;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using MiniLaunch.Core;

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
                    module.EnrichCaptured(app);

                    profile.Apps.Add(app);

                }
            }
            catch
            {
                // Never let one app break capture
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
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        File.WriteAllText(path, json);

        Log.WriteCategory("PROFILE", $"saved | {path}");
    }

    // ----------------- LOAD -----------------

    public Profile LoadProfile(string name)
    {
        var path = GetProfilePath(name);

        if (!File.Exists(path))
        {
            Log.WriteCategory("WARN", $"profile not found | {path}");
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
            // ✅ CONFIG logging (structured + consistent)
            Log.WriteCategory("CONFIG",
                $"{app.Type} | MON={app.Monitor} | REL_POS=({app.X},{app.Y}) | SIZE={app.Width}x{app.Height}");

            var module = _modules.FirstOrDefault(m => m.Type == app.Type);

            if (module != null)
            {
                module.Launch(app);
            }
            else
            {
                Log.WriteCategory("ERROR", $"{app.Type} | no module found");
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

    // ------------------ EDIT ------------------

    public void EditProfile(string name)
    {
        var path = GetProfilePath(name);

        if (!File.Exists(path))
        {
            Log.WriteCategory("ERROR", $"edit failed | not found | {path}");
            return;
        }

        Log.WriteCategory("PROFILE", $"open editor | {path}");

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true // 🔥 required to open with default editor
        });
    }

    // ----------------- PATH HELPER -----------------

    private string GetProfilePath(string name)
    {
        return Path.Combine(ProfilePaths.ProfilesDir, $"{name}.json");
    }
}