using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.System;

unsafe readonly struct Win32Event : IDisposable
{
    readonly HANDLE _handle;

    public Win32Event() => _handle = CreateEvent(null, true, false, null);

    internal void Set() => SetEvent(_handle);

    public void Dispose() => CloseHandle(_handle);

    public static implicit operator HANDLE(Win32Event @this) => @this._handle;
}