using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
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

    const string AcceptedUri = "https://cdn.flarial.xyz/202.txt";
    const string LauncherVersionUri = "https://cdn.flarial.xyz/launcher/launcherVersion.txt";
    const string LauncherDownloadUri = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.exe";
    const string Arguments = "/e:on /f:off /v:off /d /c call \"{0}\" & \"{1}\" /c start \"\" \"{2}\"";

    static readonly string s_source;
    static readonly string s_script;
    static readonly string s_content;
    static readonly string s_version;
    static readonly string s_filename;
    static readonly string s_arguments;

    public static async Task<bool> VerifyConnectionAsync()
    {
        try
        {
            using var response = await HttpService.GetAsync(AcceptedUri);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public static async Task<bool> CheckForUpdatesAsync()
    {
        using var stream = await HttpService.GetStreamAsync(LauncherVersionUri);
        var json = await JsonService.ReadAsync<Dictionary<string, string>>(stream);
        return s_version != json["version"];
    }

    public static async Task DownloadAsync(Action<int> callback)
    {
        StringBuilder builder = new(s_arguments);

        await HttpService.DownloadAsync(LauncherDownloadUri, s_source, callback);
        using (StreamWriter writer = new(s_script)) await writer.WriteAsync(s_content);

        using (Process.Start(new ProcessStartInfo
        {
            FileName = s_filename,
            CreateNoWindow = true,
            UseShellExecute = false,
            Arguments = $"{builder}"
        })) { }

        Environment.Exit(0);
    }
}