using System;
using System.Drawing;
using System.Windows.Forms;
using Flarial.Launcher.SDK;

sealed class Settings : UserControl
{
    internal Settings(Form @this)
    {
        Text = "Settings";
        Dock = DockStyle.Fill;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Margin = default;

        TableLayoutPanel tableLayoutPanel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = default
        };
        Controls.Add(tableLayoutPanel);

        tableLayoutPanel.ColumnStyles.Add(new() { SizeType = SizeType.Percent, Width = 50 });
        tableLayoutPanel.ColumnStyles.Add(new() { SizeType = SizeType.Percent, Width = 50 });

        Label label1 = new()
        {
            Text = "Developer Mode Status",
            Dock = DockStyle.Top,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new(0, 6, 0, 0)
        };

        tableLayoutPanel.Controls.Add(label1, 0, 0);

        Button button1 = new()
        {
            Text = "Show",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = default
        };
        tableLayoutPanel.Controls.Add(button1, 1, 0);

        button1.Click += (_, _) => MessageBox.Show(@this, $"Developer mode is {(Developer.Enabled ? "enabled" : "disabled")}!", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);

        Label label2 = new()
        {
            Text = "Developer Mode License",
            Dock = DockStyle.Top,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new(0, 6, 0, 0)
        };

        tableLayoutPanel.Controls.Add(label2, 0, 1);

        Button button2 = new()
        {
            Text = "Request",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = default
        };
        tableLayoutPanel.Controls.Add(button2, 1, 1);

        button2.Click += (_, _) => Developer.Request();

        Label label3 = new()
        {
            Text = "Launcher Updater",
            Dock = DockStyle.Top,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new(0, 6, 0, 0)
        };

        tableLayoutPanel.Controls.Add(label3, 0, 2);

        Button button3 = new()
        {
            Text = "Update",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = default
        };
        tableLayoutPanel.Controls.Add(button3, 1, 2);

        button3.Click += async (_, _) =>
        {
            button3.Enabled = false;
            if (!await Launcher.UpdateAsync((_) => Invoke(() => button3.Text = $"{_}%")))
            {
                button3.Text = "Update";
                button3.Enabled = true;
            }
        };
    }
}