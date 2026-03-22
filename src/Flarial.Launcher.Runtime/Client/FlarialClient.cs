using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.Modding;
using Flarial.Launcher.Runtime.Services;
using static System.StringComparison;

namespace Flarial.Launcher.Runtime.Client;

sealed class FlarialClientRelease : FlarialClient
{
    protected override string Build => "Release";
    protected override string WindowClass => "Flarial Client";
    protected override string FileName => "Flarial.Client.Release.dll";
    protected override string DownloadUri => "https://cdn.flarial.xyz/dll/latest.dll";
}

public abstract class FlarialClient
{
    internal FlarialClient() { }

    public static FlarialClient Current { get; } = new FlarialClientRelease();
    static readonly JsonService<Dictionary<string, string>> s_json = JsonService<Dictionary<string, string>>.GetJson();

    protected abstract string Build { get; }
    protected abstract string FileName { get; }
    protected abstract string DownloadUri { get; }
    protected abstract string WindowClass { get; }

    public bool? Launch(bool initialized)
    {
        if (Minecraft.GetWindow(WindowClass) is { } client)
        {
            if (Minecraft.Current.GetWindow(client._processId) is { } minecraft)
            {
                minecraft.SwitchWindow();
                return true;
            }
            return false;
        }

        Library library = new(FileName);
        if (!library.IsLoadable) return null;
        
        return Injector.Launch(initialized, library) is { };
    }

    static readonly object _lock = new();
    static readonly HashAlgorithm _algorithm = SHA256.Create();

    const string HashesUrl = "https://cdn.flarial.xyz/dll_hashes.json";

    async Task<string> GetRemoteHashAsync()
    {
        using var stream = await HttpService.GetStreamAsync(HashesUrl);
        return s_json.ReadStream(stream)[Build];
    }

    async Task<string> GetLocalHashAsync() => await Task.Run(() =>
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

    public async Task<bool> DownloadAsync(Action<int> callback)
    {
        var localHashTask = GetLocalHashAsync();
        var remoteHashTask = GetRemoteHashAsync();
        await Task.WhenAll(localHashTask, remoteHashTask);

        if ((await localHashTask).Equals(await remoteHashTask, OrdinalIgnoreCase))
            return true;

        try { File.Delete(FileName); }
        catch { return false; }

        await HttpService.DownloadAsync(DownloadUri, FileName, callback);
        return true;
    }
}