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

    public static bool UseBetaDLL, MCMinimized, AutoLogin, UseCustomDLL, HardwareAcceleration;

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

    public static void SaveConfig(bool value = true)
    {
        if (SDK.Minecraft.Installed) SDK.Minecraft.Debug = MCMinimized;
        RenderOptions.ProcessRenderMode = HardwareAcceleration ? RenderMode.Default : RenderMode.SoftwareOnly;

        using var stream = File.Create(Path);
        Serializer.WriteObject(stream, new ConfigData
        {
            shouldUseCustomDLL = UseCustomDLL,
            custom_dll_path = CustomDLLPath,
            shouldUseBetaDll = UseBetaDLL,
            mcMinimized = MCMinimized,
            autoLogin = AutoLogin,
            hardwareAcceleration = HardwareAcceleration
        });

        if (value) Application.Current.Dispatcher.Invoke(() => MainWindow.CreateMessageBox("Config saved!"));
    }

    public static void LoadConfig()
    {
        ConfigData config = new();
        try { using var stream = File.OpenRead(Path); config = (ConfigData)Serializer.ReadObject(stream); } catch { }

        UseBetaDLL = config.shouldUseBetaDll;
        CustomDLLPath = config.custom_dll_path;
        MCMinimized = config.mcMinimized;
        AutoLogin = config.autoLogin;
        UseCustomDLL = config.shouldUseCustomDLL;
        HardwareAcceleration = config.hardwareAcceleration;

        if (SDK.Minecraft.Installed) SDK.Minecraft.Debug = MCMinimized;
        SaveConfig(false);
    }
}