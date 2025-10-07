using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Bedrockix.Minecraft;
using Flarial.Launcher.Services.Modding;
using Flarial.Launcher.Services.Core;
using Windows.System;

namespace Flarial.Launcher.SDK;

static class Release
{
    internal const string Path = "Flarial.Client.Release.dll";

    internal const string Uri = "https://raw.githubusercontent.com/flarialmc/newcdn/main/dll/latest.dll";

    const string Mutex = "34F45015-6EB6-4213-ABEF-F2967818E628";

    internal static bool Exists => Instance.Exists(Mutex);

    internal static bool Launch(LaunchType type)
    {
        if (Beta.Exists) Game.Terminate();
        if (Exists) return Game.Launch(false).HasValue;

        var _ = Injector.UWP.Launch(type, Path);
        if (_.HasValue) Instance.Create((int)_.Value, Mutex);
        return _.HasValue;
    }
}

static class Beta
{
    internal const string Path = "Flarial.Client.Beta.dll";

    internal const string Uri = "https://raw.githubusercontent.com/flarialmc/newcdn/main/dll/beta.dll";

    const string Mutex = "6E41334A-423F-4A4F-9F41-5C440C9CCBDC";

    internal static bool Exists => Instance.Exists(Mutex);

    internal static bool Launch(LaunchType type)
    {
        if (Release.Exists) Game.Terminate();
        if (Exists) return Game.Launch(false).HasValue;

        var _ = Injector.UWP.Launch(type, Path);
        if (_.HasValue) Instance.Create((int)_.Value, Mutex);
        return _.HasValue;
    }
}

public static partial class Client
{
    static readonly HashAlgorithm Algorithm = SHA256.Create();

    static readonly object Lock = new();

    static async Task<bool> VerifyAsync(string path, bool value = false) => await Task.Run(async () =>
    {
        if (!File.Exists(path)) return false;
        using var stream = File.OpenRead(path); var hash = await Web.HashAsync(value);
        lock (Lock) return hash.Equals(BitConverter.ToString(Algorithm.ComputeHash(stream)).Replace("-", string.Empty), StringComparison.OrdinalIgnoreCase);
    });

    public static async partial Task DownloadAsync(bool value, Action<int> action) => await Task.Run(async () =>
    {
        var path = value ? Beta.Path : Release.Path;
        var uri = value ? Beta.Uri : Release.Uri;

        if (!await VerifyAsync(path, value))
        {
            if (value ? Beta.Exists : Release.Exists) Game.Terminate();
            await Web.DownloadAsync(uri, path, action);
        }
    });

    [Obsolete("Use 'Flarial.Launcher.Services'.", true)]
    public static async partial Task<bool> LaunchAsync(bool value) => await LaunchAsync(value, false);

    public static async partial Task<bool> LaunchAsync(bool useBeta, bool waitForResources) => await Task.Run(delegate
    {
        var type = waitForResources ? LaunchType.ResourceInit : LaunchType.MenuLoad;
        if (useBeta) return Beta.Launch(type);
        return Release.Launch(type);
    });
}