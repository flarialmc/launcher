using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;
using static System.StringComparison;

namespace Flarial.Runtime.Core;

public abstract class FlarialClient<T> : FlarialClient where T : FlarialClient<T>, new()
{
    private protected FlarialClient()
    {
        if (_ is null) return;
        throw new InvalidOperationException();
    }

    public static readonly T _ = new();
}

public abstract partial class FlarialClient
{
    private protected abstract string Build { get; }
    private protected abstract string FileName { get; }
    private protected abstract string DownloadUri { get; }

    private protected FlarialClient() { }

    public static bool IsRunning
    {
        get
        {
            if (Minecraft.GetWindow(className: ClassName) is not { } clientWindow)
                return false;

            if (Minecraft.GetWindow(clientWindow._processId) is not { } minecraftWindow)
                return false;

            return minecraftWindow.IsVisible;
        }
    }

    public bool Launch()
    {
        if (!IsRunning && Injector.Launch(new(FileName)))
        {
            _ = PostAnalyticsAsync();
            return true;
        }
        return false;
    }

    const string ClassName = "Flarial Client";
    const string HashesUri = "https://cdn.flarial.xyz/dll_hashes.json";

    async Task<string> GetRemoteHashAsync()
    {
        var json = await HttpService.GetJsonAsync<Dictionary<string, string>>(HashesUri);
        return json[Build];
    }

    async Task<string> GetLocalHashAsync()
    {
        try
        {
            using var stream = File.OpenRead(FileName);
            var array = await SHA256.HashDataAsync(stream);
            return Convert.ToHexString(array);
        }
        catch { return string.Empty; }
    }

    public async Task<bool> DownloadAsync(Action<int> callback)
    {
        var localHashTask = GetLocalHashAsync();
        var remoteHashTask = GetRemoteHashAsync();
        await Task.WhenAll(localHashTask, remoteHashTask);

        var localHash = await localHashTask;
        var remoteHash = await remoteHashTask;

        if (localHash.Equals(remoteHash, OrdinalIgnoreCase))
            return true;

        try { File.Delete(FileName); }
        catch { return false; }

        await HttpService.DownloadAsync(DownloadUri, FileName, callback);
        return true;
    }
}