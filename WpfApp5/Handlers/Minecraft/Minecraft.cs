using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Management.Deployment;

namespace Flarial.Launcher
{

    public static partial class Minecraft
    {

        public static Process Process;
        public const string FamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

        public static PackageManager PackageManager { get; private set; }
        public static Windows.ApplicationModel.Package Package { get; private set; }
        public static Windows.Storage.ApplicationData ApplicationData { get; private set; }

        public static void Init()
        {

            InitManagers();
            FindPackage();



            var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
            if (mcIndex.Length > 0)
            {
                Process = mcIndex[0];

            }

        }

        public static bool isInstalled()
        {
            PackageManager = new PackageManager();
            var Packages = PackageManager.FindPackages(FamilyName);
            return Packages.Count() != 0;
        }

        public static void InitManagers()
        {
            PackageManager = new PackageManager();
            var Packages = PackageManager.FindPackages(FamilyName);
            
            if (Packages.Count() == 0)
            {
                //Environment.Exit(0);
            }
            else
            {
                ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);
            }

            //  ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);


        }

        public static void FindPackage()
        {

            if (PackageManager is null) throw new NullReferenceException();
            var Packages = PackageManager.FindPackages(FamilyName);

            if (Packages.Count() != 0)
            {
                Package = Packages.First();
            }
        }
        public static double RoundToSignificantDigits(this double d, int digits)
        {
            if (d == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }
        public static Version GetVersion()
        {
            
            if(Package != null)
            {
                var v = Package.Id.Version;

                double buildVersion = (double)v.Build;
                var Ok = (int)buildVersion.RoundToSignificantDigits(2);
                ;
                return new Version(v.Major, v.Minor,
                    int.Parse(Ok.ToString().Substring(0, 2)));
            }

            return new Version(0, 0);
        }
        
        
        
        public static async Task WaitForModules(Action action)
        {
            if(Process is { HasExited: true }) Process = null;
            
            while (Process == null)
            {
                var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
                if (mcIndex.Length > 0)
                {
                    Process = mcIndex[0];

                }
                
                await Task.Delay(100);
            }

            Trace.WriteLine("Waiting for Minecraft to load");
            while (true)
            {
                Trace.WriteLine("test");
                
                Process.Refresh();
                if (!Process.HasExited)
                {
                    Trace.WriteLine(MainWindow.isDllDownloadFinished);
                    if (Process.Modules.Count > 160 && MainWindow.isDllDownloadFinished)
                    {
                        await Task.Delay(1500);
                        break;
                    }
                }
                else
                {
                    
                    Trace.WriteLine("Process GONE! (L)");
                    break;
                }
            }
            Trace.WriteLine("Minecraft finished loading");

            if (!Process.HasExited) action();
        }

    }
}