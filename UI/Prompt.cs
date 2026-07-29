using System.Windows.Forms;

namespace MiniLaunch.UI
{
    public static class Prompt
    {
        public static string? Show(string text, string caption)
        {
            var form = new Form()
            {
                Width = 400,
                Height = 150,
                Text = caption,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var label = new Label()
            {
                Left = 10,
                Top = 10,
                Width = 360,
                Text = text
            };

            var textBox = new TextBox()
            {
                Left = 10,
                Top = 35,
                Width = 360
            };

            var button = new Button()
            {
                Text = "OK",
                Left = 290,
                Width = 80,
                Top = 65
            };

            button.Click += (_, _) => form.DialogResult = DialogResult.OK;

            form.Controls.Add(label);
            form.Controls.Add(textBox);
            form.Controls.Add(button);

            form.AcceptButton = button;

            return form.ShowDialog() == DialogResult.OK
                ? textBox.Text
                : null;
        }
    }
}
