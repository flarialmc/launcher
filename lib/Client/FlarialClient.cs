using System;
using System.IO;
using static System.StringComparison;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Modding;
using Flarial.Launcher.Services.Networking;
using Windows.Data.Json;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.System;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Client;

public abstract class FlarialClient
{
    internal FlarialClient() { }
    protected abstract string Uri { get; }
    protected abstract string Build { get; }
    protected abstract string Path { get; }
    protected abstract string Identifer { get; }

    public static readonly FlarialClient Beta = new FlarialClientBeta(), Release = new FlarialClientRelease();

    static FlarialClient? Client
    {
        get
        {
            using NativeMutex beta = new(Beta.Identifer), release = new(Release.Identifer);
            if (!Minecraft.Current.IsRunning || (beta.Exists && release.Exists)) return null;
            if (beta.Exists) return Beta; if (release.Exists) return Release; return null;
        }
    }

    public bool Launch(bool initialized)
    {
        if (Client is { } client)
        {
            if (!ReferenceEquals(this, client)) return false;
            return Minecraft.Current.Launch(false) is { };
        }

        if (Injector.Launch(initialized, new(Path)) is not { } processId) return false;
        using NativeMutex mutex = new(Identifer); return mutex.Duplicate(processId);
    }

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

        if ((await tasks[0]).Equals(await tasks[1], OrdinalIgnoreCase))
            return true;

        unsafe
        {
            fixed (char* value = Path)
                if (!DeleteFile(value))
                    return false;
        }

        await HttpService.DownloadAsync(Uri, Path, action);
        return true;
    }
}