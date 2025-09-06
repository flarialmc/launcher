namespace Flarial.Launcher.Structures;

public sealed class ConfigData
{
    public string custom_dll_path = null;
    public bool shouldUseCustomDLL = false;
    public bool shouldUseBetaDll = true;
    public bool autoInject = false;
    public bool minimizeToTray = false;
    public bool mcMinimized = true;
    public bool autoLogin = true;
    public bool hardwareAcceleration = true;
}
