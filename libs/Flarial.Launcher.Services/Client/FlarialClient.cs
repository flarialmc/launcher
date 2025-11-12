using System;
using System.Collections.Generic;
using System.IO;
using static System.StringComparison;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Modding;
using Flarial.Launcher.Services.Networking;
using Flarial.Launcher.Services.System;
using Windows.Data.Json;
using Flarial.Launcher.Services.Core;
using Windows.Win32.Foundation;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Diagnostics.ToolHelp.CREATE_TOOLHELP_SNAPSHOT_FLAGS;
using Windows.Win32.System.Diagnostics.ToolHelp;
using static Windows.Win32.Globalization.COMPARESTRING_RESULT;

namespace Flarial.Launcher.Services.Client;

public abstract partial class FlarialClient
{
    protected abstract string Identifer { get; }
    protected abstract string FileName { get; }
    protected abstract string BuildType { get; }
    protected abstract string DownloadUri { get; }
    internal FlarialClient() { }

    public static readonly FlarialClient Beta = new FlarialClientBeta(), Release = new FlarialClientRelease();
}

unsafe partial class FlarialClient
{
    static FlarialClient? Client
    {
        get
        {
            if (Minecraft.Current.Window?.ProcessId is not { } processId) return null;

            HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, processId); try
            {
                MODULEENTRY32W entry = new() { dwSize = (uint)sizeof(MODULEENTRY32W) };

                if (snapshot == INVALID_HANDLE_VALUE) return null;
                if (!Module32NextW(snapshot, &entry)) return null;

                fixed (char* beta = Path.GetFullPath(Beta.FileName))
                fixed (char* release = Path.GetFullPath(Release.FileName))
                    do
                    {
                        if (CompareStringOrdinal(beta, -1, entry.szExePath.Value, -1, true) is CSTR_EQUAL) return Beta;
                        if (CompareStringOrdinal(release, -1, entry.szExePath.Value, -1, true) is CSTR_EQUAL) return Release;
                    }
                    while (Module32NextW(snapshot, &entry));
            }
            finally { CloseHandle(snapshot); }

            return null;
        }
    }

}

partial class FlarialClient
{
    public bool Launch(bool initialized)
    {
        if (Client is { } client)
        {
            if (!ReferenceEquals(this, client)) return false;
            return Minecraft.Current.Launch(false) is { };
        }
        return Injector.Launch(initialized, FileName) is { };
    }
}

partial class FlarialClient
{
    static readonly object _lock = new();

    static readonly HashAlgorithm _algorithm = SHA256.Create();

    const string HashesUri = "https://cdn.flarial.xyz/dll_hashes.json";

    async Task<string> RemoteHashAsync()
    {
        var @string = await HttpService.GetAsync<string>(HashesUri);
        return JsonObject.Parse(@string)[BuildType].GetString();
    }

    async Task<string> LocalHashAsync() => await Task.Run(() =>
    {
        try
        {
            lock (_lock)
            {
                using var stream = File.OpenRead(FileName);
                var value = _algorithm.ComputeHash(stream);
                var @string = BitConverter.ToString(value);
                return @string.Replace("-", string.Empty);
            }
        }
        catch { return string.Empty; }
    });

    public async Task<bool> DownloadAsync(Action<int> action)
    {
        Task<string>[] tasks = [LocalHashAsync(), RemoteHashAsync()];

        await Task.WhenAll(tasks);
        if ((await tasks[0]).Equals(await tasks[1], OrdinalIgnoreCase)) return true;

        try { File.Delete(FileName); } catch { return false; }
        //     if (IsRunning) return false;
        await HttpService.DownloadAsync(DownloadUri, FileName, action);

        return true;
    }
}