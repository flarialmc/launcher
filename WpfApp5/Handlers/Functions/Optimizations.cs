using System.Runtime.InteropServices;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using Microsoft.VisualBasic.Logging;
using Octokit;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Policy;
using System.Threading.Channels;
using System.Windows.Documents;
using System.Xml.Linq;

namespace Flarial.Launcher.Handlers.Functions
{




    public class NvidiaWifiOptimizer
    {
        private const string NvidiaRegKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\PCI\";
        private const string NvidiaRegSubKey = @"NVIDIA_DEV.0";
        private const string NvidiaRegValue = "DeviceDesc";
        private const string MinecraftExecutable = "Minecraft.Windows";
        private const string NvidiaPanelExecutable = "nvcplui.exe";

        private static bool nvidiaPanelOptimized = false; // To keep track of Nvidia panel optimization status
        private static bool internetOptimized = false; // To keep track of internet optimization status

        public static void Optimize()
        {
            // Check if Nvidia GPU is present
            if (IsNvidiaGPU())
            {
                // Optimize Nvidia panel settings for Minecraft if not already optimized
                if (!nvidiaPanelOptimized)
                {
                    SetNvidiaPanelSettings();
                    nvidiaPanelOptimized = true; // Mark Nvidia panel as optimized
                }
            }
            else
            {
                Trace.WriteLine("Nvidia GPU not found. Unable to optimize Nvidia panel settings.");
            }

            // Optimize internet settings for Minecraft if not already optimized
            if (!internetOptimized)
            {
                OptimizeInternetSettings();
                internetOptimized = true; // Mark internet settings as optimized
            }
        }

        private static bool IsNvidiaGPU()
        {
            // Get the Nvidia GPU description from the registry
            object gpuDescription = Registry.GetValue($"{NvidiaRegKey}{NvidiaRegSubKey}", NvidiaRegValue, null);

            // Check if Nvidia GPU description is not null and contains "NVIDIA"
            return gpuDescription != null && gpuDescription.ToString().Contains("NVIDIA");
        }

        private static void SetNvidiaPanelSettings()
        {
            // Start the Nvidia panel executable
            Process.Start(NvidiaPanelExecutable);

            // Wait for the Nvidia panel to open
            System.Threading.Thread.Sleep(2000);

            // Find the Nvidia panel process
            Process nvidiaPanelProcess = GetNvidiaPanelProcess();

            if (nvidiaPanelProcess != null)
            {
                // Set the Nvidia panel process priority to high for better performance
                nvidiaPanelProcess.PriorityClass = ProcessPriorityClass.High;

                // Set the Nvidia panel process affinity to a single CPU core
                nvidiaPanelProcess.ProcessorAffinity = new IntPtr(1);
            }
            else
            {
                Trace.WriteLine("Nvidia panel process not found. Unable to set Nvidia panel settings.");
            }
        }

        private static Process GetNvidiaPanelProcess()
        {
            // Find the Nvidia panel process by name
            Process[] processes = Process.GetProcessesByName(NvidiaPanelExecutable);

            // Return the first found process
            return processes.Length > 0 ? processes[0] : null;
        }

        private static void OptimizeInternetSettings()
        {
            // Modify internet settings in the Windows registry if not already optimized
            if (!internetOptimized)
            {
                SetTcpAckFrequency();
                SetTcpNoDelay();
                SetTcpIpOptimizations();
                DisableNagleAlgorithm();
                internetOptimized = true; // Mark internet settings as optimized
            }
        }

        private static void SetTcpAckFrequency()
        {
            // Set TcpAckFrequency to 1 for better internet responsiveness if not already set
            int tcpAckFrequency = (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", -1);
            if (tcpAckFrequency != 1)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", 1, RegistryValueKind.DWord);
            }
        }

        private static void SetTcpNoDelay()
        {
            // Set TcpNoDelay to 1 for improved network performance if not already set
            int tcpNoDelay = (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpNoDelay", -1);
            if (tcpNoDelay != 1)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpNoDelay", 1, RegistryValueKind.DWord);
            }
        }

