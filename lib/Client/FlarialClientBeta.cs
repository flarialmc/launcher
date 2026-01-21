namespace Flarial.Launcher.Services.Client;

sealed partial class FlarialClientBeta : FlarialClient
{
    protected override string Build => nameof(Beta);

    protected override string Name => $"Flarial.Client.{nameof(Beta)}.dll";

    protected override string Identifer => "6E41334A-423F-4A4F-9F41-5C440C9CCBDC";

    protected override string Url => "https://cdn.flarial.xyz/dll/beta.dll";

    internal FlarialClientBeta() : base() { }
}