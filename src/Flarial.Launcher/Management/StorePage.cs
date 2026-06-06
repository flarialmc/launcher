using System;
using System.Collections.Concurrent;
using Flarial.Runtime.Unmanaged;

namespace Flarial.Launcher.Management;

sealed class MinecraftPage : StorePage<MinecraftPage>
{
    protected override string ProductId => "9NBLGGH2JHXJ";
}

sealed class GamingServicesPage : StorePage<GamingServicesPage>
{
    protected override string ProductId => "9MWPM2CQNLHN";
}

abstract class StorePage<T> : StorePage where T : StorePage<T>, new()
{
    internal static readonly T _ = new();
}

abstract class StorePage
{
    readonly string _uri;

    protected abstract string ProductId { get; }

    internal StorePage() => _uri = $"ms-windows-store://pdp/?ProductId={ProductId}";

    internal void Open() => NativeMethods.ShellExecute(_uri);
}