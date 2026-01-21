using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;

namespace Flarial.Launcher.Management;

enum DllBuild { Release, Beta, Custom }

[DataContract]
sealed class ApplicationConfiguration
{
    [DataMember]
    internal DllBuild DllBuild { get; set; } = DllBuild.Release;

    [DataMember]
    internal string CustomDllPath { get; set; } = string.Empty;

    [DataMember]
    internal bool WaitForInitialization { get; set; } = true;

    [DataMember]
    internal bool HardwareAcceleration
    {
        get => field;
        set
        {
            field = value;
            RenderOptions.ProcessRenderMode = value ? RenderMode.Default : RenderMode.SoftwareOnly;
        }
    } = true;

    [OnDeserializing]
    void OnDeserializing(StreamingContext context)
    {
        HardwareAcceleration = true;
        DllBuild = DllBuild.Release;
        CustomDllPath = string.Empty;
        WaitForInitialization = true;
    }

    internal static ApplicationConfiguration Get()
    {
        try
        {
            using var stream = File.OpenRead("Flarial.Launcher.xml");
            var configuration = (ApplicationConfiguration)s_serializer.ReadObject(stream);

            if (!Enum.IsDefined(typeof(DllBuild), configuration.DllBuild))
                configuration.DllBuild = DllBuild.Release;

            try
            {
                var path = configuration.CustomDllPath.Trim();
                configuration.CustomDllPath = Path.GetFullPath(path);
            }
            catch { configuration.CustomDllPath = string.Empty; }

            return configuration;
        }
        catch { return new(); }
    }

    internal void Save()
    {
        using var writer = XmlWriter.Create("Flarial.Launcher.xml", s_settings);
        s_serializer.WriteObject(writer, this);
    }

    static readonly DataContractSerializer s_serializer = new(typeof(ApplicationConfiguration));

    static readonly XmlWriterSettings s_settings = new() { Indent = true };
}