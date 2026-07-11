using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.Models;

public partial class UserState : ReactiveObject
{
    [Reactive]
    private Uri _pfpSource = new("avares://Flarial.Launcher/Assets/person_96dp_FF2438.png");

    [Reactive]
    private string _username = "Guest";

    [Reactive]
    private Role _role = new();
}