using System.Collections.Generic;
using Flarial.Launcher.Runtime.Versions;

namespace Flarial.Launcher.Interface.Dialogs;

sealed class UnsupportedVersionDialog(string preferred) : MainDialog
{
    protected override string CloseButtonText => "Back";
    protected override string PrimaryButtonText => "Versions";
    protected override string SecondaryButtonText => "Settings";
    protected override string Title => "⚠️ Unsupported Version";

    protected override string Content
    {
        get
        {
            var key = VersionRegistry.InstalledVersion;

            if (_collection.TryGetValue(key, out var value))
                return value;

            value = string.Format(_format, key);
            _collection.Add(key, value);

            return value;
        }
    }

    readonly Dictionary<string, string> _collection = [];

    readonly string _format = $@"Minecraft {{0}} isn't supported by Flarial Client.

• Switch to {preferred} on the [Versions] page.
• Enable the client's beta on the [Settings] page.

If you need help, join our Discord.";
}
