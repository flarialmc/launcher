using System;
using System.IO;
using System.Runtime.Serialization;
using Flarial.Runtime.Services;

namespace Flarial.Launcher.Management;

public sealed class AppSettings
{
    public bool AutomaticUpdates { get; set; } = true;

    public bool UseCustomDll { get; set; } = false;

    public string CustomDllPath
    {
        get;
        set
        {
            try { field = Path.GetFullPath(value.Trim()); }
            catch { field = string.Empty; }
        }
    } = string.Empty;

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