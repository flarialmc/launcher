namespace Flarial.Launcher.Services.Networking;

static class ServicesEndpoints
{
    internal const string GameVersions = "https://raw.githubusercontent.com/ddf8196/mc-w10-versiondb-auto-update/master/versions.json.min";

    internal const string ClientHashes = "https://raw.githubusercontent.com/flarialmc/newcdn/main/dll_hashes.json";

    internal const string GameFrameworks = "https://api.nuget.org/v3/registration5-gz-semver2/microsoft.services.store.engagement/index.json";

    internal const string LauncherVersion = "https://raw.githubusercontent.com/flarialmc/newcdn/main/launcher/launcherVersion.txt";

    internal const string MicrosoftStore = "https://fe3cr.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured";

    internal const string SupportedVersions = "https://raw.githubusercontent.com/flarialmc/newcdn/main/launcher/NewSupported.txt";
}