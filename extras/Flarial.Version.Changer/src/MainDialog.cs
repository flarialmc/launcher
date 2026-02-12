using System;
using System.Threading;
using System.Threading.Tasks;
using ModernWpf.Controls;

namespace Flarial.Launcher.Interface;

abstract class MainDialog
{
    internal MainDialog()
    {
        _dialog.Title = Title;
        _dialog.Content = Content;
        _dialog.CloseButtonText = CloseButtonText;
        _dialog.PrimaryButtonText = PrimaryButtonText;
        _dialog.SecondaryButtonText = SecondaryButtonText;
    }

    readonly ContentDialog _dialog = new();
    static readonly SemaphoreSlim s_semaphore = new(1, 1);

    protected abstract string Title { get; }
    protected abstract string Content { get; }
    protected virtual string? CloseButtonText { get; }
    protected abstract string PrimaryButtonText { get; }
    protected virtual string? SecondaryButtonText { get; }

    internal virtual async Task<bool> ShowAsync() => await PromptAsync() != ContentDialogResult.None;

    internal async Task<ContentDialogResult> PromptAsync()
    {
        await s_semaphore.WaitAsync();
        try { return await _dialog.ShowAsync(); }
        finally { s_semaphore.Release(); }
    }

    internal static readonly MainDialog NotInstalled = new NotInstalled();
    internal static readonly MainDialog SelectVersion = new SelectVersion();
    internal static readonly MainDialog InstallVersion = new InstallVersion();
    internal static readonly MainDialog UnpackagedInstall = new UnpackagedInstall();
    internal static readonly MainDialog GamingServicesMissing = new GamingServicesMissing();
}

file sealed class GamingServicesMissing : MainDialog
{
    internal override async Task<bool> ShowAsync()
    {
        var _ = await base.ShowAsync();
        if (_) Product.GamingServices.OpenProductDetailsPage();
        return _;
    }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Gaming Services Missing";
    protected override string Content => @"Gaming Services isn't installed, please install it.

• Gaming Services is required for GDK builds.
• You may install Gaming Services via the Microsoft Store.

If you need help, join our Discord.";
}

file sealed class SelectVersion : MainDialog
{
    protected override string PrimaryButtonText => "Back";
    protected override string Title => "💡 Select Version";
    protected override string Content => @"No Minecraft version is selected.

• Select a Minecraft version from the list that should be installed.

If you need help, join our Discord.";
}

file sealed class InstallVersion : MainDialog
{
    protected override string Title => "💡 Install Version";

    protected override string Content => @"The selected Minecraft version will be now installed.
Once the installation starts, you won't able to cancel it.

• Free up disk space before proceeding with the installation.
• A high speed internet connection is recommended for this.

If you need help, join our Discord.";

    protected override string PrimaryButtonText => "Install";

    protected override string CloseButtonText => "Cancel";
}

[Obsolete("", true)]
file sealed class CannotFind : MainDialog
{
    protected override string Title => "⚠️ Cannot Find";

    protected override string Content => @"The client's folder cannot be found.

• Try launching the client at least once to generate its folder.

If you need help, join our Discord.";

    protected override string PrimaryButtonText => "Back";
}

file sealed class NotInstalled : MainDialog
{
    internal override async Task<bool> ShowAsync()
    {
        var _ = await base.ShowAsync();
        if (_) Product.Minecraft.OpenProductDetailsPage();
        return _;
    }

    protected override string CloseButtonText => "Cancel";
    protected override string PrimaryButtonText => "Install";
    protected override string Title => "⚠️ Not Installed";
    protected override string Content => @"Minecraft: Bedrock Edition isn't installed.

• Install Minecraft: Bedrock Edition via the Microsoft Store or Xbox App.

If you need help, join our Discord.";
}

file sealed class UnpackagedInstall : MainDialog
{
    protected override string Title => "⚠️ Unpackaged Install";
    protected override string PrimaryButtonText => "Back";
    protected override string Content => @"The current Minecraft install is unpackaged.

• Please reinstall the game via the Microsoft or Xbox App.
• The launcher can only switch versions if the install is packaged.

If you need help, join our Discord.";
}