
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Flarial.Minimal
{

    public static class Minecraft
    {
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
            long GetPackageExecutionState([MarshalAs(UnmanagedType.LPWStr)] string packageFullName, IntPtr packageExecutionState);
            long RegisterForPackageStateChanges(
            [MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
            IntPtr pPackageExecutionStateChangeNotification,
            IntPtr pdwCookie);
            long UnregisterForPackageStateChanges(ulong dwCookie);
        }

        private static Process Process;

        public static void init()
        {
            var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
            IPackageDebugSettings pPackageDebugSettings = (IPackageDebugSettings)Activator.CreateInstance(
            Type.GetTypeFromCLSID(new Guid(0xb1aec16f, 0x2383, 0x4852, 0xb0, 0xe9, 0x8f, 0x0b, 0x1d, 0xc6, 0x6b, 0x4d)));
            uint count = 0, bufferLength = 0;
            GetPackagesByPackageFamily("Microsoft.MinecraftUWP_8wekyb3d8bbwe", ref count, IntPtr.Zero, ref bufferLength, IntPtr.Zero);
            IntPtr packageFullNames = Marshal.AllocHGlobal((int)(count * IntPtr.Size)),
                   buffer = Marshal.AllocHGlobal((int)(bufferLength * 2));
            GetPackagesByPackageFamily("Microsoft.MinecraftUWP_8wekyb3d8bbwe", ref count, packageFullNames, ref bufferLength, buffer);
            for (int i = 0; i < count; i++)
            {
                pPackageDebugSettings.EnableDebugging(Marshal.PtrToStringUni(Marshal.ReadIntPtr(packageFullNames)), null, null);
                packageFullNames += IntPtr.Size;
            }
            Marshal.FreeHGlobal(packageFullNames);
            Marshal.FreeHGlobal(buffer);

            if (mcIndex.Length > 0)
            {
                Process = mcIndex[0];

            }
        }

        public static async Task WaitForModules()
        {
            while (Process == null)
            {
                init();
                await Task.Delay(100);
            }

            while (true)
            {
                Process.Refresh();
                if (Process.Modules.Count > 155)
                    break;

                await Task.Delay(100);
            }
        }
    }

    public enum DllReturns
    {
        SUCCESS = 0,
        ERROR_PROCESS_NOT_FOUND = 1,
        ERROR_PROCESS_OPEN = 2,
        ERROR_ALLOCATE_MEMORY = 3,
        ERROR_WRITE_MEMORY = 4,
        ERROR_GET_PROC_ADDRESS = 5,
        ERROR_CREATE_REMOTE_THREAD = 6,
        ERROR_WAIT_FOR_SINGLE_OBJECT = 7,
        ERROR_VIRTUAL_FREE_EX = 8,
        ERROR_CLOSE_HANDLE = 9,
        ERROR_UNKNOWN = 10,
        ERROR_NO_PATH = 11,
        ERROR_NO_ACCESS = 12,
        ERROR_NO_FILE = 13
    }

    static class DLLImports
    {

        [DllImport("dont.delete", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AddTheDLLToTheGame(string path);
    }

    public class Insertion
    {
        public static DllReturns Insert(string path)
        {
            return (DllReturns)DLLImports.AddTheDLLToTheGame(path);
        }
    }

}
