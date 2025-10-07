using System.Collections.Generic;
using System.Windows.Forms;

sealed class Pages : TableLayoutPanel
{
    internal Pages(params UserControl[] value)
    {
        Dock = DockStyle.Fill;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        RowStyles.Add(new() { SizeType = SizeType.AutoSize });
        RowStyles.Add(new() { SizeType = SizeType.Percent, Height = 100 });

        TableLayoutPanel panel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = default
        };
        Controls.Add(panel, 0, 0);

        List<Button> buttons = [];

        var width = 100 / value.Length;

        for (int index = default; index < value.Length; index++)
        {
            panel.ColumnStyles.Add(new() { SizeType = SizeType.Percent, Width = width });

            var control = value[index];
            control.Visible = default;

            Button button = new()
            {
                Text = control.Text,
                Dock = DockStyle.Fill,
                Margin = default
            };

            button.Click += (sender, _) =>
            {
                SuspendLayout();
                for (int index = default; index < value.Length; index++)
                {
                    var button = buttons[index];
                    var control = value[index];
                    button.Enabled = sender != button;
                    control.Visible = sender == button;
                }
                ResumeLayout();
            };


            Controls.Add(control, default, 1);
            panel.Controls.Add(button, index, default);
            buttons.Add(button);
        }

        var item = buttons[default];
        value[default].Visible = !(item.Enabled = default);
    }
}