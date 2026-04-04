using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Modding;
using Flarial.Runtime.Services;
using static System.StringComparison;

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

    static readonly JsonSerializer<Dictionary<string, string>> s_serializer = JsonSerializer<Dictionary<string, string>>.Get();

    protected abstract string Build { get; }
    protected abstract string FileName { get; }
    protected abstract string DownloadUri { get; }
    protected abstract string WindowClass { get; }

    public bool Launch(bool initialized)
    {
        if (Minecraft.GetWindow(WindowClass) is { } clientWindow)
        {
            if (Minecraft.Current.GetWindow(clientWindow._processId) is { } minecraftWindow)
            {
                minecraftWindow.Switch();
                return true;
            }
            return false;
        }

        Library library = new(FileName);
        if (!library.IsLoadable) return false;

        return Injector.Launch(initialized, library) is { };
    }

    static readonly object _lock = new();
    static readonly HashAlgorithm _algorithm = SHA256.Create();

    const string HashesUrl = "https://cdn.flarial.xyz/dll_hashes.json";

    async Task<string> GetRemoteHashAsync()
    {
        using var stream = await HttpStack.GetStreamAsync(HashesUrl);
        return (await s_serializer.DeserializeAsync(stream))[Build];
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

        await HttpStack.DownloadAsync(DownloadUri, FileName, callback);
        return true;
    }
}