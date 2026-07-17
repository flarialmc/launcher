namespace Flarial.Runtime.Core;

public sealed class FlarialClientBeta : FlarialClient<FlarialClientBeta>
{
    private protected override string Build => "Beta";
    private protected override string FileName => "Flarial.Client.Beta.dll";
    private protected override string DownloadUri => "https://cdn.flarial.xyz/dll/beta.dll";
}