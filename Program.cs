using MiniLaunch; // ✅ NEW
using MiniLaunch.Profiles;
using MiniLaunch.UI;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using System.Windows.Forms;
using MiniLaunch.Core;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        const string mutexName = "MiniLaunch_SingleInstance";

        bool createdNew;

        using var mutex = new Mutex(false, mutexName, out createdNew);

        if (!createdNew)
        {
            MessageBox.Show(
                "MiniLaunch is already running.\n\nUse the tray icon to access it.",
                "MiniLaunch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        Application.ThreadException += (sender, args) =>
        {
            HandleException(args.Exception);
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                HandleException(ex);
            }
        };

        try
        {
            SetupLogging();

            ApplicationConfiguration.Initialize();

            var modules = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IAppModule).IsAssignableFrom(t) && !t.IsInterface)
                .Select(t => (IAppModule)Activator.CreateInstance(t)!)
                .ToList();

            var profileService = new ProfileService(modules);

            ProfilePaths.Ensure();

            Application.Run(new MiniLaunchContext(profileService));
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    // ---------------- SETUP LOG FILE ----------------

    private static void SetupLogging()
    {
        try
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MiniLaunch");

            Directory.CreateDirectory(folder);

            string logFile = Path.Combine(folder, "debug.log");
            string prevFile = Path.Combine(folder, "debug.prev.log");

            const long maxSize = 1_048_576; // 1 MB

            if (File.Exists(logFile))
            {
                var info = new FileInfo(logFile);

                if (info.Length > maxSize)
                {
                    if (File.Exists(prevFile))
                        File.Delete(prevFile);

                    File.Move(logFile, prevFile);
                }
            }

            // ✅ ONLY write a simple startup marker
            Log.Write("========================================");
            Log.Write($"MiniLaunch START {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Log.Write("========================================");
        }
        catch
        {
            // never crash
        }
    }

    private static void HandleException(Exception ex)
    {
        try
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MiniLaunch");

            Directory.CreateDirectory(folder);

            string file = Path.Combine(folder, "crash.log");

            string message =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\n" +
                $"Type: {ex.GetType().FullName}\n" +
                $"Message: {ex.Message}\n" +
                $"Stack:\n{ex.StackTrace}\n\n";

            File.AppendAllText(file, message);

            MessageBox.Show(
                "MiniLaunch encountered an unexpected error and needs to close.\n\n" +
                "A crash log has been saved to:\n\n" +
                file,
                "MiniLaunch Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch { }
    }
}

// ---------------- CONTEXT ----------------

public class MiniLaunchContext : ApplicationContext
{
    private readonly NotifyIcon _tray;
    private readonly ProfileService _profileService;
    private readonly FileSystemWatcher _watcher;

    private static string AppDataDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MiniLaunch");

    private static string DefaultProfilePath =>
        Path.Combine(AppDataDir, "default_profile.txt");

    private static string SuppressFlagPath =>
        Path.Combine(Path.GetTempPath(), "MiniLaunch_suppress_startup.flag");

    public MiniLaunchContext(ProfileService profileService)
    {
        _profileService = profileService;

        _tray = new NotifyIcon
        {
            Icon = AppIcons.App,
            ContextMenuStrip = BuildMenu(),
            Visible = true,
            Text = "MiniLaunch"
        };

        if (File.Exists(SuppressFlagPath))
            File.Delete(SuppressFlagPath);
        else
            ShowStartupNotification();

        _tray.DoubleClick += (_, _) => RunDefaultProfile();

        _tray.MouseUp += (_, e) =>
        {
            if (e.Button == MouseButtons.Right)
                RefreshMenu();
        };

        Microsoft.Win32.SystemEvents.SessionSwitch += OnSessionSwitch;
        Microsoft.Win32.SystemEvents.SessionEnding += OnSessionEnding;

        _watcher = new FileSystemWatcher(ProfilePaths.ProfilesDir, "*.json")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        _watcher.Created += OnProfilesChanged;
        _watcher.Deleted += OnProfilesChanged;
        _watcher.Renamed += OnProfilesChanged;
        _watcher.Changed += OnProfilesChanged;

        _watcher.EnableRaisingEvents = true;
    }

    private void RunDefaultProfile()
    {
        var profiles = _profileService.GetProfileNames();

        if (profiles.Count == 0)
            return;

        Directory.CreateDirectory(AppDataDir);

        if (File.Exists(DefaultProfilePath))
        {
            var name = File.ReadAllText(DefaultProfilePath);

            if (profiles.Contains(name))
            {
                Run(name);
                return;
            }
        }

        var newDefault = profiles[0];
        File.WriteAllText(DefaultProfilePath, newDefault);
        Run(newDefault);
    }

    private void ShowStartupNotification()
    {
        _tray.ShowBalloonTip(
            3000,
            "MiniLaunch is running",
            "Use the tray menu to capture or run profiles.",
            ToolTipIcon.Info);
    }

