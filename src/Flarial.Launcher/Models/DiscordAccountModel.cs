using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.Models;

public sealed partial class DiscordAccountModel : ReactiveObject
{
    const string ProfileImageUri = "avares://Flarial.Launcher/Assets/person_96dp_FF2438.png";

    [Reactive] string _username = "Guest";
    [Reactive] DiscordRoleModel _discordRole = new();
    [Reactive] Bitmap _profileImage = new(AssetLoader.Open(new(ProfileImageUri)));

    public void Logout()
    {
        Username = "Guest";
        DiscordRole = new();
        ProfileImage = new(AssetLoader.Open(new(ProfileImageUri)));
    }
}