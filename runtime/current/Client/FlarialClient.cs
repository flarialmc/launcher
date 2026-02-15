using System;
using System.IO;
using static System.StringComparison;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Launcher.Runtime.Modding;
using Flarial.Launcher.Runtime.Networking;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.System;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;

namespace Flarial.Launcher.Runtime.Client;

sealed class FlarialClientBeta : FlarialClient
{
    internal FlarialClientBeta() : base() { }
    protected override string Build => nameof(Beta);
    protected override string Name => $"Flarial.Client.{nameof(Beta)}.dll";
    protected override string Url => "https://cdn.flarial.xyz/dll/beta.dll";
    protected override string Identifer => "6E41334A-423F-4A4F-9F41-5C440C9CCBDC";
}

sealed class FlarialClientRelease : FlarialClient
{
    internal FlarialClientRelease() : base() { }
    protected override string Build => nameof(Release);
    protected override string Url => "https://cdn.flarial.xyz/dll/latest.dll";
    protected override string Name => $"Flarial.Client.{nameof(Release)}.dll";
    protected override string Identifer => "34F45015-6EB6-4213-ABEF-F2967818E628";
}

public abstract class FlarialClient
{
    internal FlarialClient() { }
    static readonly DataContractJsonSerializer s_serializer = JsonService.Get<Dictionary<string, string>>();

    protected abstract string Url { get; }
    protected abstract string Name { get; }
    protected abstract string Build { get; }
    protected abstract string Identifer { get; }

    public static readonly FlarialClient Beta = new FlarialClientBeta();
    public static readonly FlarialClient Release = new FlarialClientRelease();

    static FlarialClient? Client
    {
        get
        {
            using NativeMutex beta = new(Beta.Identifer);
            using NativeMutex release = new(Release.Identifer);

            if (!Minecraft.Current.IsRunning)
                return null;

            if (beta.Exists && release.Exists)
                return null;

            if (beta.Exists)
                return Beta;

            if (release.Exists)
                return Release;

            return null;
        }
    }

    public bool Launch(bool initialized)
    {
        if (Client is { } client)
        {
            if (!ReferenceEquals(this, client))
                return false;

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
        var items = (Dictionary<string,string>)s_serializer.ReadObject(stream);
        return items[Build];
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

    public async Task<bool> DownloadAsync(Action<int> action)
    {
        var localHashTask = GetLocalHashAsync();
        var remoteHashTask = GetRemoteHashAsync();
        await Task.WhenAll(localHashTask, remoteHashTask);

        if ((await localHashTask).Equals(await remoteHashTask, OrdinalIgnoreCase))
            return true;

        try { File.Delete(Name); }
        catch { return false; }

        await HttpService.DownloadAsync(Url, Name, action);
        return true;
    }

    const string AcceptedUrl = "https://cdn.flarial.xyz/202.txt";

    public static async Task<bool> CanConnectAsync()
    {
        try
        {
            using var message = await HttpService.GetAsync(AcceptedUrl);
            return message.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}