namespace Flarial.Launcher.Services.Client;

sealed partial class FlarialClientBeta : FlarialClient
{
    const string Key = "Beta";

    const string Path = "Flarial.Client.Beta.dll";

    const string Name = "6E41334A-423F-4A4F-9F41-5C440C9CCBDC";

    const string Uri = "https://raw.githubusercontent.com/flarialmc/newcdn/main/dll/beta.dll";

    internal FlarialClientBeta() : base(Name, Path, Key, Uri) { }
}