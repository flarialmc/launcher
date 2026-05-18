using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flarial.Launcher.Management;

public sealed class AppSettings
{
    const string FileName = "Flarial.Launcher.json";

    public bool AutomaticUpdates { get; set; } = true;
    public bool HardwareAcceleration { get; set; } = true;
    public bool UseCustomDll { get; set; }
    public string CustomDllPath { get; set; } = string.Empty;

    public static AppSettings Get()
    {
        try
        {
            using var stream = File.OpenRead(FileName);
            var settings = JsonSerializer.Deserialize(stream, LauncherJsonContext.Default.AppSettings) ?? new();

            try
            {
                settings.CustomDllPath = Path.GetFullPath(settings.CustomDllPath.Trim());
            }
            catch
            {
                settings.CustomDllPath = string.Empty;
            }

            return settings;
        }
        catch
        {
            return new();
        }
    }

    public void Set()
    {
        using var stream = File.Create(FileName);
        JsonSerializer.Serialize(stream, this, LauncherJsonContext.Default.AppSettings);
    }
}

[JsonSerializable(typeof(AppSettings))]
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
partial class LauncherJsonContext : JsonSerializerContext;
