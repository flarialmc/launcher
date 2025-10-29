using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Flarial.Launcher.SDK;
using Flarial.Launcher.Services.Client;
using Flarial.Launcher.Services.Management;

sealed class Play : UserControl
{
    internal Play(Form _)
    {
        Text = "Play";
        Dock = DockStyle.Fill;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Margin = default;

        TableLayoutPanel panel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = default
        };
        Controls.Add(panel);

        Button button = new()
        {
            Text = "Launch",
            Dock = DockStyle.Fill,
            Anchor = AnchorStyles.None,
            Margin = default
        };

        CheckBox checkBox1 = new()
        {
            Text = "Beta",
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = default
        };

        ProgressBar progressBar = new()
        {
            Dock = DockStyle.Fill,
            Visible = default,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 1,
            Margin = default
        };

        panel.RowStyles.Add(new() { SizeType = SizeType.Percent, Height = 100 });
        panel.RowStyles.Add(new() { SizeType = SizeType.AutoSize });
        panel.RowStyles.Add(new() { SizeType = SizeType.AutoSize });
        panel.RowStyles.Add(new() { SizeType = SizeType.AutoSize });
        panel.Controls.Add(button, 0, 0);
        panel.Controls.Add(checkBox1, 0, 1);
        panel.Controls.Add(progressBar, 0, 3);

        button.Click += async (_, _) =>
        {
            try
            {

                SuspendLayout();
                progressBar.Visible = true;
                button.Enabled = checkBox1.Visible = default;
                ResumeLayout();

                if (!await LicensingService.VerifyAsync()) throw new LicenseException(typeof(object));
                if (!checkBox1.Checked && !await _.Catalog.CompatibleAsync()) return;

                var client = checkBox1.Checked ? FlarialClient.Beta : FlarialClient.Release;

                await client.DownloadAsync((_) => Invoke(() =>
                {
                    if (progressBar.Value != _)
                    {
                        if (progressBar.Style is ProgressBarStyle.Marquee) progressBar.Style = ProgressBarStyle.Blocks;
                        progressBar.Value = _;
                    }
                }));

                SuspendLayout();
                progressBar.Value = 0;
                progressBar.Style = ProgressBarStyle.Marquee;
                ResumeLayout();

                await Task.Run(() => client.LaunchGame(true));
            }
            finally
            {
                SuspendLayout();
                progressBar.Visible = default;
                button.Enabled = checkBox1.Visible = true;
                button.Text = "Launch";
                ResumeLayout();
            }

        };
    }
}