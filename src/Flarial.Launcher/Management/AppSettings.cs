using System;
using System.IO;
using Flarial.Runtime.Services;

namespace Flarial.Launcher.Management;

public sealed class AppSettings
{
    public bool AutomaticUpdates { get; set; } = true;

    public bool PerformanceMode { get; set; } = false;

    public bool UseCustomDll { get; set; } = false;

    public string? CustomDllPath
    {
        get;
        set
        {
            if (value is null) return;
            field = Path.GetFullPath(value);
        }
    }

    internal static AppSettings Get()
    {
        try
        {
            using var stream = File.OpenRead("Flarial.Launcher.json");
            return JsonService.Default.Read<AppSettings>(stream);
        }
        catch { return new(); }
    }

    internal void Set()
    {
        using var stream = File.Create("Flarial.Launcher.json");
        JsonService.Default.Write(stream, this);
    }
}