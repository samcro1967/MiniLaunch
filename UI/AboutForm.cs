/*
 * File: AboutForm.cs
 * Path: /UI/AboutForm.cs
 *
 * Purpose:
 * Display application information for MiniLaunch.
 */

using System.Diagnostics;
using System.Reflection;
using MiniLaunch.Profiles;

namespace MiniLaunch.UI;

public partial class AboutForm : Form
{
    public AboutForm()
    {
        InitializeComponent();

        // ✅ App icon
        Icon = AppIcons.App;

        if (pictureBoxIcon != null)
        {
            pictureBoxIcon.Image = AppIcons.App.ToBitmap();
        }

        Text = "About MiniLaunch";

        // ✅ Product + Version
        lblProduct.Text = Application.ProductName;

        Version? version =
            Assembly.GetExecutingAssembly().GetName().Version;

        lblVersion.Text = $"Version {version}";

        // ✅ Copyright
        lblCopyright.Text = "© 2026 samcro1967";

        // ✅ Description
        txtDescription.Text =
            "Launch and organize application workspaces from the system tray." +
            "\r\n\r\n" +
            "Capture your current layout and restore it instantly." +
            "\r\n\r\n" +
            "Designed for multi-monitor productivity.";

        // Prevent text from being highlighted
        txtDescription.TabStop = false;
        this.ActiveControl = btnOk;
    }

    // ---------------- LINKS ----------------

    private void lnkGitHub_LinkClicked(
        object sender,
        LinkLabelLinkClickedEventArgs e)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "https://github.com/samcro1967/MiniLaunch",
                UseShellExecute = true
            });
    }

    private void lnkSettingsFolder_LinkClicked(
        object sender,
        LinkLabelLinkClickedEventArgs e)
    {
        var appPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MiniLaunch"
        );

        Directory.CreateDirectory(appPath); // ensure it exists

        Process.Start(
            new ProcessStartInfo
            {
                FileName = appPath,
                UseShellExecute = true
            });
    }

    // OPTIONAL — keep only if Designer still has lnkLicense
    private void lnkLicense_LinkClicked(
        object sender,
        LinkLabelLinkClickedEventArgs e)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "https://github.com/samcro1967/MiniLaunch/blob/main/LICENSE",
                UseShellExecute = true
            });
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        Close();
    }
}