        private static void SetTcpIpOptimizations()
        {
            // Modify TCP/IP settings using registry keys if not already set
            int tcpAckFrequency = (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", -1);
            int tcpNoDelay = (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpNoDelay", -1);
            int tcpWindowSize = (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpWindowSize", -1);
            int defaultTTL = (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "DefaultTTL", -1);

            if (tcpAckFrequency != 1 || tcpNoDelay != 1 || tcpWindowSize != 64240 || defaultTTL != 64)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpNoDelay", 1, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpWindowSize", 64240, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "DefaultTTL", 64, RegistryValueKind.DWord);
            }
        }

        private static void DisableNagleAlgorithm()
        {
            // Disable the Nagle algorithm for improved network responsiveness if not already disabled
            int tcpDelAckTicks = (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpDelAckTicks", -1);
            if (tcpDelAckTicks != 0)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpDelAckTicks", 0, RegistryValueKind.DWord);
            }
        }

        public static void DisableNvidiaPanelAndInternetOptimizations()
        {
            // Disable Nvidia panel settings for Minecraft if optimized
            if (nvidiaPanelOptimized)
            {
                DisableNvidiaPanelSettings();
                nvidiaPanelOptimized = false; // Mark Nvidia panel as not optimized
            }

            // Disable internet optimizations for Minecraft if optimized
            if (internetOptimized)
            {
                DisableInternetSettings();
                internetOptimized = false; // Mark internet settings as not optimized
            }
        }

        private static void DisableNvidiaPanelSettings()
        {
            // Find the Nvidia panel process
            Process nvidiaPanelProcess = GetNvidiaPanelProcess();

            if (nvidiaPanelProcess != null)
            {
                // Set the Nvidia panel process priority to normal
                nvidiaPanelProcess.PriorityClass = ProcessPriorityClass.Normal;

                // Set the Nvidia panel process affinity to all CPU cores
                nvidiaPanelProcess.ProcessorAffinity = new IntPtr(-1);
            }
            else
            {
                Trace.WriteLine("Nvidia panel process not found. Unable to disable Nvidia panel settings.");
            }
        }

        private static void DisableInternetSettings()
        {
            // Reset TCP/IP optimizations to default values if optimized
            if (internetOptimized)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", 2, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TCPNoDelay", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpWindowSize", 0, RegistryValueKind.DWord);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "DefaultTTL", 128, RegistryValueKind.DWord);

                // Enable the Nagle algorithm (revert the change)
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpDelAckTicks", 1, RegistryValueKind.DWord);
            }
        }

        public static void FlushDnsCache()
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "ipconfig",
                Arguments = "/flushdns",
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            Process process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();

