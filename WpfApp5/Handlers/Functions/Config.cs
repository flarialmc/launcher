using System.IO;
using System.Threading.Tasks;
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


        public static string Path = $"{Managers.VersionManagement.launcherPath}\\config.txt";

        public static async Task saveConfig()
        {
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

                custom_theme_path = CustomThemePath
            };

            var tss = JsonConvert.SerializeObject(ts);

            File.WriteAllText(Path, tss);

            MainWindow.CreateMessageBox("Config saved");
        }

        public static void loadConfig()
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


            

            if (CustomDLLPath != "amongus")
            {
                //CustomDllButton.IsChecked = true;
                //dllTextBox.Visibility = Visibility.Visible;
                //dllTextBox.Text = custom_dll_path;
            }


            //TrayButton.IsChecked = closeToTray;

            //BetaDLLButton.IsChecked = shouldUseBetaDLL;
            //AutoLoginButton.IsChecked = autoLogin;
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
                {autoLogin = true,
                mcMinimized = true,
                shouldUseBetaDll = false,
                shouldUseCustomDLL = false,
                }
                ;
            }
            var s = File.ReadAllText(Path);


            return JsonConvert.DeserializeObject<ConfigData>(s);

        }
    }
}