namespace Flarial.Launcher.Services.Client;

sealed partial class FlarialClientBeta : FlarialClient
{
    protected override string Key => nameof(Beta);

    protected override string Path => $"Flarial.Client.{nameof(Beta)}.dll";

    protected override string Name => "6E41334A-423F-4A4F-9F41-5C440C9CCBDC";

    protected override string Uri => "https://cdn.flarial.xyz/dll/beta.dll";

    internal FlarialClientBeta() : base() { }
}