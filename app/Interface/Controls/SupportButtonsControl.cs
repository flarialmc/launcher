using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static System.Environment;
using static Flarial.Launcher.Interface.MessageDialog;
using static Windows.Management.Core.ApplicationDataManager;
using static Flarial.Launcher.Services.Core.Minecraft;
using System.Windows.Interop;
using System;

namespace Flarial.Launcher.Interface.Controls;

sealed class SupportButtonsControl : UniformGrid
{
    readonly WindowInteropHelper _helper;

    readonly Button _clientFolderButton = new()
    {
        Content = "Open Client Folder",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new(6, 0, 6, 0)
    };

    readonly Button _launcherFolderButton = new()
    {
        Content = "Open Launcher Folder",
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new(6, 0, 0, 0)
    };

    readonly string _launcherPath = CurrentDirectory;
    readonly string _gdkPath = Path.Combine(CurrentDirectory, @"..\Client");

    string UWPPath => Path.Combine(CreateForPackageFamily(PackageFamilyName).RoamingFolder.Path, "Flarial");

    void ShellExecute(string lpFile) => PInvoke.ShellExecute(_helper.EnsureHandle(), null!, lpFile, null!, null!, PInvoke.SW_NORMAL);

    void OnLauncherFolderButtonClick(object sender, EventArgs args) => ShellExecute(_launcherPath);

    async void OnClientFolderButtonClick(object sender, EventArgs args)
    {
        if (!IsInstalled)
        {
            await _notInstalled.ShowAsync();
            return;
        }

        var path = UsingGameDevelopmentKit ? _gdkPath : UWPPath;

        if (!Directory.Exists(path))
        {
            await _folderNotFound.ShowAsync();
            return;
        }

        ShellExecute(path);
    }

    internal SupportButtonsControl(WindowInteropHelper helper)
    {
        _helper = helper;

        Rows = 1;
        Children.Add(_clientFolderButton);
        Children.Add(_launcherFolderButton);

        _clientFolderButton.Click += OnClientFolderButtonClick;
        _launcherFolderButton.Click += OnLauncherFolderButtonClick;
    }
}