    private void OnSessionSwitch(object? sender, Microsoft.Win32.SessionSwitchEventArgs e)
    {
        if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            RecreateTrayIcon();
    }

    private void OnSessionEnding(object? sender, Microsoft.Win32.SessionEndingEventArgs e)
    {
        _tray.Visible = false;
    }

    private void RecreateTrayIcon()
    {
        try
        {
            _tray.Visible = false;

            var timer = new System.Windows.Forms.Timer { Interval = 50 };

            timer.Tick += (_, _) =>
            {
                timer.Stop();
                timer.Dispose();
                _tray.Icon = AppIcons.App;
                _tray.Visible = true;
            };

            timer.Start();
        }
        catch { }
    }

    private void OnProfilesChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            _tray?.GetType()
                .GetMethod("BeginInvoke", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(_tray, new object[] { new Action(RefreshMenu) });
        }
        catch
        {
            RefreshMenu();
        }
    }

    private ContextMenuStrip BuildMenu()
    {
        var builder = new TrayMenuBuilder(
            _profileService,
            Capture,
            Run,
            RenameProfile,
            DeleteProfile,
            _profileService.EditProfile,
            ShowAbout,
            ShowHelp,
            Exit
        );

        return builder.Build();
    }

    private void Capture()
    {
        using (var form = new CaptureProfileForm())
        {
            if (form.ShowDialog() != DialogResult.OK)
                return;

            var name = form.ProfileName;

            var existing = _profileService.GetProfileNames();

            if (existing.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase)))
            {
                var result = MessageBox.Show(
                    $"A profile named '{name}' already exists.\n\nDo you want to overwrite it?",
                    "Confirm Overwrite",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
            }

            // 🔥 ONLY debug needed here
            WindowHelpers.DebugForegroundWindow();

            // ✅ Use central capture pipeline
            var profile = _profileService.CaptureProfile();

            _profileService.SaveProfile(profile, name);

            Directory.CreateDirectory(AppDataDir);

            if (!File.Exists(DefaultProfilePath))
                File.WriteAllText(DefaultProfilePath, name);

            RefreshMenu();

            MessageBox.Show(
                $"Profile '{name}' captured.",
                "MiniLaunch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private void Run(string name)
    {
        try
        {
            var profile = _profileService.LoadProfile(name);
            _profileService.RunProfile(profile);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MiniLaunch Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RenameProfile(string oldName)
    {
        var newName = Prompt.Show($"Rename profile '{oldName}' to:", "Rename Profile");

        if (string.IsNullOrWhiteSpace(newName) || newName == oldName)
            return;

        var existing = _profileService.GetProfileNames();

        var match = existing.FirstOrDefault(p =>
            string.Equals(p, newName, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            var result = MessageBox.Show(
                $"A profile named '{newName}' already exists.\n\nDo you want to overwrite it?",
                "Confirm Overwrite",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            _profileService.DeleteProfile(match);
        }

        try
        {
            _profileService.RenameProfile(oldName, newName);

            if (File.Exists(DefaultProfilePath))
            {
                var current = File.ReadAllText(DefaultProfilePath);

                if (string.Equals(current, oldName, StringComparison.OrdinalIgnoreCase))
                    File.WriteAllText(DefaultProfilePath, newName);
            }

            RefreshMenu();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MiniLaunch Error");
        }
    }

    private void DeleteProfile(string name)
    {
        var result = MessageBox.Show(
            $"Delete profile '{name}'?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
            return;

        try
        {
            _profileService.DeleteProfile(name);

            if (File.Exists(DefaultProfilePath))
            {
                var current = File.ReadAllText(DefaultProfilePath);

                if (current == name)
                    File.Delete(DefaultProfilePath);
            }

            RefreshMenu();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MiniLaunch Error");
        }
    }

    private void ShowAbout()
    {
        new AboutForm().ShowDialog();
    }

    private string GetSupportedAppsText()
    {
        var apps = _profileService.GetSupportedAppNames();
        return "Supported Applications:\n- " + string.Join("\n- ", apps);
    }

    // 🔥 UPDATED HELP METHOD
    private void ShowHelp()
    {
        var supportedApps = GetSupportedAppsText();

        var text = HelpContent.Get(supportedApps);

        new HelpForm(text).ShowDialog();
    }

    private void Exit()
    {
        Microsoft.Win32.SystemEvents.SessionSwitch -= OnSessionSwitch;
        Microsoft.Win32.SystemEvents.SessionEnding -= OnSessionEnding;

        _watcher.Dispose();
        _tray.Visible = false;
        _tray.Dispose();

        Application.Exit();
    }

    private void RefreshMenu()
    {
        _tray.ContextMenuStrip = BuildMenu();
    }
}