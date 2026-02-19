namespace Flarial.Launcher.Management;

abstract class MicrosoftStorePage
{
    internal static MinecraftPage Minecraft => field ??= new();
    internal static GamingServicesPage GamingServices => field ??= new();

    protected abstract string ProductId { get; }

    internal void Open() => NativeMethods.ShellExecute($"ms-windows-store://pdp/?ProductId={ProductId}");
}

sealed class MinecraftPage : MicrosoftStorePage
{
    protected override string ProductId => "9NBLGGH2JHXJ";
}

sealed class GamingServicesPage : MicrosoftStorePage
{
    protected override string ProductId => "9MWPM2CQNLHN";
}