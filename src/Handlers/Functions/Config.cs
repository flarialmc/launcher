using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Flarial.Launcher.Structures;

namespace Flarial.Launcher.Functions;

public static class Config
{
    static readonly DataContractJsonSerializer Serializer = new(typeof(ConfigData), new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });

    public static string CustomDLLPath;

    public static bool UseBetaDLL, AutoLogin, UseCustomDLL, AutoInject, MinimizeToTray;

    static bool _mcMinimized;

    public static bool MCMinimized
    {
        get => _mcMinimized;
        set
        {
            _mcMinimized = value;
            if (SDK.Minecraft.Installed) SDK.Minecraft.Debug = value;
        }
    }

    static bool _hardwareAcceleration;

    public static bool HardwareAcceleration
    {
        get => _hardwareAcceleration;
        set
        {
            _hardwareAcceleration = value;
            RenderOptions.ProcessRenderMode = value ? RenderMode.Default : RenderMode.SoftwareOnly;
        }
    }

    public readonly static string Path = $"{Managers.VersionManagement.launcherPath}\\config.txt";

    public static async Task<string> ReadAllTextAsync(string path)
    {
        using var reader = File.OpenText(path);
        return await reader.ReadToEndAsync();
    }

    public static async Task WriteAllTextAsync(string path, string content)
    {
        using var writer = File.CreateText(path);
        await writer.WriteAsync(content);
    }

    public static void SaveConfig()
    {
        using var stream = File.Create(Path);
        Serializer.WriteObject(stream, new ConfigData
        {
            shouldUseCustomDLL = UseCustomDLL,
            custom_dll_path = CustomDLLPath,
            shouldUseBetaDll = UseBetaDLL,
            mcMinimized = _mcMinimized,
            autoLogin = AutoLogin,
            minimizeToTray = MinimizeToTray,
            autoInject = AutoInject,
            hardwareAcceleration = _hardwareAcceleration
        });
    }

    public static void LoadConfig(bool hardwareAcceleration)
    {
        ConfigData config = new();
        try
        {
            using var stream = File.OpenRead(Path);
            config = (ConfigData)Serializer.ReadObject(stream);
        }
        catch { }

        UseBetaDLL = config.shouldUseBetaDll;
        CustomDLLPath = config.custom_dll_path;
        MCMinimized = config.mcMinimized;
        AutoLogin = config.autoLogin;
        AutoInject = config.autoInject;
        MinimizeToTray =  config.minimizeToTray;
        UseCustomDLL = config.shouldUseCustomDLL;
        HardwareAcceleration = hardwareAcceleration && config.hardwareAcceleration;
    }
}