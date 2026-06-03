using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Core;

public static class FlarialClient
{
    const string Build = "Release";
    const string ClassName = "Flarial Client";
    const string FileName = "Flarial.Client.Release.dll";
    const string DownloadUri = "https://cdn.flarial.xyz/dll/latest.dll";

    public static bool? Launch()
    {
        if (Minecraft.GetWindow(ClassName) is { } clientWindow)
        {
            if (Minecraft.GetWindow(clientWindow._processId) is { } minecraftWindow)
            {
                minecraftWindow.Switch();
                return null;
            }
            return false;
        }
        return Injector.Launch(new(FileName)) is { };
    }

    const string HashesUrl = "https://cdn.flarial.xyz/dll_hashes.json";

    async static Task<string> GetRemoteHashAsync()
    {
        using var stream = await HttpService.GetStreamAsync(HashesUrl);
        var json = await JsonService.Default.ReadAsync<Dictionary<string, string>>(stream);
        return json[Build];
    }

    async static Task<string> GetLocalHashAsync()
    {
        try
        {
            using var stream = File.OpenRead(FileName);
            return Convert.ToHexString(await SHA256.HashDataAsync(stream));
        }
        catch { return string.Empty; }
    }

    public static async Task<bool> DownloadAsync(Action<int> callback)
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