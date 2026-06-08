using System.Collections.Generic;
using Flarial.Launcher.Interface.Presentation;
using Flarial.Runtime.Versions;

namespace Flarial.Launcher.Interface.Dialogs;

sealed class UnsupportedVersionDialog(VersionRegistry registry) : AppDialog
{
    protected override string CloseButtonText => "Back";
    protected override string PrimaryButtonText => "Versions";
    protected override string Title => "⚠️ Unsupported Version";

    protected override string Content
    {
        get
        {
            var version = VersionRegistry.InstalledVersion;

            if (_cache.TryGetValue(version, out var content))
                return content;

            content = string.Format(_format, version);
            _cache.Add(version, content);

            return content;
        }
    }

    readonly Dictionary<string, string> _cache = [];

    readonly string _format = $@"Minecraft {{0}} isn't supported by Flarial Client.

• Switch to {registry.PreferredVersion} on the [Versions] page.

If you need help, join our Discord.";
}
