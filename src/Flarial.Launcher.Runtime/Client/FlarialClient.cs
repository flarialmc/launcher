using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.Modding;
using Flarial.Launcher.Runtime.Services;
using Flarial.Launcher.Runtime.System;
using static System.StringComparison;

namespace Flarial.Launcher.Runtime.Client;

sealed class FlarialClientRelease : FlarialClient
{
    protected override string Build => nameof(Release);
    protected override string Uri => "https://cdn.flarial.xyz/dll/latest.dll";
    protected override string Name => $"Flarial.Client.{nameof(Release)}.dll";
    protected override string Identifer => "34F45015-6EB6-4213-ABEF-F2967818E628";
}

public abstract class FlarialClient
{
    internal FlarialClient() { }
    static readonly JsonService<Dictionary<string, string>> s_json = JsonService<Dictionary<string, string>>.GetJson();

    protected abstract string Uri { get; }
    protected abstract string Name { get; }
    protected abstract string Build { get; }
    protected abstract string Identifer { get; }

    public static readonly FlarialClient Release = new FlarialClientRelease();

    static FlarialClient? Current
    {
        get
        {
            if (!Minecraft.Current.IsRunning) return null;
            using NativeMutex release = new(Release.Identifer);
            return release.Exists ? Release : null;
        }
    }

    public bool Launch(bool initialized)
    {
        if (Current is { } client)
        {
            if (!ReferenceEquals(this, client)) return false;
            return Minecraft.Current.Launch(false) is { };
        }

        if (Injector.Launch(initialized, new(Name)) is not { } processId)
            return false;

        using NativeMutex mutex = new(Identifer);
        return mutex.Duplicate(processId);
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
                using var stream = File.OpenRead(Name);
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

        try { File.Delete(Name); }
        catch { return false; }

        await HttpService.DownloadAsync(Uri, Name, callback);
        return true;
    }
}