namespace Flarial.Runtime.Core;

public sealed class FlarialClientRelease : FlarialClient<FlarialClientRelease>
{
    private protected override string Build => "Release";
    private protected override string FileName => "Flarial.Client.Release.dll";
    private protected override string DownloadUri => "https://cdn.flarial.xyz/dll/latest.dll";
}