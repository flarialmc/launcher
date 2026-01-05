using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;

namespace Flarial.Launcher.Management;

enum DllBuild { Release, Beta, Custom }

[DataContract]
sealed class Configuration
{
    [DataMember]
    internal DllBuild DllBuild { get; set; } = DllBuild.Release;

    [DataMember]
    internal string CustomDllPath
    {
        get => field;
        set
        {
            try { field = value.Trim(); }
            catch { field = string.Empty; }
        }
    } = string.Empty;

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