using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flarial.Launcher.Management;

[JsonSerializable(typeof(AppSettings))]
sealed partial class AppSettingsContext : JsonSerializerContext;

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

    [OnDeserializing, Obsolete]
    void OnDeserializing(StreamingContext context)
    {
        UseCustomDll = false;
        AutomaticUpdates = true;
        CustomDllPath = string.Empty;
    }

    internal static AppSettings Get()
    {
        try
        {
            using var stream = File.OpenRead("Flarial.Launcher.json");
            return JsonSerializer.Deserialize(stream, AppSettingsContext.Default.AppSettings)!;
        }
        catch { return new(); }
    }

    internal void Set()
    {
        using var stream = File.Create("Flarial.Launcher.json");
        JsonSerializer.Serialize(stream, this, AppSettingsContext.Default.AppSettings);
    }
}