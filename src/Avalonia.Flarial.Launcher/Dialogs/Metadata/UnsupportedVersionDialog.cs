using System.Collections.Generic;
using Flarial.Runtime.Versions;

namespace Flarial.Launcher.Dialogs.Metadata;

sealed class UnsupportedVersionDialog(string preferred) : MessageDialog
{
    protected override string[] Buttons { get; } = ["Back"];
    protected override string Title { get; } = "⚠️ Unsupported Version";

    protected override string Message
    {
        get
        {
            var version = VersionRegistry.InstalledVersion;

            if (_messages.TryGetValue(version, out var message))
                return message;

            message = string.Format(_format, version);
            _messages.Add(version, message);

            return message;
        }
    }

    readonly Dictionary<string, string> _messages = [];

    readonly string _format = $@"Minecraft {{0}} isn't supported by Flarial Client.

• Switch to {preferred} for the best experience.
• You may also try older versions if required.

If you need help, join our Discord.";
}
