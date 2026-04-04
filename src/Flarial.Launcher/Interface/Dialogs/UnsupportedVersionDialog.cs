using System.Collections.Generic;
using Flarial.Runtime.Versions;

namespace Flarial.Launcher.Interface.Dialogs;

sealed class UnsupportedVersionDialog(string preferred) : MasterDialog
{
    protected override string CloseButtonText => "Back";
    protected override string PrimaryButtonText => "Versions";
    protected override string Title => "⚠️ Unsupported Version";

    protected override string Content
    {
        get
        {
            var version = VersionRegistry.InstalledVersion;

            if (_contentCache.TryGetValue(version, out var content))
                return content;

            content = string.Format(_format, version);
            _contentCache.Add(version, content);

            return content;
        }
    }

    readonly Dictionary<string, string> _contentCache = [];

    readonly string _format = $@"Minecraft {{0}} isn't supported by Flarial Client.

• Switch to {preferred} on the [Versions] page.

If you need help, join our Discord.";
}
