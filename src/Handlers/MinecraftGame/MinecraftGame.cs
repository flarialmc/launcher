using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using Windows.Management.Deployment;
using Flarial.Launcher.Services.Core;

namespace Flarial.Launcher;

public static partial class MinecraftGame
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
        var fixMinecraftMinimizing = true;
        if (fixMinecraftMinimizing && isInstalled()) Minecraft.HasUWPAppLifecycle = true;
        else if (!fixMinecraftMinimizing && isInstalled()) Minecraft.HasUWPAppLifecycle = false;
    }



    public static bool isInstalled()
    {
        return Minecraft.IsInstalled;
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

    public static Version GetVersion() => new(Minecraft.IsInstalled ? Minecraft.Version : "0.0.0");
}