using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace Flarial.Launcher;

public static partial class Minecraft
{

    public static Process Process;
    public const string FamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    public static Windows.Management.Deployment.PackageManager? PackageManager { get; private set; }
    public static Windows.ApplicationModel.Package? Package { get; private set; }
    public static Windows.ApplicationModel.PackageId PackageId => Package?.Id ?? throw new NullReferenceException();
    public static Windows.System.ProcessorArchitecture PackageArchitecture => PackageId.Architecture;
    public static Windows.ApplicationModel.PackageVersion PackageVersion => PackageId.Version;
    public static Windows.Storage.ApplicationData? ApplicationData { get; private set; }

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

    public static void InitManagers()
    {
        PackageManager = new Windows.Management.Deployment.PackageManager();
        var Packages = PackageManager.FindPackages(FamilyName);
        if (Packages.Count() == 0)
        {
            MessageBox.Show("You don't have MC installed LOL");
        } else
        {
            ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);
        }

        ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);


    }

    public static void FindPackage()
    {

        if (PackageManager is null) throw new NullReferenceException();
        var Packages = PackageManager.FindPackages(FamilyName);

        if (Packages.Count() == 0)
        {
            MessageBox.Show("You don't have MC installed LOL");
        }
        else
        {
            Package = Packages.First();
        }




    }

    public static Version GetVersion()
    {
        var v = PackageVersion;
        return new Version(v.Major, v.Minor, v.Build, v.Revision);
    }
}