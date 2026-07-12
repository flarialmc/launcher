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

public sealed partial class DiscordAccountModel(HomeViewModel model) : ReactiveObject
{
    const string ProfileImageUri = "avares://Flarial.Launcher/Assets/person_96dp_FF2438.png";

    static readonly byte[] s_avatar;

    static DiscordAccountModel()
    {
        using var source = AssetLoader.Open(new(ProfileImageUri));

        using MemoryStream destination = new();
        source.CopyTo(destination);

        s_avatar = destination.ToArray();
    }

    [Reactive] string _username = "Guest";
    [Reactive] DiscordRoleModel _role = new();
    [Reactive] Bitmap _avatar = new(new MemoryStream(s_avatar, false));

    public async Task LoginAsync(DiscordAccount account)
    {
        Username = account.Username;

        if (account.HasBetaAccess && !account.HasFlarialPlus) Dispatcher.UIThread.Post(() =>
        {
            _role.Name = "Tester";
            _role.Border = Brushes.DarkGray;
            _role.Background = Brushes.DimGray;
        }, DispatcherPriority.Background);

        if (account.HasFlarialPlus) Dispatcher.UIThread.Post(() =>
        {
            _role.Name = "Flarial+";
            _role.Border = Brushes.IndianRed;
            _role.Background = Brushes.DarkRed;
        }, DispatcherPriority.Background);

        Dispatcher.UIThread.Post(async () =>
        {
            if (await account.Avatar is { } avatar)
                Avatar = new(new MemoryStream(avatar, false));
        }, DispatcherPriority.Background);

        model.ShowPromotions = !account.HasFlarialPlus;
    }

    public void Logout()
    {
        Role = new();
        Username = "Guest";
        model.ShowPromotions = true;
        Avatar = new(new MemoryStream(s_avatar, false));
    }
}