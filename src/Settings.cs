using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Devices.Midi;
using Windows.UI.Xaml.Controls.Maps;

namespace Flarial.Launcher;

internal enum DllBuild { Stable, Beta, Nightly, Custom }

[DataContract]
sealed partial class Settings
{
    bool _hardwareAcceleration = true;

    [DataMember]
    internal bool HardwareAcceleration
    {
        get => _hardwareAcceleration;
        set
        {
            _hardwareAcceleration = value;
            RenderOptions.ProcessRenderMode = value ? RenderMode.Default : RenderMode.SoftwareOnly;
        }
    }


    bool _fixMinecraftMinimizing = true;

    [DataMember]
    internal bool FixMinecraftMinimizing
    {
        get => _fixMinecraftMinimizing;
        set
        {
            _fixMinecraftMinimizing = value;
            if (SDK.Minecraft.Installed)
                SDK.Minecraft.Debug = value;
        }
    }


    [DataMember]
    internal bool AutoInject = false;

    [DataMember]
    internal string CustomDllPath = null;

    [DataMember]
    internal DllBuild DllBuild = DllBuild.Stable;

    [DataMember]
    internal bool WaitForResources = true;

    internal bool AutoLogin = true;

    internal bool StartMinimized = false;

    internal bool MinimizeToTray = false;
}

partial class Settings
{
    [OnDeserializing]
    private void Defaults(StreamingContext context)
    {
        CustomDllPath = null;
        DllBuild = DllBuild.Stable;

        WaitForResources = true;
        FixMinecraftMinimizing = true;

        AutoInject = false;
        HardwareAcceleration = true;

        AutoLogin = true;
        StartMinimized = false;
        MinimizeToTray = false;
    }
}

sealed partial class Settings
{
    static readonly object _lock = new();

    static Settings _current;

    internal static Settings Current
    {
        get
        {
            if (_current is not null)
                return _current;

            lock (_lock)
            {
                try
                {
                    using var stream = File.OpenRead("Flarial.Launcher.Settings.json");
                    _current = (Settings)_serializer.ReadObject(stream);

                    var build = _current.DllBuild;
                    if (!Enum.IsDefined(typeof(DllBuild), build))
                        _current.DllBuild = DllBuild.Stable;
                }
                catch { _current = new(); }

                return _current;
            }
        }
    }

    static readonly DataContractJsonSerializer _serializer;

    static Settings()
    {
        DataContractJsonSerializerSettings settings = new()
        {
            UseSimpleDictionaryFormat = true
        };
        _serializer = new(typeof(Settings), settings);
    }
}

sealed partial class Settings
{
    internal static void Save()
    {
        using var stream = File.Create("Flarial.Launcher.Settings.json");
        _serializer.WriteObject(stream, Current);
    }
}