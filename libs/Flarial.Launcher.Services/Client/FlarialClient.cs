using System;
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
    protected abstract string Identifer { get; }
    protected abstract string Library { get; }
    protected abstract string Build { get; }
    protected abstract string Uri { get; }
    internal FlarialClient() { }

    public static readonly FlarialClient Beta = new FlarialClientBeta(), Release = new FlarialClientRelease();
}

partial class FlarialClient
{
    static FlarialClient? Client
    {
        get
        {
            using Win32Mutex beta = new(Beta.Identifer);
            using Win32Mutex release = new(Release.Identifer);

            if (!Minecraft.Current.IsRunning || (beta.Exists && release.Exists)) return null;
            if (beta.Exists) return Beta; if (release.Exists) return Release;

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

        if (Injector.Launch(initialized, Library) is not { } processId) return false;
        using Win32Mutex mutex = new(Identifer); return mutex.Duplicate(processId);
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
        return JsonObject.Parse(@string)[Build].GetString();
    }

    async Task<string> LocalHashAsync() => await Task.Run(() =>
    {
        try
        {
            lock (_lock)
            {
                using var stream = File.OpenRead(Library);
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

        try { File.Delete(Library); } catch { return false; }
        await HttpService.DownloadAsync(Uri, Library, action);

        return true;
    }
}