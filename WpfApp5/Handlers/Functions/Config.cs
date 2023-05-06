using System.IO;
using System.Threading.Tasks;
using Flarial.Launcher.Structures;
using Newtonsoft.Json;

namespace Flarial.Launcher.Functions;

public class Config
{
    
    public static string Path = $"{Managers.VersionManagement.launcherPath}\\config.txt";
    
    public static async Task saveConfig(string version, bool shouldUseCustomDLLreal, string customdllpath, bool shouldUseBetaDLLreal, bool closeToTray)
    {
        if (!File.Exists(Path))
        {
            File.Create(Path);


            await Task.Delay(1000);

        }
        
        var ts = new ConfigData()
        {
            minecraft_version = version,

            shouldUseCustomDLL = shouldUseCustomDLLreal,

            custom_dll_path = customdllpath,
            
            shouldUseBetaDll = shouldUseBetaDLLreal,
            
            closeToTray = closeToTray
        };

        var tss = JsonConvert.SerializeObject(ts);

        File.WriteAllText(Path, tss);
    }
    
    public static ConfigData? getConfig()
    {
        if (!File.Exists(Path))
        {
            return null;
        }
        
        if (File.ReadAllText(Path).Length == 0)
        {
            return null;
        }
        var s = File.ReadAllText(Path);


        return JsonConvert.DeserializeObject<ConfigData>(s);

    }
}