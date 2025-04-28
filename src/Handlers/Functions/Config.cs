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
    static readonly DataContractJsonSerializer Serializer = new(typeof(ConfigData), 
        new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });

    public static string CustomDllPath;
    public static bool CustomDll, BetaDll, AutoLogin, FixMinimizing, Rpc, WelcomeMessage, BackgroundParallaxEffect, HardwareAcceleration;

    public readonly static string ConfigFilePath = $"{Managers.VersionManagement.launcherPath}\\config.txt";

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

    public static void SaveConfig(bool showNotification = true)
    {
        if (SDK.Minecraft.Installed) SDK.Minecraft.Debug = FixMinimizing;
        RenderOptions.ProcessRenderMode = HardwareAcceleration ? RenderMode.Default : RenderMode.SoftwareOnly;

        using var stream = File.Create(ConfigFilePath);
        Serializer.WriteObject(stream, new ConfigData
        {
            customDll = CustomDll,
            customDllPath = CustomDllPath,
            betaDll = BetaDll,
            autoLogin = AutoLogin,
            fixMinimizing = FixMinimizing,
            rpc = Rpc,
            welcomeMessage = WelcomeMessage,
            backgroundParallaxEffect = BackgroundParallaxEffect,
            hardwareAcceleration = HardwareAcceleration
        });

        if (showNotification) Application.Current.Dispatcher.Invoke(() => MainWindow.CreateMessageBox("Config saved!", true));
    }

    public static void LoadConfig()
    {
        ConfigData config = new();
        try
        {
            using var stream = File.OpenRead(ConfigFilePath);
            config = (ConfigData)Serializer.ReadObject(stream);
        }
        catch { }

        CustomDll = config.customDll;
        CustomDllPath = config.customDllPath;
        BetaDll = config.betaDll;
        AutoLogin = config.autoLogin;
        FixMinimizing = config.fixMinimizing;
        Rpc = config.rpc;
        WelcomeMessage = config.welcomeMessage;
        BackgroundParallaxEffect = config.backgroundParallaxEffect;
        HardwareAcceleration = config.hardwareAcceleration;

        if (SDK.Minecraft.Installed) SDK.Minecraft.Debug = FixMinimizing;
        SaveConfig(false);
    }
}