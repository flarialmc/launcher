using System;
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
    internal bool UseCustomDll { get; set; } = false;

    [DataMember]
    internal string CustomDllPath { get; set; } = string.Empty;

    [DataMember]
    internal string AnalyticsInstallId { get; set; } = string.Empty;

    [OnDeserializing]
    void OnDeserializing(StreamingContext context)
    {
        UseCustomDll = false;
        AutomaticUpdates = true;
        CustomDllPath = string.Empty;
        AnalyticsInstallId = string.Empty;
    }

    static readonly XmlWriterSettings s_settings = new() { Indent = true };
    static readonly DataContractSerializer s_serializer = new(typeof(AppSettings));

    internal static AppSettings Get()
    {
        AppSettings settings;
        try
        {
            using var stream = File.OpenRead("Flarial.Launcher.xml");
            settings = (AppSettings)s_serializer.ReadObject(stream);

            try
            {
                var path = settings.CustomDllPath.Trim();
                settings.CustomDllPath = Path.GetFullPath(path);
            }
            catch { settings.CustomDllPath = string.Empty; }
        }
        catch { settings = new(); }

        if (!Guid.TryParse(settings.AnalyticsInstallId, out _))
        {
            settings.AnalyticsInstallId = Guid.NewGuid().ToString("N");
            try { settings.Set(); } catch { }
        }

        return settings;
    }

    internal void Set()
    {
        using var writer = XmlWriter.Create("Flarial.Launcher.xml", s_settings);
        s_serializer.WriteObject(writer, this);
    }
}