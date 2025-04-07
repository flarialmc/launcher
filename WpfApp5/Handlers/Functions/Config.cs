using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Flarial.Launcher.Structures;
using Newtonsoft.Json;

namespace Flarial.Launcher.Functions
{

    public class Config
    {
        public static string Version;
        public static bool UseCustomDLL;
        public static string CustomDLLPath;
        public static bool UseBetaDLL;
        public static bool MCMinimized;
        public static bool AutoLogin;
        public static string CustomThemePath;
        public static double WaitFormodules;


        public static string Path = $"{Managers.VersionManagement.launcherPath}\\config.txt";

        public static async Task<string> ReadAllTextAsync(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return await reader.ReadToEndAsync();
            }
        }
        public static async Task WriteAllTextAsync(string path, string content)
        {
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                await writer.WriteAsync(content);
            }
        }
        public static async Task saveConfig(bool shi = true)
        {
            if (SDK.Minecraft.Installed)
                SDK.Minecraft.Debug = MCMinimized;

            if (!File.Exists(Path))
            {
                File.Create(Path);


                await Task.Delay(1000);

            }
            var ts = new ConfigData()
            {
                minecraft_version = Version,

                shouldUseCustomDLL = UseCustomDLL,

                custom_dll_path = CustomDLLPath,

                shouldUseBetaDll = UseBetaDLL,

                mcMinimized = MCMinimized,

                autoLogin = AutoLogin,

                waitForModules = WaitFormodules,

                custom_theme_path = CustomThemePath
            };

            var tss = JsonConvert.SerializeObject(ts);

            await WriteAllTextAsync(Path, tss);

            if (shi)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.CreateMessageBox("Config saved");

                });
        }

        public static async Task loadConfig()
        {
            ConfigData config = Config.getConfig();

            if (config == null)
            {
                return;

            }

            Version = config.minecraft_version;
            UseBetaDLL = config.shouldUseBetaDll;
            CustomDLLPath = config.custom_dll_path;
            MCMinimized = config.mcMinimized;
            AutoLogin = config.autoLogin;
            UseCustomDLL = config.shouldUseCustomDLL;
            WaitFormodules = config.waitForModules;

            if (SDK.Minecraft.Installed) SDK.Minecraft.Debug = MCMinimized;
            await saveConfig(false);
        }

        public static ConfigData getConfig()
        {
            if (!File.Exists(Path))
            {
                return null;
            }

            if (File.ReadAllText(Path).Length == 0)
            {
                return new ConfigData()
                {
                    autoLogin = true,
                    mcMinimized = true,
                    shouldUseBetaDll = false,
                    shouldUseCustomDLL = false,
                    waitForModules = 153
                };
            }
            var s = File.ReadAllText(Path);

            ConfigData lol = JsonConvert.DeserializeObject<ConfigData>(s);
            if (lol.waitForModules == 0) lol.waitForModules = 153;
            return lol;

        }
    }
}