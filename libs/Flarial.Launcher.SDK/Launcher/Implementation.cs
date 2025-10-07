using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Flarial.Launcher.SDK;

public static partial class Launcher
{
    static Launcher()
    {
        var assembly = Assembly.GetEntryAssembly();
        Version = assembly.GetName().Version;
        Destination = assembly.ManifestModule.FullyQualifiedName;
    }

    static readonly Version Version;

    static readonly string Destination;

    static readonly int Identifier = Native.GetCurrentProcessId();

    static readonly string Temp = Path.GetTempPath();

    static readonly string System = Environment.GetFolderPath(Environment.SpecialFolder.System);

    static readonly string Content = $"chcp 65001\n\"{Path.Combine(System, "taskkill.exe")}\" /f /pid {{0}}\n:_\ncopy /y \"{{1}}\" \"{{2}}\"\nif not %errorlevel%==0 goto _\ndel \"%~f0\"";

    static readonly string File = Path.Combine(System, "cmd.exe");

    public static partial async Task<bool> AvailableAsync() => new Version((await Web.LauncherAsync())["version"].GetString()) != Version;

    public static async partial Task<bool> UpdateAsync(Action<int> action)
    {
        var @this = await Web.LauncherAsync();

        var source = Path.Combine(Temp, Path.GetRandomFileName());
        var path = Path.Combine(Temp, Path.ChangeExtension(Path.GetRandomFileName(), ".cmd"));

        await Web.DownloadAsync(@this["downloadUrl"].GetString(), source, action);

        using StreamWriter stream = new(path);
        await stream.WriteAsync(string.Format(Content, Identifier, source, Destination));

        using (Process.Start(new ProcessStartInfo
        {
            FileName = File,
            Arguments = $"/e:on /c call \"{path}\" & \"{File}\" /c start \"\" \"{Destination}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        })) { }

        return true;
    }
}