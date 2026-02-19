using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Flarial.Launcher.Management;

internal enum DllSelection { Release, Beta, Custom }

[DataContract]
sealed class ApplicationSettings
{
    [DataMember]
    internal bool AutomaticUpdates { get; set; } = true;

    [DataMember]
    internal DllSelection DllSelection { get; set; } = DllSelection.Release;

    [DataMember]
    internal string CustomDllPath { get; set; } = string.Empty;

    [DataMember]
    internal bool WaitForInitialization { get; set; } = true;

    [OnDeserializing]
    void OnDeserializing(StreamingContext context)
    {
        AutomaticUpdates = true;
        CustomDllPath = string.Empty;
        WaitForInitialization = true;
        DllSelection = DllSelection.Release;
    }

    static readonly XmlWriterSettings s_settings = new() { Indent = true };
    static readonly DataContractSerializer s_serializer = new(typeof(ApplicationSettings));

    internal static ApplicationSettings ReadSettings()
    {
        try
        {
            using var stream = File.OpenRead("Flarial.Launcher.xml");
            var settings = (ApplicationSettings)s_serializer.ReadObject(stream);

            if (!Enum.IsDefined(typeof(DllSelection), settings.DllSelection))
                settings.DllSelection = DllSelection.Release;

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

    internal void SaveSettings()
    {
        using var writer = XmlWriter.Create("Flarial.Launcher.xml", s_settings);
        s_serializer.WriteObject(writer, this);
    }
}