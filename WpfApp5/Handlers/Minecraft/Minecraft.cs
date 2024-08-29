using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Windows.Management.Deployment;
using Windows.Services.Store;
using Flarial.Launcher.Functions;
namespace Flarial.Launcher
{

    public static partial class Minecraft
    {

        public static Process Process1;
        public const string FamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

        public static PackageManager PackageManager { get; private set; }
        public static Windows.ApplicationModel.Package Package { get; private set; }
        public static Windows.Storage.ApplicationData ApplicationData { get; private set; }

        public static volatile int modules;

        public static void Init()
        {

            InitManagers();
            FindPackage();

            var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
            if (mcIndex.Length > 0)
            {
                Process1 = mcIndex[0];

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

            if (Package != null)
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

        public static async Task CustomDLLLoop()
        {
            while (true)
            {
                if (modules >= 150)
                {
                    Trace.WriteLine("Injected!");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow.StatusLabel.Text = Insertion.Insert(Config.CustomDLLPath).ToString();
                    });
                    return;
                }
            }
        }

        public static async Task RealDLLLoop()
        {

            Trace.WriteLine("starting RealDLLLoop.");
            while (true)
            {
                if (modules < 150) continue;
                Trace.WriteLine("Injected!");
                Application.Current.Dispatcher.Invoke(() => { MainWindow.actionOnInject(); });
                break;
            }
        }

        public static async Task MCLoadLoop()
        {

            while (true)
            {
                //Trace.WriteLine("Running loop...");

                while (Process1 == null)
                {
                    //Trace.WriteLine("Trying to find process");
                    var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
                    if (mcIndex.Length > 0)
                    {
                        Process1 = mcIndex[0];

                    }

                    await Task.Delay(100);
                }

                if (Process1.HasExited)
                {
                    Trace.WriteLine("Process GONE!");
                    modules = 0;
                    Process1 = null;
                }
                else
                {
                    try
                    {
                        Process1 = Process.GetProcessesByName("Minecraft.Windows")[0];
                        //Trace.WriteLine(Process1.ProcessName + " " + Process1.Id);

                        try
                        {
                            //Trace.WriteLine("Load COUNT: " + Process1.Modules.Count);
                            modules = Process1.Modules.Count;
                        }
                        catch (System.ComponentModel.Win32Exception ex)
                        {
                            Trace.WriteLine($"Failed to access modules: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Unexpected error: {ex.Message}");
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Trace.WriteLine("No processes found with the name 'Minecraft.Windows'.");
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error finding process: {ex.Message}");
                    }

                }
            }
        }


        public class StoreHelper
        {
            private static StoreContext context = StoreContext.GetDefault();

            public static async Task<bool> HasBought()
            {
                try
                {
                    StoreAppLicense appLicense = await context.GetAppLicenseAsync();
                    Trace.Write("TROLLING " + appLicense.IsActive + "\nIs Trial Mode: " + appLicense.IsTrial + "\nTrial time: " + appLicense.TrialTimeRemaining + "\nsmth: " + appLicense.IsTrialOwnedByThisUser);
                    return appLicense.IsActive && !appLicense.IsTrial;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}