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

namespace Flarial.Launcher.Services.Client;

public abstract partial class FlarialClient
{
    protected abstract string Name { get; }
    protected abstract string Path { get; }
    protected abstract string Key { get; }
    protected abstract string Uri { get; }
    internal FlarialClient() { }
}

partial class FlarialClient
{
    static readonly List<FlarialClient> s_clients = [];

    static FlarialClient()
    {
        s_clients.Add(Beta = new FlarialClientBeta());
        s_clients.Add(Release = new FlarialClientRelease());
    }

    public static readonly FlarialClient Release, Beta;
}

partial class FlarialClient
{
    bool IsInjectable
    {
        get
        {
            foreach (var client in s_clients)
            {
                if (ReferenceEquals(this, client)) continue;
                else if (client.IsRunning) return false;
            }
            return true;
        }
    }

    bool IsRunning
    {
        get
        {
            if (!Minecraft.Current.IsRunning) return false;
            using Win32Mutex mutex = new(Name); return mutex.Exists;
        }
    }
}

partial class FlarialClient
{
    public bool Launch(bool initialized)
    {
        if (!IsInjectable) return false;
        if (IsRunning) return Minecraft.Current.Launch(initialized) is { };

        if (Injector.Launch(initialized, Path) is not { } processId) return false;
        using Win32Mutex mutex = new(Name); return mutex.Duplicate(processId);
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
        return JsonObject.Parse(@string)[Key].GetString();
    }

    async Task<string> LocalHashAsync() => await Task.Run(() =>
    {
        try
        {
            lock (_lock)
            {
                using var stream = File.OpenRead(Path);
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

        if (IsRunning) return false;
        await HttpService.DownloadAsync(Uri, Path, action);

        return true;
    }
}