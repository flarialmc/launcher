using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Flarial.Launcher.Services.Core;
using static System.Environment;
using static Flarial.Launcher.Interface.MessageDialogContent;
using static Windows.Management.Core.ApplicationDataManager;
using static Flarial.Launcher.Services.Core.Minecraft;
using ModernWpf;
using ModernWpf.Controls;
using Windows.Management.Core;

namespace Flarial.Launcher.Interface.Controls;

sealed class FolderButtonsControl : UniformGrid
{
    readonly Button _clientFolderButton = new()
    {
        Content = new FolderButtonContent("Open Client Folder"),
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new(0, 0, 6, 0)
    };

    readonly Button _launcherFolderButton = new()
    {
        Content = new FolderButtonContent("Open Launcher Folder"),
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new(6, 0, 0, 0)
    };

    sealed class FolderButtonContent : SimpleStackPanel
    {
        internal FolderButtonContent(string text)
        {
            Spacing = 12;
            Orientation = Orientation.Horizontal;

            Children.Add(new SymbolIcon(Symbol.MoveToFolder));
            Children.Add(new TextBlock { Text = text });
        }
    }

    internal FolderButtonsControl()
    {
        Rows = 1;
        Columns = 2;

        Children.Add(_clientFolderButton);
        Children.Add(_launcherFolderButton);

        _launcherFolderButton.Click += OnLauncherFolderButtonClick;
        _clientFolderButton.Click += OnClientFolderButtonClick;
    }

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