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
        model.ShowPromotions = !account.HasFlarialPlus;

        var hasBetaAccess = account.HasBetaAccess;
        var hasFlarialPlus = account.HasFlarialPlus;

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
            Avatar = new(new MemoryStream(avatar, false));
    }

    public void Logout()
    {
        Role.Logout();
        Username = "Guest";
        model.ShowPromotions = true;
        Avatar = new(new MemoryStream(s_avatar, false));
    }
}