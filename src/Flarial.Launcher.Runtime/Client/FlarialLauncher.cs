using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.Services;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Runtime.Client;

public static class FlarialLauncher
{
    static FlarialLauncher()
    {
        var assembly = Assembly.GetEntryAssembly();

        var temp = Path.GetTempPath();
        s_source = $"{Path.Combine(temp, Path.GetRandomFileName())}.exe";
        s_script = $"{Path.Combine(temp, Path.GetRandomFileName())}.cmd";

        s_version = assembly.GetName().Version.ToString();
        var destination = assembly.ManifestModule.FullyQualifiedName;

        var system = Environment.GetFolderPath(Environment.SpecialFolder.System);

        s_filename = Path.Combine(system, "cmd.exe");
        s_arguments = string.Format(Arguments, s_script, s_filename, destination, "{0}");
        s_content = string.Format(Content, Path.Combine(system, "taskkill.exe"), GetCurrentProcessId(), s_source, destination);
    }

    const string Content = @"chcp 65001
""{0}"" /f /pid ""{1}""
:_
move /y ""{2}"" ""{3}""
if not %errorlevel%==0 goto _
del ""%~f0""";

    const string AcceptedUrl = "https://cdn.flarial.xyz/202.txt";
    const string LauncherVersionUri = "https://cdn.flarial.xyz/launcher/launcherVersion.txt";
    const string LauncherDownloadUri = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.exe";
    const string Arguments = "/e:on /f:off /v:off /d /c call \"{0}\" & \"{1}\" /c start \"\" \"{2}\"";

    static readonly string s_filename, s_arguments, s_version, s_source, s_script, s_content;
    static readonly JsonService<Dictionary<string, string>> s_json = JsonService<Dictionary<string, string>>.Get();

    public static async Task<bool> ConnectAsync()
    {
        try
        {
            using var message = await HttpService.GetAsync(AcceptedUrl);
            return message.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public static async Task<bool> CheckAsync()
    {
        using var stream = await HttpService.GetStreamAsync(LauncherVersionUri);
        return s_version != s_json.Read(stream)["version"];
    }

    public static async Task DownloadAsync(Action<int> callback)
    {
        StringBuilder builder = new(s_arguments);

        if ((bool)Minecraft.AllowUnsignedInstalls!)
            builder.Append(' ').Append("--allow-unsigned-installs");

        await HttpService.DownloadAsync(LauncherDownloadUri, s_source, callback);

        using (StreamWriter writer = new(s_script))
            await writer.WriteAsync(s_content);

        using (Process.Start(new ProcessStartInfo
        {
            FileName = s_filename,
            CreateNoWindow = true,
            UseShellExecute = false,
            Arguments = $"{builder}"
        })) { }
    }
}