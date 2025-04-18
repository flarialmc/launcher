namespace Flarial.Launcher.Structures
{
    public struct ConfigData
    {
        public ConfigData() { }
        public string custom_dll_path;
        public bool shouldUseCustomDLL, shouldUseBetaDll, mcMinimized = true, autoLogin = true;
    }
}