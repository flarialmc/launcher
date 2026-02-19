namespace Flarial.Launcher.Management;

abstract class MicrosoftStorePage
{
    internal static MinecraftStorePage Minecraft => field ??= new();
    internal static GamingServicesStorePage GamingServices => field ??= new();

    protected abstract string ProductId { get; }

    internal void Open() => NativeMethods.ShellExecute($"ms-windows-store://pdp/?ProductId={ProductId}");
}

sealed class MinecraftStorePage : MicrosoftStorePage
{
    protected override string ProductId => "9NBLGGH2JHXJ";
}

sealed class GamingServicesStorePage : MicrosoftStorePage
{
    protected override string ProductId => "9MWPM2CQNLHN";
}