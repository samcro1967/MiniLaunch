using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniLaunch.UI
{
    public partial class CaptureProfileForm : Form
    {
        private Label lblPrompt = null!;
        private TextBox txtProfileName = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;

        public string ProfileName => txtProfileName.Text.Trim();

        public CaptureProfileForm()
        {
            InitializeComponent();

            // ✅ App icon
            Icon = AppIcons.App;

            Text = "Capture Profile";

            // ✅ Behavior
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            // ✅ Focus textbox
            Load += (s, e) => txtProfileName.Focus();
        }

        private void InitializeComponent()
        {
            lblPrompt = new Label();
            txtProfileName = new TextBox();
            btnOk = new Button();
            btnCancel = new Button();

            SuspendLayout();

            // lblPrompt
            lblPrompt.AutoSize = true;
            lblPrompt.Location = new Point(12, 15);
            lblPrompt.Name = "lblPrompt";
            lblPrompt.Size = new Size(115, 15);
            lblPrompt.Text = "Enter profile name:";

            // txtProfileName
            txtProfileName.Location = new Point(12, 35);
            txtProfileName.Name = "txtProfileName";
            txtProfileName.Size = new Size(360, 23);

            // btnOk
            btnOk.Location = new Point(216, 70);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 23);
            btnOk.Text = "OK";
            btnOk.Click += btnOk_Click;

            // btnCancel
            btnCancel.Location = new Point(297, 70);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.Text = "Cancel";
            btnCancel.DialogResult = DialogResult.Cancel;

            // Form
            AcceptButton = btnOk;
            CancelButton = btnCancel;
            ClientSize = new Size(384, 111);
            Controls.Add(lblPrompt);
            Controls.Add(txtProfileName);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CaptureProfileForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Capture Profile";

            ResumeLayout(false);
            PerformLayout();
        }

        private void btnOk_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProfileName.Text))
            {
                MessageBox.Show(
                    "Please enter a profile name.",
                    "MiniLaunch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtProfileName.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}