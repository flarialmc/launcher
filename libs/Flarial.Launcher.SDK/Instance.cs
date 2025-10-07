using System;
using System.Runtime.InteropServices;
using static Flarial.Launcher.SDK.Native;

static class Instance
{
    internal readonly ref struct Mutex : IDisposable
    {
        readonly nint _;

        internal readonly bool Exists;

        public static implicit operator Mutex(string @this) => new(@this);

        public static implicit operator nint(Mutex @this) => @this._;

        Mutex(string value) => Exists = (_ = CreateMutex(default, false, value)) != default && Marshal.GetLastWin32Error() != default;

        public void Dispose() => CloseHandle(_);
    }

    internal readonly ref struct Process : IDisposable
    {
        readonly nint _;

        public static implicit operator Process(int @this) => new(@this);

        public static implicit operator nint(Process @this) => @this._;

        Process(int value) => _ = OpenProcess(PROCESS_ALL_ACCESS, false, value);

        public void Dispose() => CloseHandle(_);
    }

    internal static bool Exists(in Mutex mutex) { using (mutex) return mutex.Exists; }

    internal static void Create(in Process process, in Mutex mutex)
    {
        using (process) using (mutex)
        {
            nint handle = default;
            try { DuplicateHandle(GetCurrentProcess(), mutex, process, out handle, default, false, DUPLICATE_SAME_ACCESS); }
            finally { CloseHandle(handle); }
        }
    }
}