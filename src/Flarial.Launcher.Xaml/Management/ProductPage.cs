namespace Flarial.Launcher.Management;

abstract class ProductPage
{
    internal static readonly ProductPage s_minecraft = new Minecraft();
    internal static readonly ProductPage s_gamingServices = new GamingServices();

    protected abstract string ProductId { get; }

    internal void Open() => NativeMethods.ShellExecute($"ms-windows-store://pdp/?ProductId={ProductId}");
}

file sealed class Minecraft : ProductPage
{
    protected override string ProductId => "9NBLGGH2JHXJ";
}

file sealed class GamingServices : ProductPage
{
    protected override string ProductId => "9MWPM2CQNLHN";
}