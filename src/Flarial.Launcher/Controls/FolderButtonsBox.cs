using System.IO;
using Flarial.Launcher.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Flarial.Launcher.Controls;

sealed class FolderButtonsBox : Grid
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

    internal FolderButtonsBox()
    {
        var path = Path.Combine(GetFolderPath(LocalApplicationData), "Flarial");

        _clientFolderButton.Tag = Path.Combine(path, "Client");
        _launcherFolderButton.Tag = Path.Combine(path, "Launcher");

        _clientFolderButton.Click += OnButtonClick;
        _launcherFolderButton.Click += OnButtonClick;

        ColumnSpacing = 12;
        ColumnDefinitions.Add(new());
        ColumnDefinitions.Add(new());

        Grid.SetRow(_clientFolderButton, 0);
        Grid.SetColumn(_clientFolderButton, 0);

        Grid.SetRow(_launcherFolderButton, 0);
        Grid.SetColumn(_launcherFolderButton, 1);

        Children.Add(_clientFolderButton);
        Children.Add(_launcherFolderButton);
    }
}