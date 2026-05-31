using System.IO;
using System.Runtime.Serialization;
using Flarial.Runtime.Services;

namespace Flarial.Launcher.Management;

[DataContract]
sealed class AppSettings
{
    [DataMember]
    internal bool AutomaticUpdates { get; set; } = true;

    [DataMember]
    internal bool UseCustomDll { get; set; } = false;

    [DataMember]
    internal string CustomDllPath
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
            return JsonService.Read<AppSettings>(stream);
        }
        catch { return new(); }
    }

    internal void Set()
    {
        using var stream = File.Create("Flarial.Launcher.json");
        JsonService.Write(stream, this);
    }
}