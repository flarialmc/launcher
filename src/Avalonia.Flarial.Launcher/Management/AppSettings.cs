using System.IO;
using System.Runtime.Serialization;
using Flarial.Runtime.Services;

namespace Flarial.Launcher.Management;

[DataContract]
public sealed class AppSettings
{
    [DataMember]
    public bool AutomaticUpdates { get; set; } = true;

    [DataMember]
    public bool UseCustomDll { get; set; } = false;

    [DataMember]
    public string CustomDllPath
    {
        get;
        set
        {
            try { field = Path.GetFullPath(value.Trim()); }
            catch { field = string.Empty; }
        }
    } = string.Empty;

    [OnDeserializing]
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
            return JsonSerializer.Deserialize<AppSettings>(stream);
        }
        catch { return new(); }
    }

    internal void Set()
    {
        using var stream = File.OpenWrite("Flarial.Launcher.json");
        JsonSerializer.Serialize(stream, this);
    }
}