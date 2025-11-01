namespace Flarial.Launcher.Services.Client;

sealed partial class FlarialClientRelease : FlarialClient
{
    protected override string Key => nameof(Release);

    protected override string Path => $"Flarial.Client.{nameof(Release)}.dll";

    protected override string Name => "34F45015-6EB6-4213-ABEF-F2967818E628";

    protected override string Uri => "https://cdn.flarial.xyz/dll/latest.dll";

    internal FlarialClientRelease() : base() { }
}