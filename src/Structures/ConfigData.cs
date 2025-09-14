using System;

namespace Flarial.Launcher.Structures;

[Obsolete("Use Flarial.Launcher.Settings instead.", true)]
public sealed class ConfigData
{
    public DllSelection dllBuild = DllSelection.Beta;
    public string custom_dll_path = null;
    public bool autoInject = false;
    public bool minimizeToTray = false;
    public bool startMinimized = false;
    public bool mcMinimized = true;
    public bool autoLogin = true;
    public bool hardwareAcceleration = true;
}
