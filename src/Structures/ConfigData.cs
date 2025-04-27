namespace Flarial.Launcher.Structures;

public struct ConfigData
{
    public ConfigData() { }

    public string customDllPath;
    public bool customDll, betaDll, autoLogin = true, fixMinimizing = true, rpc = true, welcomeMessage = true, backgroundParallaxEffect = true;
}
