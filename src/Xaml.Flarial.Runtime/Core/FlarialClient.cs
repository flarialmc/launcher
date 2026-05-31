using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Modding;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Core;

sealed class FlarialClientRelease : FlarialClient
{
    protected override string Build => "Release";
    protected override string WindowClass => "Flarial Client";
    protected override string FileName => "Flarial.Client.Release.dll";
    protected override string DownloadUri => "https://cdn.flarial.xyz/dll/latest.dll";
}

public abstract class FlarialClient
{
    protected FlarialClient() { }

    public static FlarialClient Current { get; } = new FlarialClientRelease();

    protected abstract string Build { get; }
    protected abstract string FileName { get; }
    protected abstract string DownloadUri { get; }
    protected abstract string WindowClass { get; }

    public bool? Launch()
    {
        if (Minecraft.GetWindow(WindowClass) is { } client)
        {
            if (Minecraft.Current.GetWindow(client.ProcessId) is { } minecraft)
            {
                minecraft.Switch();
                return null;
            }
            return false;
        }
        return Injector.Launch(new(FileName)) is { };
    }

    public async Task<bool?> LaunchAsync() => await Task.Run(Launch);

    static readonly object s_lock = new();
    static readonly HashAlgorithm s_algorithm = SHA256.Create();

    const string HashesUrl = "https://cdn.flarial.xyz/dll_hashes.json";

    async Task<string> GetRemoteHashAsync()
    {
        using var stream = await HttpService.GetStreamAsync(HashesUrl);
        var json = await JsonService.ReadAsync<Dictionary<string, string>>(stream);
        return json[Build];
    }

    async Task<string> GetLocalHashAsync() => await Task.Run(() =>
    {
        try
        {
            lock (s_lock)
            {
                using var stream = File.OpenRead(FileName);
                var hash = s_algorithm.ComputeHash(stream);
                var value = BitConverter.ToString(hash);
                return value.Replace("-", string.Empty);
            }
        }
        catch { return string.Empty; }
    });

    public async Task<bool> DownloadAsync(Action<int> callback)
    {
        var localHashTask = GetLocalHashAsync();
        var remoteHashTask = GetRemoteHashAsync();
        await Task.WhenAll(localHashTask, remoteHashTask);

        if ((await localHashTask).Equals(await remoteHashTask, StringComparison.OrdinalIgnoreCase))
            return true;

        try { File.Delete(FileName); }
        catch { return false; }

        await HttpService.DownloadAsync(DownloadUri, FileName, callback);
        return true;
    }
}