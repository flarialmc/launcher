using System;
using Flarial.Launcher.Types;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Flarial.Launcher.Management;

public partial class UserState : ReactiveObject
{
    [Reactive]
    private Uri _pfpSource = new("https://raw.githubusercontent.com/megahendick/Flarial.Laucher.Testing/refs/heads/main/person_24dp_FF2438.png", UriKind.Absolute);

    [Reactive]
    private string _username = "Guest";

    [Reactive]
    private Role _role = new();
}