using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Flarial.Launcher.Functions.Windows;

namespace Flarial.Launcher.Functions
{
    public static class Injector // yes i stole from my own launcher
    {


        public static async Task Inject(string path)
        {
            await ApplyAppPackages(path);




            //Wait For MC to load
            await WaitForModules();


            await Task.Run(() =>
            {
                Trace.WriteLine("Injecting " + path);
                try
                {
                    var targetProcess = Minecraft.Process;
                    var procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION |
                        PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                        false, targetProcess.Id);

                    var loadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                    var allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero,
                        (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT
                        | MEM_RESERVE, PAGE_READWRITE);

                    WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(path),
                        (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), out _);
                    CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddress,
                        allocMemAddress, 0, IntPtr.Zero);

                    Trace.WriteLine("Finished injecting");
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Injection failed. Exception: " + e);
                }
            });
        }

        private static async Task ApplyAppPackages(string path)
        {
            await Task.Run(() =>
            {
                var infoFile = new FileInfo(path);
                var fSecurity = infoFile.GetAccessControl();
                fSecurity.AddAccessRule(
                    new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"),
                    FileSystemRights.FullControl, InheritanceFlags.None,
                    PropagationFlags.NoPropagateInherit, AccessControlType.Allow));

                infoFile.SetAccessControl(fSecurity);
            });

            Console.WriteLine("Applied ALL_APPLICATION_PACKAGES permission to " + path);
        }

        private static async Task WaitForModules()
        {
            while (Minecraft.Process == null)
            {
                await Task.Delay(1);
            }

            await Task.Run(() =>
            {
                Console.WriteLine("Waiting for Minecraft to load");
                while (true)
                {
                    Minecraft.Process.Refresh();
                    if (Minecraft.Process.Modules.Count > 160) break;
                    Thread.Sleep(4000);
                }
            });
            Console.WriteLine("Minecraft finished loading");
        }
    }
}
