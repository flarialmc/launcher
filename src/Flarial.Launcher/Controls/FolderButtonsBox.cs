using System;
using System.IO;
using Flarial.Launcher.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Flarial.Launcher.Controls;

sealed class FolderButtonsBox : XamlElement<Grid>
{
    readonly Button _clientFolderButton = new()
    {
        Content = "Open Client Folder",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    readonly Button _launcherFolderButton = new()
    {
        Content = "Open Launcher Folder",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    static void OnButtonClick(object sender, RoutedEventArgs args)
    {
        var button = (Button)sender;
        var path = (string)button.Tag;

        Directory.CreateDirectory(path);
        NativeMethods.ShellExecute(path);
    }

    internal FolderButtonsBox() : base(new())
    {
        var path = Path.Combine(GetFolderPath(LocalApplicationData), "Flarial");

        _clientFolderButton.Tag = Path.Combine(path, "Client");
        _launcherFolderButton.Tag = Path.Combine(path, "Launcher");

        _clientFolderButton.Click += OnButtonClick;
        _launcherFolderButton.Click += OnButtonClick;

        @this.ColumnSpacing = 12;
        @this.ColumnDefinitions.Add(new());
        @this.ColumnDefinitions.Add(new());

        Grid.SetRow(_clientFolderButton, 0);
        Grid.SetColumn(_clientFolderButton, 0);

        Grid.SetRow(_launcherFolderButton, 0);
        Grid.SetColumn(_launcherFolderButton, 1);

        @this.Children.Add(_clientFolderButton);
        @this.Children.Add(_launcherFolderButton);
    }
}