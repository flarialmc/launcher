using System.Drawing;
using System.Windows.Forms;
using Bedrockix.Minecraft;
using Flarial.Launcher.SDK;

sealed class Form : System.Windows.Forms.Form
{
    internal Catalog Catalog = default;

    internal Form()
    {
        Application.ThreadException += (_, e) =>
        {
            MessageBox.Show(this, e.Exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.ExitThread();
        };

        Text = "Flarial Launcher";
        Font = SystemFonts.MessageBoxFont;
        ClientSize = LogicalToDeviceUnits(new Size(400, 300));
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        ProgressBar progressBar = new()
        {
            Dock = DockStyle.Bottom,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 1
        };
        Controls.Add(progressBar);

        Load += async (_, _) =>
        {
            Catalog = await Catalog.GetAsync();

            SuspendLayout();
            progressBar.Visible = false;
            Controls.Add(new Pages(new Play(this), new Versions(this), new Settings(this)) { Enabled = Game.Installed && !Minecraft.GDK });
            ResumeLayout();
        };
    }
}