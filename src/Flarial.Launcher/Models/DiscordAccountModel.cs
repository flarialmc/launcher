using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Flarial.Launcher.ViewModels;
using Flarial.Runtime.Discord;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.Models;

public sealed partial class DiscordAccountModel : ReactiveObject
{
    [Reactive] Bitmap _avatar;
    [Reactive] string _username;
    [Reactive] DiscordRoleModel _role;

    const string DefaultUserName = "Guest";
    const string ProfileImageUri = "avares://Flarial.Launcher/Assets/avatar.webp";

    readonly Bitmap _defaultAvatar;

    internal DiscordAccountModel()
    {
        Uri uri = new(ProfileImageUri);

        using var stream = AssetLoader.Open(uri);
        _defaultAvatar = new(stream);

        Role = new();
        Avatar = _defaultAvatar;
        Username = DefaultUserName;
    }

    public void Login(DiscordAccount account) => Dispatcher.UIThread.Post(async () =>
    {
        var hasBetaAccess = account.HasBetaAccess;
        var hasFlarialPlus = account.HasFlarialPlus;

        Username = account.Username;

        if (hasBetaAccess && !hasFlarialPlus)
        {
            Role.Name = "Tester";
            Role.Border = Brushes.DarkGray;
            Role.Background = Brushes.DimGray;
        }
        else if (hasBetaAccess)
        {
            Role.Name = "Flarial+";
            Role.Border = Brushes.IndianRed;
            Role.Background = Brushes.DarkRed;
        }

        if (await account.GetAvatarAsync() is { } avatar)
        {
            using MemoryStream stream = new(avatar, false);
            Avatar = new(stream);
        }
        else Avatar = _defaultAvatar;
    }, DispatcherPriority.Background);

    public void Logout() => Dispatcher.UIThread.Post(() =>
    {
        Role.Logout();
        Avatar = _defaultAvatar;
        Username = DefaultUserName;
    }, DispatcherPriority.Background);
}