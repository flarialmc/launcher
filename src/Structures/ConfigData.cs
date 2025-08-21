namespace Flarial.Launcher.Structures;

public sealed class ConfigData
{
    public string custom_dll_path;
    public bool shouldUseCustomDLL;
    public bool shouldUseBetaDll;
    public bool autoInject;
    public bool mcMinimized = true;
    public bool autoLogin = true;
    public bool hardwareAcceleration = true;
}
