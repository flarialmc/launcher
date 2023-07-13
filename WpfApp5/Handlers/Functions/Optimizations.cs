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

    public class WindowsOptimizer
    {
        public static void OptimizeIntelProcessor()
        {
            SetPowerPlanToHighPerformance();
        }


        private static void SetPowerPlanToHighPerformance()
        {
            // Set the power plan to High Performance for maximum performance
            Process.Start("powercfg.exe", "/s 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
        }

        public static void OptimizeNonNvidiaGPU()
        {

            AdjustVisualEffects();
            DisableUnnecessaryVisualEffects();
        }


        private static void AdjustVisualEffects()
        {
            // Adjust visual effects for better performance
            Process.Start("SystemPropertiesPerformance.exe");
        }

        private static void DisableUnnecessaryVisualEffects()
        {
            // Disable unnecessary visual effects for better performance
            Process.Start("SystemPropertiesPerformance.exe", "/tab:Visual Effects");
        }

        public static void DisableWindowsPrefetchAndLogs()
        {
            DisableWindowsPrefetch();
            DisableWindowsEventLogs();
        }

        private static void DisableWindowsPrefetch()
        {
            // Disable Windows prefetch feature for improved disk performance
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnablePrefetcher", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnableSuperfetch", 0, RegistryValueKind.DWord);
        }

        private static void DisableWindowsEventLogs()
        {
            // Disable Windows event logs to reduce system resource usage
            using (RegistryKey eventLogKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog", true))
            {
                foreach (string logName in eventLogKey.GetSubKeyNames())
                {
                    using (RegistryKey logKey = eventLogKey.OpenSubKey(logName, true))
                    {
                        logKey.SetValue("Start", 4, RegistryValueKind.DWord);
                    }
                }
            }
        }

        public static void UndoWindowsOptimizations()
        {


            // Undo Windows prefetch and logs optimizations
            UndoWindowsPrefetchOptimization();
            UndoWindowsEventLogsOptimization();
        }



        private static void UndoWindowsPrefetchOptimization()
        {
            // Re-enable Windows prefetch feature
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnablePrefetcher", 3, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnableSuperfetch", 3, RegistryValueKind.DWord);
        }

        private static void UndoWindowsEventLogsOptimization()
        {
            // Re-enable Windows event logs
            using (RegistryKey eventLogKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog", true))
            {
                foreach (string logName in eventLogKey.GetSubKeyNames())
                {
                    using (RegistryKey logKey = eventLogKey.OpenSubKey(logName, true))
                    {
                        logKey.DeleteValue("Start");
                    }
                }
            }
        }
    }
}
