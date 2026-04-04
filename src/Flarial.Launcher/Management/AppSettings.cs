using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Flarial.Launcher.Management;

[DataContract]
sealed class AppSettings
{
    [DataMember]
    internal bool AutomaticUpdates { get; set; } = true;

    [DataMember]
    internal bool DownloadVersions { get; set; } = false;

    [DataMember]
    internal bool WaitForInitialization { get; set; } = true;

    [DataMember]
    internal bool UseCustomDll { get; set; } = false;

    [DataMember]
    internal string CustomDllPath { get; set; } = string.Empty;

    [OnDeserializing]
    void OnDeserializing(StreamingContext context)
    {
        UseCustomDll = false;
        AutomaticUpdates = true;
        DownloadVersions = false;
        CustomDllPath = string.Empty;
        WaitForInitialization = true;
    }

    static readonly XmlWriterSettings s_settings = new() { Indent = true };

    static readonly DataContractSerializer s_serializer = new(typeof(AppSettings));

    internal static AppSettings Get()
    {
        try
        {
            using var stream = File.OpenRead("Flarial.Launcher.xml");
            var settings = (AppSettings)s_serializer.ReadObject(stream);

            try
            {
                var path = settings.CustomDllPath.Trim();
                settings.CustomDllPath = Path.GetFullPath(path);
            }
            catch { settings.CustomDllPath = string.Empty; }

            return settings;
        }
        catch { return new(); }
    }

    internal void Flush()
    {
        using var writer = XmlWriter.Create("Flarial.Launcher.xml", s_settings);
        s_serializer.WriteObject(writer, this);
    }
}