using System;
using System.Collections.Generic;
using System.IO;
using static System.StringComparison;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Modding;
using Flarial.Launcher.Services.Networking;
using Flarial.Launcher.Services.System;
using Windows.Data.Json;

namespace Flarial.Launcher.Services.Client;

public abstract partial class FlarialClient
{
    static readonly Injector s_injector = Injector.UWP;

    readonly string _name, _path, _key, _uri;

    internal FlarialClient(string name, string path, string key, string uri)
    {
        _key = key;
        _uri = uri;
        _name = name;
        _path = path;
    }
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
    internal bool IsInjectable
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

    public bool IsRunning
    {
        get
        {
            if (!s_injector._minecraft.IsRunning)
                return false;

            using Win32Mutex mutex = new(_name);
            return mutex.Exists;
        }
    }
}

partial class FlarialClient
{
    public bool LaunchGame(bool initialized)
    {
        var minecraft = s_injector._minecraft;
        
        if (!IsInjectable) minecraft.TerminateGame();
        if (IsRunning) return minecraft.LaunchGame(initialized) is not null;
        if (s_injector.LaunchGame(initialized, _path) is not { } processId) return false;

        using Win32Process process = new(processId);
        using Win32Mutex mutex = new(_name);

        mutex.Duplicate(process); return true;
    }
}

partial class FlarialClient
{
    static readonly object _lock = new();

    static readonly HashAlgorithm _algorithm = SHA256.Create();

    const string HashesUri = "https://cdn.flarial.xyz/dll_hashes.json";

    async Task<string> RemoteHashAsync()
    {
        var @string = await HttpService.StringAsync(HashesUri);
        return JsonObject.Parse(@string)[_key].GetString();
    }

    async Task<string> LocalHashAsync() => await Task.Run(() =>
    {
        try
        {
            lock (_lock)
            {
                using var stream = File.OpenRead(_path);
                var value = _algorithm.ComputeHash(stream);
                var @string = BitConverter.ToString(value);
                return @string.Replace("-", string.Empty);
            }
        }
        catch { return string.Empty; }
    });

    public async Task DownloadAsync(Action<int> action)
    {
        Task<string>[] tasks = [LocalHashAsync(), RemoteHashAsync()]; await Task.WhenAll(tasks);
        if ((await tasks[0]).Equals(await tasks[1], OrdinalIgnoreCase)) return;

        if (IsRunning) s_injector._minecraft.TerminateGame();
        await HttpService.DownloadAsync(_uri, _path, action);
    }
}