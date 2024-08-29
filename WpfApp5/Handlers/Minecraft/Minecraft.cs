using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Windows.Management.Deployment;
using Windows.Security.Authentication.Web.Core;
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
                    string minecraftStoreId = "9NBLGGH2JHXJ";
                    Trace.WriteLine("TROLLING " + appLicense.IsActive + "\nIs Trial Mode: " + appLicense.IsTrial + "\nTrial time: " + appLicense.TrialTimeRemaining + "\nsmth: " + appLicense.IsTrialOwnedByThisUser + "\nContaints Key: " +  appLicense.AddOnLicenses.ContainsKey(minecraftStoreId));
                    Trace.WriteLine("Funny Man"+ appLicense.AddOnLicenses.First().Value.SkuStoreId);
                    return appLicense.IsActive && !appLicense.IsTrial && appLicense.AddOnLicenses.ContainsKey(minecraftStoreId) && appLicense.AddOnLicenses[minecraftStoreId].IsActive;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        
       class WUTokenHelper {

        public static string GetWUToken() {
            try {
                string token;
                int status = WUTokenCaller.GetWUToken(out token);
                if (status >= WU_ERRORS_START && status <= WU_ERRORS_END)
                    throw new WUTokenException(status);
                else if (status != 0)
                    Marshal.ThrowExceptionForHR(status);
                return token;
            } catch (SEHException e) {
                Marshal.ThrowExceptionForHR(e.HResult);
                return ""; //ghey
            }
        }

        private const int WU_ERRORS_START = unchecked((int) 0x80040200);
        private const int WU_NO_ACCOUNT = unchecked((int) 0x80040200);

        private const int WU_TOKEN_FETCH_ERROR_BASE = unchecked((int) 0x80040300);
        private const int WU_TOKEN_FETCH_ERROR_END = unchecked((int) 0x80040400);

        private const int WU_ERRORS_END = unchecked((int) 0x80040400);

        public class WUTokenException : Exception {
            public WUTokenException(int exception) : base(GetExceptionText(exception)) {
                HResult = exception;
            }
            private static String GetExceptionText(int e) {
                if (e >= WU_TOKEN_FETCH_ERROR_BASE && e < WU_TOKEN_FETCH_ERROR_END)
                {
                    var actualCode = (byte) e & 0xff;

                    if(!Enum.IsDefined(typeof(WebTokenRequestStatus), e))
                    {
                        return $"WUTokenHelper returned bogus HRESULT: {e} (THIS IS A BUG)";
                    }
                    var status = (WebTokenRequestStatus) Enum.ToObject(typeof(WebTokenRequestStatus), actualCode);
                    switch (status)
                    {
                        case WebTokenRequestStatus.Success:
                            return "Success (THIS IS A BUG)";
                        case WebTokenRequestStatus.UserCancel:
                            return "User cancelled token request (THIS IS A BUG)"; //TODO: should never happen?
                        case WebTokenRequestStatus.AccountSwitch:
                            return "User requested account switch (THIS IS A BUG)"; //TODO: should never happen?
                        case WebTokenRequestStatus.UserInteractionRequired:
                            return "User interaction required to complete token request (THIS IS A BUG)";
                        case WebTokenRequestStatus.AccountProviderNotAvailable:
                            return "Xbox Live account services are currently unavailable";
                        case WebTokenRequestStatus.ProviderError:
                            return "Unknown Xbox Live error";
                    }
                }
                switch (e) {
                    case WU_NO_ACCOUNT: return "No Microsoft account found";
                    default: return "Unknown " + e;
                }
            }
        }

    }
    }
}