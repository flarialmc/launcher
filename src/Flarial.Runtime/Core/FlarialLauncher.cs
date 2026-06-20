using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Core;

public static class FlarialLauncher
{
    static FlarialLauncher()
    {
        var assembly = Assembly.GetEntryAssembly()!;

        var temp = Path.GetTempPath();
        var destination = Environment.ProcessPath;
        var system = Environment.GetFolderPath(Environment.SpecialFolder.System);

        s_source = $"{Path.Combine(temp, Path.GetRandomFileName())}.exe";
        s_script = $"{Path.Combine(temp, Path.GetRandomFileName())}.cmd";

        s_version = $"{assembly.GetName().Version}";
        s_filename = Path.Combine(system, "cmd.exe");

        s_content = string.Format(Format, s_source, destination);
        s_arguments = string.Format(Arguments, s_script, s_filename, destination, "{0}");
    }

    const string Format = @"chcp 65001
:_
move /y ""{0}"" ""{1}""
if not %errorlevel%==0 goto _
del ""%~f0""";

    const string FlarialAcceptedUri = "https://cdn.flarial.xyz/202.txt";
    const string ExternalAcceptedUri = "https://cdn.jsdelivr.net/gh/flarialmc/newcdn@refs/heads/main/202.txt";

    const string LauncherVersionUri = "https://cdn.flarial.xyz/launcher/launcherVersion.txt";
    const string LauncherDownloadUri = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.exe";
    const string Arguments = "/e:on /f:off /v:off /d /c call \"{0}\" & \"{1}\" /c start \"\" \"{2}\"";

    static readonly string s_source;
    static readonly string s_script;
    static readonly string s_content;
    static readonly string s_version;
    static readonly string s_filename;
    static readonly string s_arguments;

    public static async Task<bool> PingFlarialServicesAsync() => await HttpService.PingAsync(FlarialAcceptedUri) is { };

    public static async Task<bool> PingExternalServicesAsync() => await HttpService.PingAsync(ExternalAcceptedUri) is { };

    public static async Task<bool> CheckForUpdatesAsync() => s_version != (await HttpService.GetJsonAsync<Dictionary<string, string>>(LauncherVersionUri))["version"];

    public static async Task DownloadAsync(Action<int> callback)
    {
        await HttpService.DownloadAsync(LauncherDownloadUri, s_source, callback);
        await File.WriteAllTextAsync(s_script, s_content);

        using (Process.Start(new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = s_filename,
            Arguments = s_arguments
        })) { }

        Environment.Exit(0);
    }
}