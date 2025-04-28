namespace Flarial.Launcher.Structures;

public struct ConfigData
{
    public ConfigData() { }

    public string customDllPath;
    public bool customDll;
    public bool betaDll;
    public bool autoLogin = true;
    public bool fixMinimizing = true;
    public bool rpc = true;
    public bool welcomeMessage = true;
    public bool backgroundParallaxEffect = true;
    public bool hardwareAcceleration = true;
}
