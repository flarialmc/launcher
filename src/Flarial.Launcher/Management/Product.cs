namespace Flarial.Launcher.Management;

abstract class Product
{
    internal static readonly Product Minecraft = new Minecraft();
    internal static readonly Product GamingServices = new GamingServices();

    protected abstract string ProductId { get; }

    internal void OpenProductDetailsPage() => NativeMethods.ShellExecute($"ms-windows-store://pdp/?ProductId={ProductId}");
}

file sealed class Minecraft : Product
{
    protected override string ProductId => "9NBLGGH2JHXJ";
}

file sealed class GamingServices : Product
{
    protected override string ProductId => "9MWPM2CQNLHN";
}