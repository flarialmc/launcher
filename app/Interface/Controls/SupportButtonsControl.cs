using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static System.Environment;
using static Flarial.Launcher.Interface.MessageDialogContent;
using static Windows.Management.Core.ApplicationDataManager;
using static Flarial.Launcher.Services.Core.Minecraft;
using ModernWpf.Controls;
using System.Windows.Interop;
using System;
using static Flarial.Launcher.PInvoke;

namespace Flarial.Launcher.Interface.Controls;

sealed class SupportButtonsControl : UniformGrid
{
    [Obsolete("", true)]
    readonly Button _discordLinkButton = new()
    {
        Content = new SupportButtonContent(Symbol.Globe, "Discord"),
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new(0, 0, 6, 0)
    };

    readonly Button _clientFolderButton = new()
    {
        Content = new SupportButtonContent(Symbol.MoveToFolder, "Client"),
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new(6, 0, 6, 0)
    };

    readonly Button _launcherFolderButton = new()
    {
        Content = new SupportButtonContent(Symbol.MoveToFolder, "Launcher"),
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new(6, 0, 0, 0)
    };

    sealed class SupportButtonContent : SimpleStackPanel
    {
        internal SupportButtonContent(Symbol symbol, string text)
        {
            Spacing = 12;
            Orientation = Orientation.Horizontal;

            Children.Add(new SymbolIcon(symbol));
            Children.Add(new TextBlock { Text = text });
        }
    }

    readonly string _launcher = CurrentDirectory, _gdk = Path.Combine(CurrentDirectory, @"..\Client");

    internal SupportButtonsControl(WindowInteropHelper helper)
    {
        Rows = 1;

        Children.Add(_clientFolderButton);
        Children.Add(_launcherFolderButton);

        _launcherFolderButton.Click += (_, _) => ShellExecute(helper.EnsureHandle(), lpFile: _launcher, nShowCmd: SW_NORMAL);

        _clientFolderButton.Click += async (_, _) =>
        {
            if (!IsInstalled)
            {
                await MessageDialog.ShowAsync(_notInstalled);
                return;
            }

            var path = UsingGameDevelopmentKit ? _gdk : Path.Combine(CreateForPackageFamily(PackageFamilyName).RoamingFolder.Path, "Flarial");

            if (!Directory.Exists(path))
            {
                await MessageDialog.ShowAsync(_folderNotFound);
                return;
            }

            ShellExecute(helper.EnsureHandle(), lpFile: path, nShowCmd: SW_NORMAL);
        };
    }
}