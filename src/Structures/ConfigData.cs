namespace Flarial.Launcher.Structures;

public sealed class ConfigData
{
    public int dllBuild = 0;
    public string custom_dll_path = null;
    public bool shouldUseCustomDLL = false;
    public bool autoInject = false;
    public bool minimizeToTray = false;
    public bool startMinimized = false;
    public bool mcMinimized = true;
    public bool autoLogin = true;
    public bool hardwareAcceleration = true;
}
