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

    bool? Launch()
    {
        if (Minecraft.GetWindow(WindowClass) is { } clientWindow)
        {
            if (Minecraft.Current.GetWindow(clientWindow.ProcessId) is { } minecraftWindow)
            {
                minecraftWindow.Switch();
                return null;
            }
            return false;
        }
        return Injector.Launch(new(FileName)) is { };
    }

    public async Task<bool?> LaunchAsync() => await Task.Run(Launch);

    const string HashesUrl = "https://cdn.flarial.xyz/dll_hashes.json";

    async Task<string> GetRemoteHashAsync()
    {
        using var stream = await HttpService.GetStreamAsync(HashesUrl);
        var json = await JsonService.Default.ReadAsync<Dictionary<string, string>>(stream);
        return json[Build];
    }

    async Task<string> GetLocalHashAsync()
    {
        try
        {
            using var stream = File.OpenRead(FileName);
            return Convert.ToHexString(await SHA256.HashDataAsync(stream));
        }
        catch { return string.Empty; }
    }

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