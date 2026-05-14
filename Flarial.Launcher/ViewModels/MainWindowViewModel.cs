using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using Flarial.Launcher.Management;
using Flarial.Launcher.Services;
using Flarial.Runtime.Core;
using Flarial.Runtime.Game;
using Flarial.Runtime.Versions;
using ReactiveUI;
using Windows.ApplicationModel;

namespace Flarial.Launcher.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDialogService, INotificationService
{
    readonly AppSettings _settings;
    readonly PackageCatalog _catalog;

    public MessageBoxViewModel? CurrentDialog
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public HomeViewModel HomeViewModel { get; }

    public SettingsViewModel? SettingsViewModel
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public NotificationAreaViewModel NotificationArea { get; } = new();

    public bool IsVersionInstallActive
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public MainWindowViewModel(AppSettings settings)
    {
        _settings = settings;
        _catalog = PackageCatalog.OpenForCurrentUser();
        HomeViewModel = new HomeViewModel(settings, this, this);
    }

    public async Task InitializeSettingsAsync()
    {
        SettingsViewModel = new SettingsViewModel(_settings, this, HomeViewModel);

        if (!await FlarialLauncher.VerifyConnectionAsync())
        {
            await ShowMessageBoxAsync("Connection failed", "Unable to connect to the Flarial servers.", ["Close"]);
            HomeViewModel.Status = "Offline";
            return;
        }

        if (await FlarialLauncher.CheckForUpdatesAsync())
        {
            if (_settings.AutomaticUpdates
                || await ConfirmAsync(
                    "Launcher update available",
                    "An update is available for the launcher.\n\nUpdating the launcher provides fixes and new features. New versions of the client and Minecraft might require a launcher update.",
                    "Update",
                    "Later"))
            {
                HomeViewModel.Status = "Updating...";
                HomeViewModel.LaunchText = "Updating...";

                await FlarialLauncher.DownloadAsync(value =>
                    Dispatcher.UIThread.Post(() => HomeViewModel.LaunchText = $"Updating... {value}%"));
                return;
            }
        }

        var registry = await VersionRegistry.CreateAsync();
        HomeViewModel.SetVersionRegistry(registry);
        SettingsViewModel.SettingsVersionsViewModel.SetVersionRegistry(registry);

        UpdateMinecraftStatus();
        RegisterPackageEvents();

        HomeViewModel.IsLaunchEnabled = true;
        HomeViewModel.LaunchText = "Launch";
        HomeViewModel.Status = "Ready!";
    }

    void RegisterPackageEvents()
    {
        _catalog.PackageInstalling += (_, args) =>
        {
            if (args.IsComplete) UpdateMinecraftStatus(args.Package.Id.FamilyName);
        };
        _catalog.PackageUninstalling += (_, args) =>
        {
            if (args.IsComplete) UpdateMinecraftStatus(args.Package.Id.FamilyName);
        };
        _catalog.PackageUpdating += (_, args) =>
        {
            if (args.IsComplete) UpdateMinecraftStatus(args.TargetPackage.Id.FamilyName);
        };
    }

    void UpdateMinecraftStatus(string? packageFamilyName = null)
    {
        if (packageFamilyName is not null
            && !packageFamilyName.Equals(Minecraft.PackageFamilyName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Dispatcher.UIThread.Post(HomeViewModel.UpdateMinecraftStatus);
    }

    public async Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
        => await ShowMessageBoxAsync(title, message, [accept, cancel]) == accept;

    public void SetVersionInstallActive(bool active) => IsVersionInstallActive = active;

    public void NotifyInstallCloseBlocked() => NotificationArea.Add("You cannot close the launcher while a version installation is in progress.");

    public async Task<string> ShowMessageBoxAsync(
        string title,
        string message,
        IEnumerable<string> buttons)
    {
        var dialog = new MessageBoxViewModel(title, message, buttons);
        CurrentDialog = dialog;

        var result = await dialog.Result;

        CurrentDialog = null;
        return result;
    }

    void INotificationService.Show(string message) => NotificationArea.Add(message);
}
