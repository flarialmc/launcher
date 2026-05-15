using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Flarial.Launcher.Management;

[DataContract]
public sealed class AppSettings
{
    [DataMember]
    public bool AutomaticUpdates { get; set; } = true;

    [DataMember]
    public bool HardwareAcceleration { get; set; } = true;

    [DataMember]
    public bool UseCustomDll { get; set; }

    [DataMember]
    public string CustomDllPath { get; set; } = string.Empty;

    [OnDeserializing]
    void OnDeserializing(StreamingContext context)
    {
        AutomaticUpdates = true;
        HardwareAcceleration = true;
        UseCustomDll = false;
        CustomDllPath = string.Empty;
    }

    static readonly XmlWriterSettings s_settings = new() { Indent = true };
    static readonly DataContractSerializer s_serializer = new(typeof(AppSettings));

    public static AppSettings Get()
    {
        try
        {
            using var stream = File.OpenRead("Flarial.Launcher.xml");
            var settings = (AppSettings)s_serializer.ReadObject(stream)!;

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
        using var writer = XmlWriter.Create("Flarial.Launcher.xml", s_settings);
        s_serializer.WriteObject(writer, this);
    }
}
