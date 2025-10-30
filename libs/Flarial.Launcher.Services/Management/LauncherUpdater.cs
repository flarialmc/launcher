using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Networking;
using Windows.Data.Json;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Management;

public static class LauncherUpdater
{
    static LauncherUpdater()
    {
        var assembly = Assembly.GetEntryAssembly();

        var path = Path.GetTempPath();
        s_source = $"{Path.Combine(path, Path.GetRandomFileName())}.exe";
        s_script = $"{Path.Combine(path, Path.GetRandomFileName())}.cmd";

        s_version = assembly.GetName().Version.ToString();
        var destination = assembly.ManifestModule.FullyQualifiedName;

        path = Environment.GetFolderPath(Environment.SpecialFolder.System);

        s_filename = Path.Combine(path, "cmd.exe");
        s_arguments = string.Format(Arguments, s_script, s_filename, destination, "{0}");
        s_content = string.Format(Content, Path.Combine(path, "taskkill.exe"), GetCurrentProcessId(), s_source, destination);
    }

    const string Arguments = "/e:on /f:off /v:off /d /c call \"{0}\" & \"{1}\" /c start \"\" \"{2}\" {3}";

    const string Content = @"
chcp 65001
""{0}"" /f /pid ""{1}""

:_
move /y ""{2}"" ""{3}""
if not %errorlevel%==0 goto _

del ""%~f0""
";

    const string VersionUri = "https://cdn.flarial.xyz/launcher/launcherVersion.txt";

    const string LauncherUri = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.exe";

    static readonly string s_filename, s_arguments, s_version, s_source, s_script, s_content;

    public static async Task<bool> CheckAsync()
    {
        var input = await HttpService.GetAsync<string>(VersionUri);
        var version = JsonObject.Parse(input)["version"];
        return s_version != version.GetString();
    }

    public static async Task DownloadAsync(Action<int> action)
    {
        await HttpService.DownloadAsync(LauncherUri, s_source, action);

        using (StreamWriter writer = new(s_script))
            await writer.WriteAsync(s_content);

        using (Process.Start(new ProcessStartInfo
        {
            FileName = s_filename,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = string.Format(s_arguments, HttpService.UseProxy ? "--use-proxy" : string.Empty)
        })) { }
    }
}