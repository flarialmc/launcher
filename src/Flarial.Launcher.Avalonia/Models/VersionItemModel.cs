using System;

namespace Flarial.Launcher.Models;

[Obsolete]
public record VersionItemData(string Version, VersionItemState State);

public enum VersionItemState
{
    NotInstalled,
    Downloading,
    Installed,
    Installing
}