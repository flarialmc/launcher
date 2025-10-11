using System;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Client;

namespace Flarial.Launcher.SDK;

public static partial class Client
{
    public static async partial Task DownloadAsync(bool beta, Action<int> action)
    {
        var client = beta ? FlarialClient.Beta : FlarialClient.Release;
        await client.DownloadAsync(action);
    }

    public static partial async Task<bool> LaunchAsync(bool beta, bool initialized) => await Task.Run(() =>
    {
        var client = beta ? FlarialClient.Beta : FlarialClient.Release;
        return client.LaunchGame(initialized);
    });
}