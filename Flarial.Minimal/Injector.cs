using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Flarial.Minimal
{
    public static class Minecraft
    {
        private static Process Process;

        public static void init()
        {
            var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
            if (mcIndex.Length > 0)
            {
                Process = mcIndex[0];

            }
        }
        
        public static async Task WaitForModules()
        {
            while (Process == null)
            {
                await Task.Delay(4000);
            }

            while (true)
            {
                Process.Refresh();
                if (Process.Modules.Count > 155)
                    break;
                
                await Task.Delay(4000);
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

        [DllImport("DllUtil.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AddTheDLLToTheGame(string path);
    }

    public class Insertion
    {
        public static async Task<DllReturns> Insert(string path)
        {
            Minecraft.init();
            await Minecraft.WaitForModules();
            return (DllReturns)DLLImports.AddTheDLLToTheGame(path);
        }
    }

}
