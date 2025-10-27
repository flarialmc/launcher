namespace Flarial.Launcher.Services.Client;

sealed partial class FlarialClientRelease : FlarialClient
{
    const string Key = "Release";

    const string Name = "34F45015-6EB6-4213-ABEF-F2967818E628";

    const string Path = "Flarial.Client.Release.dll";

    const string Uri = "https://cdn.flarial.xyz/dll/latest.dll";

    internal FlarialClientRelease() : base(Name, Path, Key, Uri) { }
}