using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Windows.Win32.Foundation.HWND;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

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

    unsafe static void OnButtonClick(object sender, RoutedEventArgs args)
    {
        var button = (Button)sender;

        var path = (string)button.Tag;
        Directory.CreateDirectory(path);

        fixed (char* lpFile = path)
            ShellExecute(Null, null, lpFile, null, null, SW_NORMAL);
    }

    internal FolderButtonsBox()
    {
        _launcherFolderButton.Tag = ".";
        _clientFolderButton.Tag = @"..\Client";

        _clientFolderButton.Click += OnButtonClick;
        _launcherFolderButton.Click += OnButtonClick;

        ColumnSpacing = 12;
        ColumnDefinitions.Add(new());
        ColumnDefinitions.Add(new());

        SetRow(_clientFolderButton, 0);
        SetColumn(_clientFolderButton, 0);

        SetRow(_launcherFolderButton, 0);
        SetColumn(_launcherFolderButton, 1);

        Children.Add(_clientFolderButton);
        Children.Add(_launcherFolderButton);
    }
}