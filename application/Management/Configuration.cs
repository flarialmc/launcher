using System;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Windows.Interop;
using System.Windows.Media;

namespace Flarial.Launcher.Management;

[DataContract]
sealed class Configuration
{
    internal enum Build { Release, Beta, Custom }

    [DataMember]
    internal bool AutomaticUpdates { get; set; } = true;

    [DataMember]
    internal Build DllBuild { get; set; } = Build.Release;

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
        AutomaticUpdates = true;
        DllBuild = Build.Release;
        HardwareAcceleration = true;
        CustomDllPath = string.Empty;
        WaitForInitialization = true;
    }

    internal static Configuration Get()
    {
        try
        {
            using var stream = File.OpenRead("Flarial.Launcher.xml");
            var configuration = (Configuration)s_serializer.ReadObject(stream);

            if (!Enum.IsDefined(typeof(Build), configuration.DllBuild))
                configuration.DllBuild = Build.Release;

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

    static readonly DataContractSerializer s_serializer = new(typeof(Configuration));

    static readonly XmlWriterSettings s_settings = new() { Indent = true };
}