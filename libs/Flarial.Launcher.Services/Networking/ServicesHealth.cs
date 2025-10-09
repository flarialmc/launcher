using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flarial.Launcher.Services.Networking;

public enum FailedService
{
    None,
    GameVersions,
    ClientHashes,
    GameFrameworks,
    LauncherVersion,
    MicrosoftStore,
    SupportedVersions
}

public static class ServicesHealth
{
    public static async Task<FailedService> CheckAsync()
    {
        var gameVersions = HttpService.CheckAsync(ServicesEndpoints.GameVersions);
        var clientHashes = HttpService.CheckAsync(ServicesEndpoints.ClientHashes);
        var gameFrameworks = HttpService.CheckAsync(ServicesEndpoints.GameFrameworks);
        var launcherVersion = HttpService.CheckAsync(ServicesEndpoints.LauncherVersion);
        var microsoftStore = HttpService.CheckAsync(ServicesEndpoints.MicrosoftStore);
        var supportedVersions = HttpService.CheckAsync(ServicesEndpoints.SupportedVersions);

        List<Task> tasks = [];
        tasks.Add(gameVersions);
        tasks.Add(clientHashes);
        tasks.Add(gameFrameworks);
        tasks.Add(launcherVersion);
        tasks.Add(microsoftStore);
        tasks.Add(supportedVersions);
        await Task.WhenAll(tasks);

        if (!await launcherVersion)
            return FailedService.LauncherVersion;

        if (!await supportedVersions)
            return FailedService.SupportedVersions;

        if (!await clientHashes)
            return FailedService.ClientHashes;

        if (!await gameVersions)
            return FailedService.GameVersions;

        if (!await gameFrameworks)
            return FailedService.GameFrameworks;

        if (!await microsoftStore)
            return FailedService.MicrosoftStore;

        return FailedService.None;
    }
}