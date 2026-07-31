namespace MiniLaunch.UI
{
    partial class AboutForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pictureBoxIcon = new PictureBox();
            lblProduct = new Label();
            lblVersion = new Label();
            lblCopyright = new Label();
            btnOk = new Button();
            txtDescription = new TextBox();
            lnkLicense = new LinkLabel();
            lnkGitHub = new LinkLabel();
            lnkSettingsFolder = new LinkLabel();
            ((System.ComponentModel.ISupportInitialize)pictureBoxIcon).BeginInit();
            SuspendLayout();

            // pictureBoxIcon
            pictureBoxIcon.Location = new Point(20, 21);
            pictureBoxIcon.Name = "pictureBoxIcon";
            pictureBoxIcon.Size = new Size(48, 51);
            pictureBoxIcon.SizeMode = PictureBoxSizeMode.CenterImage;

            // lblProduct
            lblProduct.AutoSize = true;
            lblProduct.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold);
            lblProduct.Location = new Point(98, 21);
            lblProduct.Name = "lblProduct";
            lblProduct.Text = "Product";

            // lblVersion
            lblVersion.AutoSize = true;
            lblVersion.Location = new Point(98, 62);
            lblVersion.Name = "lblVersion";
            lblVersion.Text = "Version text";

            // lblCopyright
            lblCopyright.AutoSize = true;
            lblCopyright.Location = new Point(98, 101);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Text = "Copyright";

            // txtDescription
            txtDescription.BorderStyle = BorderStyle.None;
            txtDescription.Location = new Point(98, 142);
            txtDescription.Multiline = true;
            txtDescription.ReadOnly = true;
            txtDescription.Size = new Size(300, 149);

            // lnkGitHub
            lnkGitHub.AutoSize = true;
            lnkGitHub.Location = new Point(98, 294);
            lnkGitHub.Text = "Project on GitHub";
            lnkGitHub.LinkClicked += lnkGitHub_LinkClicked;

            // lnkSettingsFolder
            lnkSettingsFolder.AutoSize = true;
            lnkSettingsFolder.Location = new Point(98, 329);
            lnkSettingsFolder.Text = "Open App Folder";
            lnkSettingsFolder.LinkClicked += lnkSettingsFolder_LinkClicked;

            // lnkLicense (optional – keep or remove)
            lnkLicense.AutoSize = true;
            lnkLicense.Location = new Point(98, 367);
            lnkLicense.Text = "View MIT License";
            lnkLicense.LinkClicked += lnkLicense_LinkClicked;

            // btnOk
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(98, 405);
            btnOk.Text = "OK";
            btnOk.Click += btnOk_Click;

            // AboutForm
            AcceptButton = btnOk;
            ClientSize = new Size(462, 453);
            Controls.Add(pictureBoxIcon);
            Controls.Add(lblProduct);
            Controls.Add(lblVersion);
            Controls.Add(lblCopyright);
            Controls.Add(txtDescription);
            Controls.Add(lnkGitHub);
            Controls.Add(lnkSettingsFolder);
            Controls.Add(lnkLicense);
            Controls.Add(btnOk);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About MiniLaunch";

            ((System.ComponentModel.ISupportInitialize)pictureBoxIcon).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private PictureBox pictureBoxIcon;
        private Label lblProduct;
        private Label lblVersion;
        private Label lblCopyright;
        private Button btnOk;
        private TextBox txtDescription;
        private LinkLabel lnkLicense;
        private LinkLabel lnkGitHub;
        private LinkLabel lnkSettingsFolder;
    }
}
