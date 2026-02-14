using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Flarial.Launcher.Runtime.Game;
using System;
using Windows.Management.Core;

namespace Flarial.Launcher.Interface.Controls;

sealed class SupportButtonsControl : UniformGrid
{
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

    readonly string _launcherPath = Environment.CurrentDirectory;
    readonly string _gdkPath = Path.Combine(Environment.CurrentDirectory, @"..\Client");

    void OnLauncherFolderButtonClick(object sender, EventArgs args) => PInvoke.ShellExecute(_launcherPath);

    async void OnClientFolderButtonClick(object sender, EventArgs args)
    {
        if (!Minecraft.IsInstalled)
        {
            await AppDialog.NotInstalled.ShowAsync();
            return;
        }

        PInvoke.ShellExecute(Directory.CreateDirectory(Minecraft.UsingGameDevelopmentKit switch
        {
            true => Directory.CreateDirectory(_gdkPath).FullName,
            false => Path.Combine(ApplicationDataManager.CreateForPackageFamily(Minecraft.PackageFamilyName).RoamingFolder.Path, "Flarial")
        }).FullName);
    }

    internal SupportButtonsControl()
    {

        Rows = 1;
        Children.Add(_clientFolderButton);
        Children.Add(_launcherFolderButton);

        _clientFolderButton.Click += OnClientFolderButtonClick;
        _launcherFolderButton.Click += OnLauncherFolderButtonClick;
    }
}