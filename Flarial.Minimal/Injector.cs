using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Flarial.Minimal
{
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
        public static DllReturns Insert(string path)
        {
            return (DllReturns)DLLImports.AddTheDLLToTheGame(path);
        }
    }

}
