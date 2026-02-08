namespace Flarial.Launcher.Management;

abstract class Product
{
    internal static readonly Product Minecraft = new Minecraft();
    internal static readonly Product GamingServices = new GamingServices();

    protected abstract string ProductId { get; }
    internal abstract string PackageFamilyName { get; }

    internal void OpenProductDetailsPage() => PInvoke.ShellExecute($"ms-windows-store://pdp/?ProductId={ProductId}");
}

file sealed class Minecraft : Product
{
    protected override string ProductId => "9NBLGGH2JHXJ";
    internal override string PackageFamilyName => "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
}

file sealed class GamingServices : Product
{
    protected override string ProductId => "9MWPM2CQNLHN";
    internal override string PackageFamilyName => "Microsoft.GamingServices_8wekyb3d8bbwe";
}