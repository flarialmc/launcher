namespace Flarial.Launcher.Services.Client;

sealed partial class FlarialClientRelease : FlarialClient
{
    protected override string Build => nameof(Release);

    protected override string Name => $"Flarial.Client.{nameof(Release)}.dll";

    protected override string Identifer => "34F45015-6EB6-4213-ABEF-F2967818E628";

    protected override string Url => "https://cdn.flarial.xyz/dll/latest.dll";

    internal FlarialClientRelease() : base() { }
}