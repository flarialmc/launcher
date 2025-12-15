using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Flarial.Launcher.Services.Core;
using Windows.Devices.Midi;
using Windows.UI.Xaml.Controls.Maps;

namespace Flarial.Launcher;

internal enum DllBuild { Release, Beta, Nightly, Custom }

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

    [DataMember]
    internal string CustomDllPath = null;

    [DataMember]
    internal DllBuild DllBuild = DllBuild.Release;

    [DataMember]
    internal bool WaitForInitialization = true;

    [DataMember]
    internal bool BypassPCBootstrapper
    {
        get => Minecraft.BypassPCBootstrapper;
        set => Minecraft.BypassPCBootstrapper = value;
    }

    internal bool AutoLogin = true;
}

partial class Settings
{
    [OnDeserializing]
    private void OnDeserializing(StreamingContext context)
    {
        CustomDllPath = null;
        DllBuild = DllBuild.Release;
        WaitForInitialization = true;

        AutoLogin = true;
        BypassPCBootstrapper = false;
        HardwareAcceleration = true;
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
                        _current.DllBuild = DllBuild.Release;
                }
                catch { _current = new(); }

                return _current;
            }
        }
    }

    static readonly DataContractJsonSerializer _serializer;

    static Settings() => _serializer = new(typeof(Settings), new DataContractJsonSerializerSettings
    {
        UseSimpleDictionaryFormat = true
    });
}

sealed partial class Settings
{
    internal void Save()
    {
        using var stream = File.Create("Flarial.Launcher.Settings.json");
        _serializer.WriteObject(stream, this);
    }
}