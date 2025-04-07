using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using Windows.Management.Deployment;
using Bedrockix.Minecraft;
using Flarial.Launcher.Functions;
using Version = System.Version;

namespace Flarial.Launcher
{

    public static partial class Minecraft
    {

        public static Process Process1;
        public const string FamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
        public static PackageManager PackageManager { get; private set; }
        public static Windows.ApplicationModel.Package Package { get; private set; }
        public static Windows.Storage.ApplicationData ApplicationData { get; private set; }


        /* CREDITS @AETOPIA */

        public static void Init()
        {

            InitManagers();
            FindPackage();

            var mcIndex = Process.GetProcessesByName("Minecraft.Windows");

            if (mcIndex.Length > 0)
            {
                Process1 = mcIndex[0];

            }

            // minimize fix
            if (Config.MCMinimized && isInstalled()) Game.Debug = true;
            else if (!Config.MCMinimized && isInstalled()) Game.Debug = false;
        }



        public static bool isInstalled()
        {
            return Game.Installed;
        }

        public static void InitManagers()
        {
            PackageManager = new PackageManager();
            var userSecurityId = WindowsIdentity.GetCurrent().User.Value;
            var packages = PackageManager.FindPackagesForUser(userSecurityId);

            bool packageFound = false;

            foreach (var package in packages)
            {
                if (package.Id.FamilyName == FamilyName)
                {
                    packageFound = true;
                    ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);
                    break;
                }
            }

            if (!packageFound)
            {
                Trace.WriteLine($"Package {FamilyName} not found.");
            }
        }



        public static void FindPackage()
        {
            if (PackageManager is null) throw new NullReferenceException();
            var userSecurityId = WindowsIdentity.GetCurrent().User.Value;
            var packages = PackageManager.FindPackagesForUser(userSecurityId);

            if (packages.Any(package => package.Id.FamilyName == FamilyName))
            {
                Package = packages.First(package => package.Id.FamilyName == FamilyName);
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

            if (Package != null)
            {
                var v = Package.Id.Version;

                double buildVersion = (double)v.Build;
                var Ok = (int)buildVersion.RoundToSignificantDigits(2);
                ;
                return new Version(v.Major, v.Minor,
                    int.Parse(Ok.ToString().Substring(0, 2)));
            }

            return new Version(0, 0, 0);
        }
    }
}