using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;
using static System.StringComparison;

namespace Flarial.Runtime.Core;

public static partial class FlarialClient
{
    const string Build = "Release";
    const string ClassName = "Flarial Client";
    const string FileName = "Flarial.Client.Release.dll";
    const string DownloadUri = "https://cdn.flarial.xyz/dll/latest.dll";

    static bool? Activate()
    {
        if (Minecraft.GetWindow(className: ClassName) is { } clientWindow)
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
        var json = await HttpService.GetJsonAsync<Dictionary<string, string>>(HashesUrl);
        return json[Build];
    }

    async static Task<string> GetLocalHashAsync()
    {
        try
        {
            using var stream = File.OpenRead(FileName);
            var array = await SHA256.HashDataAsync(stream);
            return Convert.ToHexString(array);
        }
        catch { return string.Empty; }
    }

    public static async Task<bool> DownloadAsync(Action<int> callback)
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