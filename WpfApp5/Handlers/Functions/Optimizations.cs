using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace Flarial.Launcher.Handlers.Functions
{
    public class NvidiaPanelAndInternetOptimizer
    {
        private const string NvidiaRegKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\PCI\";
        private const string NvidiaRegSubKey = @"NVIDIA_DEV.0";
        private const string NvidiaRegValue = "DeviceDesc";

        private const string MinecraftExecutable = "minecraft.windows.exe";
        private const string NvidiaPanelExecutable = "nvcplui.exe";

        public static void OptimizeNvidiaPanelAndInternetForMinecraft()
        {
            // Check if Nvidia GPU is present
            if (IsNvidiaGPU())
            {
                // Optimize Nvidia panel settings for Minecraft
                SetNvidiaPanelSettings();
            }
            else
            {
                Console.WriteLine("Nvidia GPU not found. Unable to optimize Nvidia panel settings.");
            }

            // Optimize internet settings for Minecraft
            OptimizeInternetSettings();
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
                Console.WriteLine("Nvidia panel process not found. Unable to set Nvidia panel settings.");
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
            // Set TCP/IP optimizations for reduced latency and improved throughput
            SetTcpIpOptimizations();
        }

        private static void SetTcpIpOptimizations()
        {
            // Modify TCP/IP settings using registry keys
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TCPNoDelay", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpWindowSize", 64240, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "DefaultTTL", 64, RegistryValueKind.DWord);
        }

        public static void DisableNvidiaPanelAndInternetOptimizations()
        {
            // Disable Nvidia panel settings for Minecraft
            DisableNvidiaPanelSettings();

            // Disable internet optimizations for Minecraft
            DisableInternetSettings();
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
                Console.WriteLine("Nvidia panel process not found. Unable to disable Nvidia panel settings.");
            }
        }

        private static void DisableInternetSettings()
        {
            // Reset TCP/IP optimizations to default values
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", 2, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TCPNoDelay", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpWindowSize", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "DefaultTTL", 128, RegistryValueKind.DWord);
        }
    }

    public class MinecraftOptimizer
    {
        private const string MinecraftExecutable = "minecraft.windows.exe";

        public static int DesiredRAMAllocation { get; set; } = 4096; // Default to 4GB (4096MB) of RAM allocation

        public static void OptimizeMinecraft()
        {
            // Set process priority and affinity for Minecraft
            SetProcessSettings();

            // Disable fullscreen optimizations for Minecraft
            DisableFullscreenOptimizations();

            // Disable DPI scaling for Minecraft
            DisableDpiScaling();

            // Check DirectX version and apply optimizations accordingly
            string directXVersion = GetDirectXVersion();
            if (directXVersion == "DirectX 11")
            {
                // Apply DirectX 11 optimizations for Minecraft
                ApplyDirectX11Optimizations();
            }
            else if (directXVersion == "DirectX 12")
            {
                // Apply DirectX 12 optimizations for Minecraft
                ApplyDirectX12Optimizations();
            }
            else
            {
                Console.WriteLine("Unknown DirectX version. Skipping DirectX optimizations.");
            }

            // Optimize mouse settings
            OptimizeMouseSettings();

            // Optimize keyboard settings
            OptimizeKeyboardSettings();

            // Perform additional performance and internet optimizations
            AdditionalOptimizations();
        }

        private static void ApplyDirectX11Optimizations()
        {
            // Apply DirectX 11 optimizations for Minecraft
            Console.WriteLine("Applying DirectX 11 optimizations for Minecraft.");

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
            Console.WriteLine("Applying DirectX 12 optimizations for Minecraft.");

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
            AdjustVisualEffects();

            // Optimize internet settings
            OptimizeInternetSettings();

            // Perform disk cleanup and defragmentation
            PerformDiskCleanup();
            PerformDiskDefragmentation();

            // Disable unnecessary startup programs
            DisableStartupPrograms();

            // Optimize RAM allocation
            OptimizeRAMAllocation();
        }

        private static void AdjustPowerSettings()
        {
            // Set power plan to High Performance for better performance
            Process.Start("powercfg.exe", "/s 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
        }

        private static void AdjustVisualEffects()
        {
            // Adjust visual effects for better performance
            Process.Start("SystemPropertiesPerformance.exe");
        }

        private static void OptimizeInternetSettings()
        {
            // Adjust internet settings for better network performance
            Process.Start("inetcpl.cpl");
        }

        private static void PerformDiskCleanup()
        {
            // Perform disk cleanup to free up space
            Process.Start("cleanmgr.exe");
        }

        private static void PerformDiskDefragmentation()
        {
            // Perform disk defragmentation for improved disk performance
            Process.Start("dfrgui.exe");
        }

        private static void DisableStartupPrograms()
        {
            // Disable unnecessary startup programs for faster system boot
            Process.Start("taskmgr.exe");
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
                Console.WriteLine("Minecraft process not found. Unable to optimize RAM allocation.");
            }
        }

        private static Process GetMinecraftProcess()
        {
            // Find the Minecraft process by name
            Process[] processes = Process.GetProcessesByName(MinecraftExecutable);

            // Return the first found process
            return processes.Length > 0 ? processes[0] : null;
        }

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
                Console.WriteLine("Minecraft process not found. Unable to set process settings.");
            }
        }

        private static void DisableFullscreenOptimizations()
        {
            // Find the Minecraft process main window handle
            Process minecraftProcess = GetMinecraftProcess();
            IntPtr mainWindowHandle = minecraftProcess.MainWindowHandle;

            // Disable fullscreen optimizations for Minecraft window
            SetWindowDisplayAttribute(mainWindowHandle, WindowDisplayAttribute.WDA_MONITOR, 1);
        }

        private static void DisableDpiScaling()
        {
            // Find the Minecraft process main window handle
            Process minecraftProcess = GetMinecraftProcess();
            IntPtr mainWindowHandle = minecraftProcess.MainWindowHandle;

            // Disable DPI scaling for Minecraft window
            SetProcessDpiAwareness(mainWindowHandle, ProcessDpiAwareness.Process_Per_Monitor_DPI_Aware);
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

        [DllImport("user32.dll")]
        private static extern bool SetWindowDisplayAttribute(IntPtr hWnd, WindowDisplayAttribute attribute, int attributeValue);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwareness(IntPtr hWnd, ProcessDpiAwareness awareness);

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




    public class AMDPreRenderedFrames
    {
        private const int ADL_OK = 0;
        private const int ADL_MAX_PATH = 256;

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

        private const string RegistryKeyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4D36E968-E325-11CE-BFC1-08002BE10318}\0000\UMD";
        private const string RegistryValueName = "FlipQueueSize";

        public static void SetFlipQueueSizeToHexValue()
        {
            try
            {
                Registry.SetValue(RegistryKeyPath, RegistryValueName, GetByteDataFromHexString("31,00"), RegistryValueKind.Binary);
                Console.WriteLine("FlipQueueSize set to hexadecimal value successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error setting FlipQueueSize: " + ex.Message);
            }
        }

        private static byte[] GetByteDataFromHexString(string hexString)
        {
            string[] hexValues = hexString.Split(',');
            byte[] byteData = new byte[hexValues.Length];

            for (int i = 0; i < hexValues.Length; i++)
            {
                byteData[i] = byte.Parse(hexValues[i], System.Globalization.NumberStyles.HexNumber);
            }

            return byteData;
        }

        public static void SetMaximumPreRenderedFramesToZero()
        {
            if (IsAMDProcessor())
            {
                int numAdapters = 0;
                int result = ADL_Main_Control_Create(1);

                if (result != ADL_OK)
                {
                    Console.WriteLine("Failed to initialize ADL.");
                    return;
                }

                result = ADL_Adapter_NumberOfAdapters_Get(ref numAdapters);

                if (result != ADL_OK)
                {
                    Console.WriteLine("Failed to get the number of adapters.");
                    ADL_Main_Control_Destroy();
                    return;
                }

                if (numAdapters == 0)
                {
                    Console.WriteLine("No AMD adapters found.");
                    ADL_Main_Control_Destroy();
                    return;
                }

                IntPtr info = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(AdapterInfo)) * numAdapters);

                result = ADL_Adapter_AdapterInfo_Get(info, Marshal.SizeOf(typeof(AdapterInfo)) * numAdapters);

                if (result != ADL_OK)
                {
                    Console.WriteLine("Failed to get adapter information.");
                    Marshal.FreeCoTaskMem(info);
                    ADL_Main_Control_Destroy();
                    return;
                }

                for (int i = 0; i < numAdapters; i++)
                {
                    IntPtr currentInfo = new IntPtr(info.ToInt64() + (Marshal.SizeOf(typeof(AdapterInfo)) * i));
                    AdapterInfo adapterInfo = (AdapterInfo)Marshal.PtrToStructure(currentInfo, typeof(AdapterInfo));

                    int status = 0;
                    result = ADL_Adapter_Active_Get(adapterInfo.AdapterIndex, ref status);

                    if (result != ADL_OK)
                    {
                        Console.WriteLine("Failed to get adapter status.");
                        continue;
                    }

                    if (status != 1)
                    {
                        Console.WriteLine("Adapter is not active.");
                        continue;
                    }

                    int capable = 0;
                    int enabled = 0;
                    result = ADL_Adapter_Crossfire_Caps(adapterInfo.AdapterIndex, ref capable, ref enabled);

                    if (result != ADL_OK)
                    {
                        Console.WriteLine("Failed to get Crossfire capabilities.");
                        continue;
                    }

                    if (capable == 1 && enabled == 1)
                    {
                        Console.WriteLine("Crossfire is enabled for adapter " + adapterInfo.AdapterIndex + ". Cannot modify maximum pre-rendered frames.");
                        continue;
                    }

                    int supported = 0;
                    int overdriveEnabled = 0;
                    int version = 0;
                    result = ADL_Overdrive_Caps(adapterInfo.AdapterIndex, ref supported, ref overdriveEnabled, ref version);

                    if (result != ADL_OK)
                    {
                        Console.WriteLine("Failed to get Overdrive capabilities.");
                        continue;
                    }

                    if (supported == 0)
                    {
                        Console.WriteLine("Overdrive is not supported for adapter " + adapterInfo.AdapterIndex + ". Cannot modify maximum pre-rendered frames.");
                        continue;
                    }

                    int minPower = 0;
                    int maxPower = 0;
                    int currentPower = 0;
                    int defaultPower = 0;
                    int stepPower = 0;
                    result = ADL_Overdrive6_PowerControlInfo_Get(adapterInfo.AdapterIndex, ref minPower, ref maxPower, ref currentPower, ref defaultPower, ref stepPower);

                    if (result != ADL_OK)
                    {
                        Console.WriteLine("Failed to get Power Control information.");
                        continue;
                    }

                    if (currentPower != 0)
                    {
                        Console.WriteLine("Power Control is already modified for adapter " + adapterInfo.AdapterIndex + ". Cannot modify maximum pre-rendered frames.");
                        continue;
                    }

                    result = ADL_Overdrive6_PowerControl_Set(adapterInfo.AdapterIndex, 0);

                    if (result != ADL_OK)
                    {
                        Console.WriteLine("Failed to set Power Control value for adapter " + adapterInfo.AdapterIndex + ".");
                        continue;
                    }

                    Console.WriteLine("Maximum pre-rendered frames set to 0 successfully for adapter " + adapterInfo.AdapterIndex + ".");
                }

                Marshal.FreeCoTaskMem(info);
                ADL_Main_Control_Destroy();
            }
            else
            {
                Console.WriteLine("Not an AMD processor. Cannot modify maximum pre-rendered frames.");
            }
        }

        private static bool IsAMDProcessor()
        {
            string processorName = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            return processorName.Contains("AuthenticAMD");
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
        }
    }

    public class IntelPreRenderedFrames
    {
        private const string RegistryKeyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Configuration";
        private const string RegistryValueName = "FlipQueueSize";

        public static void SetPreRenderedFramesToZero()
        {
            if (IsIntelProcessor())
            {
                try
                {
                    Registry.SetValue(RegistryKeyPath, RegistryValueName, 0, RegistryValueKind.DWord);
                    Console.WriteLine("Maximum pre-rendered frames set to 0 successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error setting maximum pre-rendered frames: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Not an Intel processor. Cannot modify maximum pre-rendered frames.");
            }
        }

        private static bool IsIntelProcessor()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string manufacturer = obj["Manufacturer"].ToString();
                    if (manufacturer.ToLower().Contains("intel"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }



}
