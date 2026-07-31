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
    private readonly Action<string> _edit;
    private readonly Action _showHelp;
    private readonly Action _showAbout;
    private readonly Action _exit;

    private static string AppDataDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MiniLaunch");

    private static string DefaultProfilePath =>
        Path.Combine(AppDataDir, "default_profile.txt");

    private static string SuppressFlagPath =>
        Path.Combine(Path.GetTempPath(), "MiniLaunch_suppress_startup.flag");

    public TrayMenuBuilder(
        ProfileService profileService,
        Action capture,
        Action<string> run,
        Action<string> rename,
        Action<string> delete,
        Action<string> edit,
        Action showAbout,
        Action showHelp,
        Action exit)
    {
        _profileService = profileService;
        _capture = capture;
        _run = run;
        _rename = rename;
        _delete = delete;
        _edit = edit;
        _showAbout = showAbout;
        _showHelp = showHelp;
        _exit = exit;
    }

    public ContextMenuStrip Build()
    {
        var menu = new ContextMenuStrip();

        var profiles = _profileService.GetProfileNames();

        string? currentDefault = null;

        if (File.Exists(DefaultProfilePath))
        {
            currentDefault = File.ReadAllText(DefaultProfilePath);
        }

        // ---------------- PROFILES ----------------
        var profilesMenu = new ToolStripMenuItem("Profiles", AppIcons.Profile.ToBitmap());

        // 🔥 Capture Profile (WITH ICON)
        profilesMenu.DropDownItems.Add(
            new ToolStripMenuItem("Capture Profile", AppIcons.Capture.ToBitmap(), (_, _) => _capture())
        );

        profilesMenu.DropDownItems.Add(new ToolStripSeparator());

        // -------- RUN --------
        var runMenu = new ToolStripMenuItem("Run", AppIcons.Run.ToBitmap());

        if (profiles.Count == 0)
        {
            runMenu.DropDownItems.Add(new ToolStripMenuItem("(No profiles)") { Enabled = false });
        }
        else
        {
            foreach (var name in profiles)
            {
                bool isDefault = name == currentDefault;

                var item = new ToolStripMenuItem(
                    name,
                    isDefault ? AppIcons.Default.ToBitmap() : AppIcons.Run.ToBitmap(),
                    (_, _) => _run(name)
                );

                runMenu.DropDownItems.Add(item);
            }
        }

        profilesMenu.DropDownItems.Add(runMenu);

        // -------- EDIT --------
        var editMenu = new ToolStripMenuItem("Edit", AppIcons.Edit.ToBitmap());

        if (profiles.Count == 0)
        {
            editMenu.DropDownItems.Add(new ToolStripMenuItem("(No profiles)") { Enabled = false });
        }
        else
        {
            foreach (var name in profiles)
            {
                editMenu.DropDownItems.Add(
                    new ToolStripMenuItem(name, AppIcons.Edit.ToBitmap(), (_, _) => _edit(name))
                );
            }
        }

        profilesMenu.DropDownItems.Add(editMenu);

        // -------- RENAME --------
        var renameMenu = new ToolStripMenuItem("Rename", AppIcons.Rename.ToBitmap());

        if (profiles.Count == 0)
        {
            renameMenu.DropDownItems.Add(new ToolStripMenuItem("(No profiles)") { Enabled = false });
        }
        else
        {
            foreach (var name in profiles)
            {
                renameMenu.DropDownItems.Add(
                    new ToolStripMenuItem(name, AppIcons.Rename.ToBitmap(), (_, _) => _rename(name))
                );
            }
        }

        profilesMenu.DropDownItems.Add(renameMenu);

        // -------- DELETE --------
        var deleteMenu = new ToolStripMenuItem("Delete", AppIcons.Delete.ToBitmap());

        if (profiles.Count == 0)
        {
            deleteMenu.DropDownItems.Add(new ToolStripMenuItem("(No profiles)") { Enabled = false });
        }
        else
        {
            foreach (var name in profiles)
            {
                deleteMenu.DropDownItems.Add(
                    new ToolStripMenuItem(name, AppIcons.Delete.ToBitmap(), (_, _) => _delete(name))
                );
            }
        }

        profilesMenu.DropDownItems.Add(deleteMenu);

        // -------- SET DEFAULT --------
        var defaultMenu = new ToolStripMenuItem("Set as Default", AppIcons.Default.ToBitmap());

        if (profiles.Count == 0)
        {
            defaultMenu.DropDownItems.Add(new ToolStripMenuItem("(No profiles)") { Enabled = false });
        }
        else
        {
            foreach (var name in profiles)
            {
                bool isDefault = name == currentDefault;

                defaultMenu.DropDownItems.Add(
                    new ToolStripMenuItem(
                        name,
                        isDefault ? AppIcons.Default.ToBitmap() : null,
                        (_, _) =>
                        {
                            Directory.CreateDirectory(AppDataDir);
                            File.WriteAllText(DefaultProfilePath, name);
                        }
                    )
                );
            }
        }

        profilesMenu.DropDownItems.Add(new ToolStripSeparator());
        profilesMenu.DropDownItems.Add(defaultMenu);

        menu.Items.Add(profilesMenu);

        // ---------------- SETTINGS ----------------
        var settingsMenu = new ToolStripMenuItem("Settings", AppIcons.Settings.ToBitmap());

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

        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add(
            new ToolStripMenuItem("Help", AppIcons.Help.ToBitmap(), (_, _) => _showHelp())
        );

        menu.Items.Add(
            new ToolStripMenuItem("About", AppIcons.About.ToBitmap(), (_, _) => _showAbout())
        );

        menu.Items.Add(
            new ToolStripMenuItem("Exit", AppIcons.Exit.ToBitmap(), (_, _) => _exit())
        );

        return menu;
    }
}