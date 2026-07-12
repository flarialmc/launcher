using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Flarial.Runtime.Discord;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.Models;

public sealed partial class DiscordAccountModel : ReactiveObject
{
    const string ProfileImageUri = "avares://Flarial.Launcher/Assets/person_96dp_FF2438.png";

    static readonly byte[] s_profileImage;

    static DiscordAccountModel()
    {
        using var source = AssetLoader.Open(new(ProfileImageUri));

        using MemoryStream destination = new();
        source.CopyTo(destination);

        s_profileImage = destination.ToArray();
    }

    [Reactive] string _username = "Guest";
    [Reactive] DiscordRoleModel _discordRole = new();
    [Reactive] Bitmap _profileImage = new(new MemoryStream(s_profileImage));

    public void Login(DiscordAccount account)
    {
        Username = account.Username;
    }

    public void Logout()
    {
        Username = "Guest";
        DiscordRole = new();
        ProfileImage = new(new MemoryStream(s_profileImage));
    }
}