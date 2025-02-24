using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using Windows.Management.Deployment;
using Windows.Security.Authentication.Web.Core;
using Windows.Services.Store;
using Flarial.Launcher.Functions;
using Version = System.Version;

namespace Flarial.Launcher
{

    public static partial class Minecraft
    {

        public static Process Process1;
        public const string FamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
        public static double waitformodules = 153;
        public static PackageManager PackageManager { get; private set; }
        public static Windows.ApplicationModel.Package Package { get; private set; }
        public static Windows.Storage.ApplicationData ApplicationData { get; private set; }

        public static volatile int modules;

        /* CREDITS @AETOPIA */
        [DllImport("kernel32.dll")]
        static extern long GetPackagesByPackageFamily(
            [MarshalAs(UnmanagedType.LPWStr)] string packageFamilyName,
            ref uint count,
            IntPtr packageFullNames,
            ref uint bufferLength,
            IntPtr buffer);

        [ComImport]
        [Guid("f27c3930-8029-4ad1-94e3-3dba417810c1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IPackageDebugSettings
        {
            long EnableDebugging(
                [MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
                [MarshalAs(UnmanagedType.LPWStr)] string debuggerCommandLine,
                [MarshalAs(UnmanagedType.LPWStr)] string environment);

            long DisableDebugging([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long Suspend([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long Resume([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long TerminateAllProcesses([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long SetTargetSessionId(ulong sessionId);
            long StartServicing([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long StopServicing([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);
            long StartSessionRedirection([MarshalAs(UnmanagedType.LPWStr)] string packageFullName, ulong sessionId);
            long StopSessionRedirection([MarshalAs(UnmanagedType.LPWStr)] string packageFullName);

            long GetPackageExecutionState([MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
                IntPtr packageExecutionState);

            long RegisterForPackageStateChanges(
                [MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
                IntPtr pPackageExecutionStateChangeNotification,
                IntPtr pdwCookie);

            long UnregisterForPackageStateChanges(ulong dwCookie);
        }

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
            if (Config.MCMinimized) SDK.Minecraft.Debug = true;
            else SDK.Minecraft.Debug = false;
        }



        public static bool isInstalled()
        {
            PackageManager = new PackageManager();
            var userSecurityId = WindowsIdentity.GetCurrent().User.Value;
            var packages = PackageManager.FindPackagesForUser(userSecurityId);

            foreach (var package in packages)
            {
                if (package.Id.FamilyName == FamilyName)
                {
                    return true;
                }
            }

            return false;
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
            else
            {
                Task.Run(() => Flarial.Launcher.SDK.Minecraft.Installed);
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

        public static async Task CustomDLLLoop()
        {
            while (true)
            {
                if (modules >= Config.WaitFormodules)
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
                if (modules < Config.WaitFormodules) continue;
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
                    var filteredProcesses = mcIndex.Where(p => p.PrivateMemorySize64 > 200 * 1024 * 1024);

                    if (mcIndex.Length > 0)
                    {
                        Process1 = filteredProcesses.OrderByDescending(p => p.StartTime).FirstOrDefault();
                    }

                    await Task.Delay(100);
                }

                if (Process1.HasExited)
                {
                    Trace.WriteLine("Process GONE!");
                    Application.Current.Dispatcher.Invoke(() => { MainWindow.StatusLabel.Text = "MC_EXITED_PROCESS"; });
                    modules = 0;
                    Process1 = null;
                }
                else
                {
                    try
                    {
                        var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
                        var filteredProcesses = mcIndex.Where(p => p.PrivateMemorySize64 > 200 * 1024 * 1024);
                        Process1 = filteredProcesses.OrderByDescending(p => p.StartTime).FirstOrDefault();
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
                        Application.Current.Dispatcher.Invoke(() => { MainWindow.StatusLabel.Text = "MC_NO_PROCESS"; });
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error finding process: {ex.Message}");
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindow.StatusLabel.Text = "MC_ERROR_PROCESS";
                        });
                    }

                }
            }
        }


    }
}