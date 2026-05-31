using System;
using System.Collections.Concurrent;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

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
    static readonly ConcurrentDictionary<Type, StorePage<T>> s_pages = [];

    static StorePage<T> Get() => s_pages.GetOrAdd(typeof(T), static type => new T());

    public static void Open() => Get().OnOpen();
}

unsafe abstract class StorePage
{
    readonly string _uri;

    protected abstract string ProductId { get; }

    internal StorePage() => _uri = $"ms-windows-store://pdp/?ProductId={ProductId}";

    internal void OnOpen()
    {
        fixed (char* uri = _uri)
            ShellExecute(new(), null, uri, null, null, SW_NORMAL);
    }
}