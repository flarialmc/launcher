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

    static readonly object s_lock = new();
    static readonly HashAlgorithm s_algorithm = SHA256.Create();

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

    async static Task<string> GetRemoteHashAsync() => (await HttpService.GetJsonAsync<Dictionary<string, string>>(HashesUrl))[Build];

    async static Task<string> GetLocalHashAsync()
    {
        try
        {
            lock (s_lock)
            {
                using var stream = File.OpenRead(FileName);
                var value = s_algorithm.ComputeHash(stream);
                return BitConverter.ToString(value).Replace("-", string.Empty);
            }
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