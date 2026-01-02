using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static System.Environment;
using static Flarial.Launcher.Interface.MessageDialogContent;
using static Windows.Management.Core.ApplicationDataManager;
using static Flarial.Launcher.Services.Core.Minecraft;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface.Controls;

sealed class SupportButtonsControl : UniformGrid
{
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

    readonly string _gdk = Path.Combine(CurrentDirectory, @"..\Client");

    internal SupportButtonsControl()
    {
        Rows = 1;

        Children.Add(_discordLinkButton);
        Children.Add(_clientFolderButton);
        Children.Add(_launcherFolderButton);

        _discordLinkButton.Click += (_, _) => { using (Process.Start("https://flarial.xyz/discord")) { } };

        _launcherFolderButton.Click += (_, _) => { using (Process.Start(CurrentDirectory)) { } };

        _clientFolderButton.Click += async (_, _) =>
        {
            if (!IsInstalled) { await MessageDialog.ShowAsync(_notInstalled); return; }
            var path = UsingGameDevelopmentKit ? _gdk : Path.Combine(CreateForPackageFamily(PackageFamilyName).RoamingFolder.Path, "Flarial");

            if (!Directory.Exists(path)) { await MessageDialog.ShowAsync(_folderNotFound); return; }
            using (Process.Start(path)) { }
        };
    }

    [Obsolete("", true)]
    void OnLauncherFolderButtonClick(object sender, RoutedEventArgs args)
    {
        using (Process.Start(CurrentDirectory)) { }
    }

    async void OnClientFolderButtonClick(object sender, RoutedEventArgs args)
    {
        if (!IsInstalled)
        {
            await MessageDialog.ShowAsync(_notInstalled);
            return;
        }

        var gdk = Path.Combine(CurrentDirectory, @"..\Client");
        var uwp = Path.Combine(CreateForPackageFamily(PackageFamilyName).RoamingFolder.Path, "Flarial");

        var path = UsingGameDevelopmentKit ? gdk : uwp;

        if (!Directory.Exists(path))
        {
            await MessageDialog.ShowAsync(_folderNotFound);
            return;
        }

        using (Process.Start(path)) { }
    }
}