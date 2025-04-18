using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Flarial.Launcher.Structures;
using Newtonsoft.Json;

namespace Flarial.Launcher.Functions
{
    public class Config
    {
        public static string CustomDLLPath;

        public static bool UseBetaDLL, MCMinimized, AutoLogin, UseCustomDLL;

        public readonly static string Path = $"{Managers.VersionManagement.launcherPath}\\config.txt";

        public static async Task<string> ReadAllTextAsync(string path)
        {
            using StreamReader reader = new(path);
            return await reader.ReadToEndAsync();
        }

        public static async Task WriteAllTextAsync(string path, string content)
        {
            using StreamWriter writer = new(path, false);
            await writer.WriteAsync(content);
        }

        public static async Task SaveConfigAsync(bool value = true)
        {
            if (SDK.Minecraft.Installed) SDK.Minecraft.Debug = MCMinimized;
            await WriteAllTextAsync(Path, JsonConvert.SerializeObject(new ConfigData()
            {
                shouldUseCustomDLL = UseCustomDLL,
                custom_dll_path = CustomDLLPath,
                shouldUseBetaDll = UseBetaDLL,
                mcMinimized = MCMinimized,
                autoLogin = AutoLogin,
            }));
            if (value) Application.Current.Dispatcher.Invoke(() => MainWindow.CreateMessageBox("Config saved!"));
        }

        public static async Task LoadConfigAsync()
        {
            ConfigData config = new();
            try { config = JsonConvert.DeserializeObject<ConfigData>(await ReadAllTextAsync(Path)); } catch { }

            UseBetaDLL = config.shouldUseBetaDll;
            CustomDLLPath = config.custom_dll_path;
            MCMinimized = config.mcMinimized;
            AutoLogin = config.autoLogin;
            UseCustomDLL = config.shouldUseCustomDLL;

            if (SDK.Minecraft.Installed) SDK.Minecraft.Debug = MCMinimized;
            await SaveConfigAsync(false);
        }

    }
}