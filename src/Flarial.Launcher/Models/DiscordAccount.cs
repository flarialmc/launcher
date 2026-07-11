using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.Models;

public partial class DiscordAccount : ReactiveObject
{
    [Reactive]
    private string _userName = "Guest";

    [Reactive]
    private DiscordRole _discordRole = new();

    [Reactive]
    private Bitmap _profileImage = new(AssetLoader.Open(new("avares://Flarial.Launcher/Assets/person_96dp_FF2438.png")));
}