using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Management.Deployment;

namespace Flarial.Launcher
{

    public static partial class Minecraft
    {

        public static Process Process;
        public const string FamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

        public static Windows.Management.Deployment.PackageManager PackageManager { get; private set; }
        public static Windows.ApplicationModel.Package Package { get; private set; }
        public static Windows.ApplicationModel.PackageId PackageId => Package.Id ?? throw new NullReferenceException();
        public static Windows.System.ProcessorArchitecture PackageArchitecture => PackageId.Architecture;
        public static Windows.ApplicationModel.PackageVersion PackageVersion => PackageId.Version;
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
                    
                CustomDialogBox MessageBox = new CustomDialogBox("Error", "Minecraft needs to be installed to verify that you have bought the game.", "MessageBox");
                MessageBox.ShowDialog();
                Environment.Exit(0);
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

        public static Version GetVersion()
        {
            var v = PackageVersion;
            return new Version(v.Major, v.Minor, v.Build, v.Revision);
        }
        
        public static async Task WaitForModules()
        {
            while (Minecraft.Process == null)
            {
                await Task.Delay(4000);
            }

            Console.WriteLine("Waiting for Minecraft to load");
            while (true)
            {
                Minecraft.Process.Refresh();
                if (Minecraft.Process.Modules.Count > 155)
                    break;
                else
                    await Task.Delay(4000);
            }
            Console.WriteLine("Minecraft finished loading");
        }

    }
}