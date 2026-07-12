using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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

        if (account.HasBetaAccess && !account.HasFlarialPlus)
        {
            _role.Name = "Tester";
            _role.Border = Brushes.DarkGray;
            _role.Background = Brushes.DimGray;
        }

        if (account.HasFlarialPlus)
        {
            _role.Name = "Flarial+";
            _role.Border = Brushes.IndianRed;
            _role.Background = Brushes.DarkRed;
        }

        model.ShowPromotions = !account.HasFlarialPlus;
        Avatar = new(new MemoryStream(await account.Avatar, false));
    }

    public void Logout()
    {
        Role = new();
        Username = "Guest";
        model.ShowPromotions = true;
        Avatar = new(new MemoryStream(s_avatar, false));
    }
}