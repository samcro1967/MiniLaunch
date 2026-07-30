using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniLaunch.UI
{
    public partial class HelpForm : Form
    {
        private TextBox txtHelp = null!;
        private Button btnClose = null!;

        public HelpForm(string content)
        {
            InitializeComponent();

            Icon = AppIcons.App;
            Text = "MiniLaunch Help";

            txtHelp.Text = content;

            txtHelp.SelectionStart = 0;
            txtHelp.SelectionLength = 0;

            AcceptButton = btnClose;
        }

        private void InitializeComponent()
        {
            txtHelp = new TextBox();
            btnClose = new Button();

            SuspendLayout();

            // txtHelp
            txtHelp.Multiline = true;
            txtHelp.ReadOnly = true;
            txtHelp.ScrollBars = ScrollBars.Vertical;
            txtHelp.Location = new Point(12, 12);
            txtHelp.Size = new Size(560, 400);
            txtHelp.BorderStyle = BorderStyle.None;
            txtHelp.Font = new Font("Segoe UI", 10.5F);

            // btnClose
            btnClose.Text = "Close";
            btnClose.Location = new Point(497, 420);
            btnClose.Size = new Size(75, 23);
            btnClose.Click += (s, e) => Close();

            // Form
            ClientSize = new Size(584, 461);
            Controls.Add(txtHelp);
            Controls.Add(btnClose);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            ResumeLayout(false);
        }
    }
}