            if (process.ExitCode == 0)
            {
                Trace.WriteLine("DNS cache flushed successfully.");
            }
            else
            {
                Trace.WriteLine("Failed to flush DNS cache. Error: " + output);
            }
        }
    }



    public class MinecraftOptimizer
    {
        private const string MinecraftExecutable = "Minecraft.Windows";

        public static int DesiredRAMAllocation { get; set; } = 4096; // Default to 4GB (4096MB) of RAM allocation

        public static void OptimizeMinecraft()
        {
            // Check if Minecraft is running
            Process minecraftProcess = GetMinecraftProcess();
            if (minecraftProcess == null)
            {
                Trace.WriteLine("Minecraft is not running. No optimizations performed.");
                return;
            }

            // Set process priority and affinity for Minecraft
            SetProcessSettings();

            // Check if fullscreen optimizations and DPI scaling are supported
            bool isFullscreenOptimizationsSupported = IsFullscreenOptimizationsSupported();
            bool isDPIScalingSupported = IsDPIScalingSupported();

            // Disable fullscreen optimizations for Minecraft if supported
            //if (isFullscreenOptimizationsSupported)
            //{
            //    DisableFullscreenOptimizations(minecraftProcess.MainWindowHandle);
            //}
            //else
            //{
            //    Trace.WriteLine("Fullscreen optimizations not supported on this version of Windows. Skipping...");
            //}

            // Disable DPI scaling for Minecraft if supported
            //if (isDPIScalingSupported)
            //{
            //    DisableDpiScaling(minecraftProcess.MainWindowHandle);
            //}
            //else
            //{
            //    Trace.WriteLine("DPI scaling not supported on this version of Windows. Skipping...");
            //}

            // Check DirectX version and apply optimizations accordingly
            string directXVersion = GetDirectXVersion();
            if (directXVersion == "DirectX 11")
            {
                // Apply DirectX 11 optimizations for Minecraft if supported
                if (isFullscreenOptimizationsSupported)
                {
                    ApplyDirectX11Optimizations();
                }
                else
                {
                    Trace.WriteLine("DirectX 11 optimizations require fullscreen optimizations. Skipping...");
                }
            }
            else if (directXVersion == "DirectX 12")
            {
                // Apply DirectX 12 optimizations for Minecraft if supported
                if (isFullscreenOptimizationsSupported)
                {
                    ApplyDirectX12Optimizations();
                }
                else
                {
                    Trace.WriteLine("DirectX 12 optimizations require fullscreen optimizations. Skipping...");
                }
            }
            else
            {
                Trace.WriteLine("Unknown DirectX version. Skipping DirectX optimizations.");
            }

            // Optimize mouse settings
            OptimizeMouseSettings();

            // Optimize keyboard settings
            OptimizeKeyboardSettings();

            // Perform additional performance and internet optimizations
            AdditionalOptimizations();
        }

        private static bool IsFullscreenOptimizationsSupported()
        {
            // Check if fullscreen optimizations are supported on the current version of Windows
            OperatingSystem os = Environment.OSVersion;
            Version version = os.Version;

            // Fullscreen optimizations are supported on Windows 10 version 1803 (April 2018 Update) and above.
            return os.Platform == PlatformID.Win32NT && version >= new Version(10, 0, 17134);
        }

        private static bool IsDPIScalingSupported()
        {
            // Check if DPI scaling is supported on the current version of Windows
            OperatingSystem os = Environment.OSVersion;
            Version version = os.Version;

            // DPI scaling is supported on Windows 8.1 (Windows 6.3) and above.
            return os.Platform == PlatformID.Win32NT && version >= new Version(6, 3);
        }

        //private static void DisableFullscreenOptimizations(IntPtr mainWindowHandle)
        //{
        //    // Disable fullscreen optimizations for Minecraft window
        //    SetWindowDisplayAttribute(mainWindowHandle, WindowDisplayAttribute.WDA_MONITOR, 1);
        //}

        //private static void DisableDpiScaling(IntPtr mainWindowHandle)
        //{
        //    // Disable DPI scaling for Minecraft window
        //    SetProcessDpiAwareness(mainWindowHandle, ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware);
        //}

        private static void SetProcessSettings()
        {
            // Find the Minecraft process
            Process minecraftProcess = GetMinecraftProcess();

            if (minecraftProcess != null)
            {
                // Set the Minecraft process priority to high for better performance
                minecraftProcess.PriorityClass = ProcessPriorityClass.High;

                // Set the Minecraft process affinity to a single CPU core
                minecraftProcess.ProcessorAffinity = new IntPtr(1);
            }
            else
            {
                Trace.WriteLine("Minecraft process not found. Unable to set process settings.");
            }
        }

        private static Process GetMinecraftProcess()
        {
            // Find the Minecraft process by name
            Process[] processes = Process.GetProcessesByName(MinecraftExecutable);

            // Return the first found process
            if (processes.Length > 0)
                return processes[0];
            else
                return null;
        }

        private static void ApplyDirectX11Optimizations()
        {
            // Apply DirectX 11 optimizations for Minecraft
            Trace.WriteLine("Applying DirectX 11 optimizations for Minecraft.");

            // Disable vsync for DirectX 11
            DisableVSyncDirectX11();

            // Set maximum pre-rendered frames to 1 for DirectX 11
            SetMaxPreRenderedFramesDirectX11(1);

            // Additional optimizations for DirectX 11
            DisableMouseAcceleration();
        }

        private static void ApplyDirectX12Optimizations()
        {
            // Apply DirectX 12 optimizations for Minecraft
            Trace.WriteLine("Applying DirectX 12 optimizations for Minecraft.");

            // Set maximum frame latency to 1 for DirectX 12
            SetMaxFrameLatencyDirectX12(1);

            // Additional optimizations for DirectX 12
            DisableMouseAcceleration();
        }

        private static void DisableVSyncDirectX11()
        {
            // Modify the Nvidia control panel settings to disable VSync for DirectX 11
            Process.Start("nvcplui.exe", "-silent -set SyncToVBlank=0");

            // Wait for the Nvidia control panel process to exit
            WaitForNvidiaControlPanelExit();
        }

        private static void SetMaxPreRenderedFramesDirectX11(int maxFrames)
        {
            // Modify the Nvidia control panel settings to set maximum pre-rendered frames for DirectX 11
            Process.Start("nvcplui.exe", $"-silent -set MultiFrame=1 -set MultiFrameValues={maxFrames}");

            // Wait for the Nvidia control panel process to exit
            WaitForNvidiaControlPanelExit();
        }

        private static void SetMaxFrameLatencyDirectX12(int maxLatency)
        {
            // Modify the Nvidia control panel settings to set maximum frame latency for DirectX 12
            Process.Start("nvcplui.exe", $"-silent -set LowLatencyMode=1 -set LowLatencyModeValues={maxLatency}");

            // Wait for the Nvidia control panel process to exit
            WaitForNvidiaControlPanelExit();
        }

        private static void WaitForNvidiaControlPanelExit()
        {
            // Wait for the Nvidia control panel process to exit
            Process[] processes = Process.GetProcessesByName("nvcplui");

            while (processes.Length > 0)
            {
                System.Threading.Thread.Sleep(100);
                processes = Process.GetProcessesByName("nvcplui");
            }
        }

        private static void DisableMouseAcceleration()
        {
            // Disable mouse acceleration in Windows
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseAcceleration", 0, RegistryValueKind.DWord);
        }

        private static void OptimizeMouseSettings()
        {
            // Set mouse settings for better precision and reduced latency
            SetMouseSpeed(10); // Set mouse speed to a desired value (e.g., 10)
            DisableEnhancePointerPrecision();
            DisableMouseTrails();
        }

        private static void SetMouseSpeed(int speed)
        {
            // Set mouse speed in Windows registry
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSensitivity", speed, RegistryValueKind.DWord);
        }

        private static void DisableEnhancePointerPrecision()
        {
            // Disable Enhance Pointer Precision (mouse acceleration) in Windows
            SystemParametersInfo(SPI_SETMOUSE, 0, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        private static void DisableMouseTrails()
        {
            // Disable mouse trails in Windows
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseTrails", 0, RegistryValueKind.DWord);
        }

        private static void OptimizeKeyboardSettings()
        {
            // Disable FilterKeys, StickyKeys, and ToggleKeys for better keyboard input responsiveness
            DisableFilterKeys();
            DisableStickyKeys();
            DisableToggleKeys();
        }

        private static void DisableFilterKeys()
        {
            // Disable FilterKeys in Windows
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Response", "Flags", 122, RegistryValueKind.DWord);
        }

        private static void DisableStickyKeys()
        {
            // Disable StickyKeys in Windows
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Preference", "OnOff", 0, RegistryValueKind.DWord);
        }

        private static void DisableToggleKeys()
        {
            // Disable ToggleKeys in Windows
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Preference", "Flags", 58, RegistryValueKind.DWord);
        }

        private static void AdditionalOptimizations()
        {
            // Adjust system settings for improved performance
            AdjustPowerSettings();

            // Optimize RAM allocation
            OptimizeRAMAllocation();
        }

        private static void AdjustPowerSettings()
        {
            // Set power plan to High Performance for better performance
            Process.Start("powercfg.exe", "/s 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
        }

        private static void OptimizeRAMAllocation()
        {
            // Set the desired amount of RAM allocation for Minecraft
            int desiredRAMInMB = DesiredRAMAllocation;
            Process minecraftProcess = GetMinecraftProcess();
            if (minecraftProcess != null)
            {
                int processId = minecraftProcess.Id;
                Process hackerProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = $"process where processid={processId} CALL setpriority \"Memory\", {desiredRAMInMB}",
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                hackerProcess.WaitForExit();
            }
            else
            {
                Trace.WriteLine("Minecraft process not found. Unable to optimize RAM allocation.");
            }
        }

        private static string GetDirectXVersion()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string description = obj["Description"]?.ToString();
                    if (!string.IsNullOrEmpty(description))
                    {
                        if (description.Contains("Direct3D 11"))
                        {
                            return "DirectX 11";
                        }
                        else if (description.Contains("Direct3D 12"))
                        {
                            return "DirectX 12";
                        }
                    }
                }
            }

            return "Unknown";
        }

        //[DllImport("user32.dll")]
        //private static extern bool SetWindowDisplayAttribute(IntPtr hWnd, WindowDisplayAttribute attribute, int attributeValue);

        //[DllImport("user32.dll")]
        //private static extern bool SetProcessDpiAwareness(IntPtr hWnd, ProcessDpiAwareness awareness);

        private enum WindowDisplayAttribute
        {
            WDA_MONITOR = 1
        }

        private enum ProcessDpiAwareness
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(int uiAction, int uiParam, IntPtr pvParam, int fWinIni);

        private const int SPI_SETMOUSE = 0x0004;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;
    }





    public class OptimizerManager
    {
        private const string AMDProcessorIdentifier = "AuthenticAMD";

        private AMDOptimizer amdOptimizer = new AMDOptimizer();
        private IntelOptimizer intelOptimizer = new IntelOptimizer();

        public void Optimize()
        {
            if (IsAMDProcessor())
                amdOptimizer.Optimize();
            else
            intelOptimizer.Optimize();
        }

        public void RevertOptimizations()
        {
            if (IsAMDProcessor())
            
                amdOptimizer.RevertOptimizations();
            else

            intelOptimizer.RevertOptimizations();
        }

        private static bool IsAMDProcessor()
        {
            string processorName = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            return processorName.Contains(AMDProcessorIdentifier);
        }


        public class AMDOptimizer
        {
            private const int ADL_OK = 0;
            private const int ADL_MAX_PATH = 256;
            private const string RegistryKeyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4D36E968-E325-11CE-BFC1-08002BE10318}\0000\UMD";
            private const string RegistryValueName = "FlipQueueSize";

            private bool preRenderedFramesModified = false; // To keep track of pre-rendered frames modification status

            [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int ADL_Main_Control_Create(int enumConnectedAdapters);

            [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int ADL_Main_Control_Destroy();

            [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int ADL_Adapter_NumberOfAdapters_Get(ref int numAdapters);

            [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int ADL_Adapter_AdapterInfo_Get(IntPtr info, int size);

            [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int ADL_Adapter_Active_Get(int adapterIndex, ref int status);

            [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int ADL_Adapter_Crossfire_Caps(int adapterIndex, ref int capable, ref int enabled);

            [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int ADL_Overdrive_Caps(int adapterIndex, ref int supported, ref int enabled, ref int version);

            [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int ADL_Overdrive6_PowerControlInfo_Get(int adapterIndex, ref int min, ref int max, ref int currentValue, ref int defaultValue, ref int stepValue);

            [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int ADL_Overdrive6_PowerControl_Get(int adapterIndex, ref int value);

            [DllImport("atiadlxx.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int ADL_Overdrive6_PowerControl_Set(int adapterIndex, int value);

            private bool InitializeADL()
            {
                return ADL_Main_Control_Create(1) == ADL_OK;
            }

            private void DestroyADL()
            {
                ADL_Main_Control_Destroy();
            }

            private int GetNumberOfAdapters()
            {
                int numAdapters = 0;
                ADL_Adapter_NumberOfAdapters_Get(ref numAdapters);
                return numAdapters;
            }

            private AdapterInfo[] GetAdapterInformation(int numAdapters)
            {
                IntPtr info = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(AdapterInfo)) * numAdapters);
                ADL_Adapter_AdapterInfo_Get(info, Marshal.SizeOf(typeof(AdapterInfo)) * numAdapters);
                AdapterInfo[] adapterInfoArray = new AdapterInfo[numAdapters];
                for (int i = 0; i < numAdapters; i++)
                {
                    IntPtr currentInfo = new IntPtr(info.ToInt64() + (Marshal.SizeOf(typeof(AdapterInfo)) * i));
                    adapterInfoArray[i] = (AdapterInfo)Marshal.PtrToStructure(currentInfo, typeof(AdapterInfo));
                }
                Marshal.FreeCoTaskMem(info);
                return adapterInfoArray;
            }

            private bool IsCrossfireEnabled(int adapterIndex)
            {
                int capable = 0;
                int enabled = 0;
                ADL_Adapter_Crossfire_Caps(adapterIndex, ref capable, ref enabled);
                return capable == 1 && enabled == 1;
            }

            private bool IsOverdriveSupported(int adapterIndex)
            {
                int supported = 0;
                int overdriveEnabled = 0;
                int version = 0;
                ADL_Overdrive_Caps(adapterIndex, ref supported, ref overdriveEnabled, ref version);
                return supported == 1;
            }

            private bool IsPowerControlModified(int adapterIndex)
            {
                int currentValue = 0;
                ADL_Overdrive6_PowerControl_Get(adapterIndex, ref currentValue);
                return currentValue != 0;
            }

            private bool SetPowerControl(int adapterIndex, int value)
            {
                int result = ADL_Overdrive6_PowerControl_Set(adapterIndex, value);
                return result == ADL_OK;
            }

            private byte[] GetByteDataFromHexString(string hexString)
            {
                string[] hexValues = hexString.Split(',');
                byte[] byteData = new byte[hexValues.Length];

                for (int i = 0; i < hexValues.Length; i++)
                {
                    byteData[i] = byte.Parse(hexValues[i], System.Globalization.NumberStyles.HexNumber);
                }

                return byteData;
            }

            public void SetFlipQueueSizeToHexValue()
            {
                try
                {
                    Registry.SetValue(RegistryKeyPath, RegistryValueName, GetByteDataFromHexString("31,00"), RegistryValueKind.Binary);
                    Trace.WriteLine("FlipQueueSize set to hexadecimal value successfully.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error setting FlipQueueSize: " + ex.Message);
                }
            }

            public void SetMaximumPreRenderedFramesToZero()
            {
                if (IsAMDProcessor())
                {
                    if (InitializeADL())
                    {
                        int numAdapters = GetNumberOfAdapters();

                        if (numAdapters == 0)
                        {
                            Trace.WriteLine("No AMD adapters found.");
                            DestroyADL();
                            return;
                        }

                        AdapterInfo[] adapterInfoArray = GetAdapterInformation(numAdapters);

                        foreach (var adapterInfo in adapterInfoArray)
                        {
                            if (!adapterInfo.IsActive)
                            {
                                Trace.WriteLine("Adapter " + adapterInfo.AdapterIndex + " is not active.");
                                continue;
                            }

                            if (IsCrossfireEnabled(adapterInfo.AdapterIndex))
                            {
                                Trace.WriteLine("Crossfire is enabled for adapter " + adapterInfo.AdapterIndex + ". Cannot modify maximum pre-rendered frames.");
                                continue;
                            }

                            if (!IsOverdriveSupported(adapterInfo.AdapterIndex))
                            {
                                Trace.WriteLine("Overdrive is not supported for adapter " + adapterInfo.AdapterIndex + ". Cannot modify maximum pre-rendered frames.");
                                continue;
                            }

                            if (IsPowerControlModified(adapterInfo.AdapterIndex))
                            {
                                Trace.WriteLine("Power Control is already modified for adapter " + adapterInfo.AdapterIndex + ". Cannot modify maximum pre-rendered frames.");
                                continue;
                            }

                            if (SetPowerControl(adapterInfo.AdapterIndex, 0))
                            {
                                preRenderedFramesModified = true; // Mark the modification status
                                Trace.WriteLine("Maximum pre-rendered frames set to 0 successfully for adapter " + adapterInfo.AdapterIndex + ".");
                            }
                            else
                            {
                                Trace.WriteLine("Failed to set Power Control value for adapter " + adapterInfo.AdapterIndex + ".");
                            }
                        }

                        DestroyADL();
                    }
                    else
                    {
                        Trace.WriteLine("Failed to initialize ADL.");
                    }
                }
                else
                {
                    Trace.WriteLine("Not an AMD processor. Cannot modify maximum pre-rendered frames.");
                }
            }

        
            public void SetHighPerformanceMode()
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes", "54533251-82be-4824-96c1-47b60b740d00", "High performance", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\be337238-0d82-4146-a960-4f3749d470c7", "Attributes", 2, RegistryValueKind.DWord);
            }

            public void SetPowerProfile()
            {
                Process.Start("powercfg.exe", "/s 54533251-82be-4824-96c1-47b60b740d00");
            }

            public void DisableCoolnQuiet()
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\3edd42d6-2b98-4beb-9965-3afcbcbc1425", "Attributes", 0, RegistryValueKind.DWord);
            }

            public void Optimize()
            {
               
                    SetHighPerformanceMode();
                    SetMaximumPreRenderedFramesToZero();
                    SetFlipQueueSizeToHexValue();
                    DisableCoolnQuiet();
                    SetPowerProfile();
             
            }

            public void RevertOptimizations()
            {
                if (preRenderedFramesModified)
                {
                    // Revert Maximum Pre-Rendered Frames optimization
                    // Set pre-rendered frames back to the original value or default value
                    SetMaximumPreRenderedFramesToDefault();
                    Trace.WriteLine("Reverted Maximum Pre-Rendered Frames optimization.");
                    preRenderedFramesModified = false; // Mark the modification status as reverted
                }
            }

            private void SetMaximumPreRenderedFramesToDefault()
            {
                try
                {
                    // Replace this with logic to set pre-rendered frames back to the default value
                    // You might need to read the original value before modifying it and then set it back.
                    Registry.SetValue(RegistryKeyPath, RegistryValueName, GetByteDataFromHexString("original_hex_value_here"), RegistryValueKind.Binary);
                    Trace.WriteLine("Maximum Pre-Rendered Frames set back to default value.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error setting Maximum Pre-Rendered Frames to default: " + ex.Message);
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct AdapterInfo
            {
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
                public string UDID;
                public int BusNumber;
                public int DeviceNumber;
                public int FunctionNumber;
                public int VendorID;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
                public string AdapterName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
                public string DisplayName;
                public int Present;
                public int Exist;
                public int DriverPath;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
                public string DriverPathExt;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
                public string PNPString;
                public int OSDisplayIndex;
                public int AdapterIndex; // Added AdapterIndex property

                public bool IsActive => Exist == 1 && Present == 1;
            }
        }

        public class IntelOptimizer
        {
            private const string RegistryKeyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Configuration";
            private const string RegistryValueName = "FlipQueueSize";

            private bool preRenderedFramesModified = false; // To keep track of pre-rendered frames modification status

            public void Optimize()
            {
             
                    SetPreRenderedFramesToZero();
                    SetPowerProfileToHighPerformance();
                    EnableTurboBoost();
                    Trace.WriteLine("Intel processor optimization completed.");
             
            }

            public void RevertOptimizations()
            {
                if (preRenderedFramesModified)
                {
                    // Revert Maximum Pre-Rendered Frames optimization
                    // Set pre-rendered frames back to the original value or default value
                    SetPreRenderedFramesToDefault();
                    Trace.WriteLine("Reverted Maximum Pre-Rendered Frames optimization for Intel processor.");
                    preRenderedFramesModified = false; // Mark the modification status as reverted
                }
            }

            private void SetPreRenderedFramesToDefault()
            {
                try
                {
                    // Replace this with logic to set pre-rendered frames back to the default value
                    Registry.SetValue(RegistryKeyPath, RegistryValueName, 0, RegistryValueKind.DWord);
                    Trace.WriteLine("Pre-Rendered Frames set back to default value for Intel processor.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error setting Pre-Rendered Frames to default: " + ex.Message);
                }
            }

            private void SetPreRenderedFramesToZero()
            {
                try
                {
                    Registry.SetValue(RegistryKeyPath, RegistryValueName, 0, RegistryValueKind.DWord);
                    preRenderedFramesModified = true; // Mark the modification status
                    Trace.WriteLine("Pre-Rendered Frames set to 0 for Intel processor.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error setting Pre-Rendered Frames: " + ex.Message);
                }
            }

            private void SetPowerProfileToHighPerformance()
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes", "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", "High performance", RegistryValueKind.String);
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c\5d76a2ca-e8c0-402f-a133-2158492d58ad", "Attributes", 2, RegistryValueKind.DWord);
                Trace.WriteLine("Power profile set to High Performance for Intel processor.");
            }

            private void EnableTurboBoost()
            {
                // Add logic here to enable Turbo Boost for Intel processor
                // You may need to use WMI or other methods to control Turbo Boost.
                // For example:
                // ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                // foreach (ManagementObject obj in searcher.Get())
                // {
                //     obj["TurboBoostEnabled"] = true;
                //     obj.Put();
                //     Trace.WriteLine("Turbo Boost enabled for Intel processor.");
                // }
            }

          
        }

    }


}



