namespace Flarial.Launcher.Services.Client;

sealed partial class FlarialClientBeta : FlarialClient
{
    protected override string BuildType => nameof(Beta);

    protected override string FileName => $"Flarial.Client.{nameof(Beta)}.dll";

    protected override string Identifer => "6E41334A-423F-4A4F-9F41-5C440C9CCBDC";

    protected override string DownloadUri => "https://cdn.flarial.xyz/dll/beta.dll";

    internal FlarialClientBeta() : base() { }
}