namespace Flarial.Launcher.Models;

public record VersionItemData (string Version, VersionItemState State);

public enum VersionItemState
{
    NotInstalled,
    Downloading,
    Installed,
}