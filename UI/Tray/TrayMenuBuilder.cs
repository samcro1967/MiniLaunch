using System.Windows.Forms;
using MiniLaunch.Profiles;
using MiniLaunch.Core;
using MiniLaunch.UI;

namespace MiniLaunch.UI;

public class TrayMenuBuilder
{
    private readonly ProfileService _profileService;
    private readonly Action _capture;
    private readonly Action<string> _run;
    private readonly Action<string> _rename;
    private readonly Action<string> _delete;
    private readonly Action _showHelp;
    private readonly Action _showAbout;
    private readonly Action _exit;

    private static string AppDataDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MiniLaunch");

    private static string DefaultProfilePath =>
        Path.Combine(AppDataDir, "default_profile.txt");

    // 🔥 SAME FLAG PATH USED BY CONTEXT
    private static string SuppressFlagPath =>
        Path.Combine(Path.GetTempPath(), "MiniLaunch_suppress_startup.flag");

    public TrayMenuBuilder(
        ProfileService profileService,
        Action capture,
        Action<string> run,
        Action<string> rename,
        Action<string> delete,
        Action showAbout,
        Action showHelp,
        Action exit)
    {
        _profileService = profileService;
        _capture = capture;
        _run = run;
        _rename = rename;
        _delete = delete;
        _showAbout = showAbout;
        _showHelp = showHelp;
        _exit = exit;
    }

    public ContextMenuStrip Build()
    {
        var menu = new ContextMenuStrip();

        var profiles = _profileService.GetProfileNames();

        // 🔥 Load current default
        string? currentDefault = null;

        if (File.Exists(DefaultProfilePath))
        {
            currentDefault = File.ReadAllText(DefaultProfilePath);
        }

        // ---------------- PROFILES ----------------
        var profilesMenu = new ToolStripMenuItem("Profiles", AppIcons.Profile.ToBitmap());

        profilesMenu.DropDownItems.Add(
            new ToolStripMenuItem("Capture Profile", null, (_, _) => _capture())
        );

        profilesMenu.DropDownItems.Add(new ToolStripSeparator());

        // -------- RUN --------
        var runMenu = new ToolStripMenuItem("Run");

        if (profiles.Count == 0)
        {
            runMenu.DropDownItems.Add(new ToolStripMenuItem("(No profiles)") { Enabled = false });
        }
        else
        {
            foreach (var name in profiles)
            {
                var text = name;

                if (name == currentDefault)
                    text = $"⭐ {name}";

                runMenu.DropDownItems.Add(
                    new ToolStripMenuItem(text, null, (_, _) => _run(name))
                );
            }
        }

        profilesMenu.DropDownItems.Add(runMenu);

        // -------- RENAME --------
        var renameMenu = new ToolStripMenuItem("Rename");

        if (profiles.Count == 0)
        {
            renameMenu.DropDownItems.Add(new ToolStripMenuItem("(No profiles)") { Enabled = false });
        }
        else
        {
            foreach (var name in profiles)
            {
                renameMenu.DropDownItems.Add(
                    new ToolStripMenuItem(name, null, (_, _) => _rename(name))
                );
            }
        }

        profilesMenu.DropDownItems.Add(renameMenu);

        // -------- DELETE --------
        var deleteMenu = new ToolStripMenuItem("Delete");

        if (profiles.Count == 0)
        {
            deleteMenu.DropDownItems.Add(new ToolStripMenuItem("(No profiles)") { Enabled = false });
        }
        else
        {
            foreach (var name in profiles)
            {
                deleteMenu.DropDownItems.Add(
                    new ToolStripMenuItem(name, null, (_, _) => _delete(name))
                );
            }
        }

        profilesMenu.DropDownItems.Add(deleteMenu);

        // -------- SET DEFAULT --------
        var defaultMenu = new ToolStripMenuItem("Set as Default");

        if (profiles.Count == 0)
        {
            defaultMenu.DropDownItems.Add(new ToolStripMenuItem("(No profiles)") { Enabled = false });
        }
        else
        {
            foreach (var name in profiles)
            {
                var text = name;

                if (name == currentDefault)
                    text = $"⭐ {name}";

                defaultMenu.DropDownItems.Add(
                    new ToolStripMenuItem(text, null, (_, _) =>
                    {
                        Directory.CreateDirectory(AppDataDir);
                        File.WriteAllText(DefaultProfilePath, name);
                    })
                );
            }
        }

        profilesMenu.DropDownItems.Add(new ToolStripSeparator());
        profilesMenu.DropDownItems.Add(defaultMenu);

        menu.Items.Add(profilesMenu);

        // ---------------- SETTINGS ----------------
        var settingsMenu = new ToolStripMenuItem("Settings", AppIcons.Settings.ToBitmap());

        // ---------------- STARTUP ----------------
        bool isEnabled = StartupManager.IsEnabled();

        var startupMenu = new ToolStripMenuItem("Startup With Windows");

        var stateItem = new ToolStripMenuItem(
            isEnabled ? "Currently Enabled" : "Currently Disabled"
        )
        {
            Enabled = false
        };

        var actionItem = new ToolStripMenuItem(
            isEnabled ? "Disable" : "Enable"
        );

        actionItem.Click += (_, _) =>
        {
            if (isEnabled)
            {
                var result = MessageBox.Show(
                    "Disable 'Start with Windows'?",
                    "MiniLaunch",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result != DialogResult.Yes)
                    return;

                StartupManager.Disable();
            }
            else
            {
                StartupManager.Enable();
            }

            File.WriteAllText(SuppressFlagPath, "1");

            Application.Restart();
        };

        startupMenu.DropDownItems.Add(stateItem);
        startupMenu.DropDownItems.Add(new ToolStripSeparator());
        startupMenu.DropDownItems.Add(actionItem);

        settingsMenu.DropDownItems.Add(startupMenu);

        menu.Items.Add(settingsMenu);

        // ---------------- SEPARATOR ----------------
        menu.Items.Add(new ToolStripSeparator());

        // ---------------- HELP ----------------
        menu.Items.Add(
            new ToolStripMenuItem("Help", AppIcons.Help.ToBitmap(), (_, _) => _showHelp())
        );

        // ---------------- ABOUT ----------------
        menu.Items.Add(
            new ToolStripMenuItem("About", AppIcons.About.ToBitmap(), (_, _) => _showAbout())
        );

        // ---------------- EXIT ----------------
        menu.Items.Add(
            new ToolStripMenuItem("Exit", AppIcons.Exit.ToBitmap(), (_, _) => _exit())
        );

        return menu;
    }
}