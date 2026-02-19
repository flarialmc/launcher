namespace Flarial.Launcher.Management;

abstract class MicrosoftStorePage
{
    internal static MicrosoftStorePage Minecraft => field ??= new Minecraft();
    internal static MicrosoftStorePage GamingServices => field ??= new GamingServices();

    protected abstract string ProductId { get; }

    internal void Open() => NativeMethods.ShellExecute($"ms-windows-store://pdp/?ProductId={ProductId}");
}

file sealed class Minecraft : MicrosoftStorePage
{
    protected override string ProductId => "9NBLGGH2JHXJ";
}

file sealed class GamingServices : MicrosoftStorePage
{
    protected override string ProductId => "9MWPM2CQNLHN";
}