namespace Flarial.Launcher.Structures;

public struct ConfigData
{
    public ConfigData() { }
    public string custom_dll_path;
    public bool shouldUseCustomDLL;
    public bool shouldUseBetaDll;
    public bool mcMinimized = true;
    public bool autoLogin = true;
    public bool hardwareAcceleration = true;
}
