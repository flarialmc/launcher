using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;

namespace Flarial.Launcher.App;

enum DllBuild { Release, Beta, Custom }

[DataContract]
sealed class Configuration
{
    [DataMember]
    internal DllBuild DllBuild;

    [DataMember]
    internal string? CustomDllPath;

    [DataMember]
    internal bool HardwareAcceleration
    {
        get => field;
        set
        {
            field = value;
            RenderOptions.ProcessRenderMode = value ? RenderMode.Default : RenderMode.SoftwareOnly;
        }
    }

    [OnDeserializing]
    private void OnDeserializing(StreamingContext context)
    {
        DllBuild = DllBuild.Release;
        CustomDllPath = null;
        HardwareAcceleration = true;
    }

    internal static Configuration Get()
    {
        try
        {
            using var stream = File.OpenRead("Flarial.Launcher.xml");
            var s_configuration = (Configuration)s_serializer.ReadObject(stream);

            if (!Enum.IsDefined(typeof(DllBuild), s_configuration.DllBuild))
                s_configuration.DllBuild = default;

            return s_configuration;
        }
        catch { return new(); }
    }

    internal void Save()
    {
        using var writer = XmlWriter.Create("Flarial.Launcher.xml", s_settings);
        s_serializer.WriteObject(writer, this);
    }

    static readonly DataContractSerializer s_serializer = new(typeof(Configuration));

    static readonly XmlWriterSettings s_settings = new() { Indent = true };
}