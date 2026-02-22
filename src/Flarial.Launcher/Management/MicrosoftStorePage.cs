using static Windows.Win32.Foundation.HWND;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

namespace Flarial.Launcher.Management;

sealed class MinecraftPage : MicrosoftStorePage { protected override string ProductId => "9NBLGGH2JHXJ"; }
sealed class GamingServicesPage : MicrosoftStorePage { protected override string ProductId => "9MWPM2CQNLHN"; }

abstract class MicrosoftStorePage
{
    internal static MinecraftPage Minecraft => field ??= new();
    internal static GamingServicesPage GamingServices => field ??= new();

    readonly string _uri;

    protected abstract string ProductId { get; }

    internal MicrosoftStorePage() => _uri = $"ms-windows-store://pdp/?ProductId={ProductId}";

    unsafe internal void Open()
    {
        fixed (char* lpFile = _uri)
            ShellExecute(Null, null, lpFile, null, null, SW_NORMAL);
    }
}