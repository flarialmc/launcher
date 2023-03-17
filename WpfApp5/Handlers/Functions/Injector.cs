using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Lunar;
using static Flarial.Launcher.Functions.Windows;

namespace Flarial.Launcher.Functions
{
    public static class Injector // yes i stole from my own launcher
    {


        public static async Task Inject(string path, Label Status)
        {

            if (!File.Exists(path))
            {
                MessageBox.Show($"The file does not exist in the provided path. Path: {path}");


                return;
            }
            Utils.OpenGame();
            while (Utils.IsGameOpen() == false)
            {
                await Task.Delay(1);
            }

            Minecraft.Init();


            await ApplyAppPackages(path);



            Status.Content = "Waiting for Minecraft";

            Thread.Sleep(2000);
            await WaitForModules();



            Status.Content = "Injection begun";
            try
            {
                // Below is Kibou Injector
                /*
                var targetProcess = Minecraft.Process;
                IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);

                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                UIntPtr bytesWritten;
                WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(path), (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), out bytesWritten);
                CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero); */
                
                // This is Bari's Manual Map Injector!
                var process = Minecraft.Process;
                var dllFilePath = path;
                var flags = MappingFlags.DiscardHeaders;
                var mapper = new LibraryMapper(process, dllFilePath, flags);

                mapper.MapLibrary();

                Status.Content = "Finished injecting";
            }
            catch (Exception e)
            {
                Status.Content = "Injection failed.";
            }
        }

        private static async Task ApplyAppPackages(string path)
        {
            await Task.Run(() =>
            {
                FileInfo InfoFile = new FileInfo(path);
                FileSecurity fSecurity = InfoFile.GetAccessControl();
                fSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                InfoFile.SetAccessControl(fSecurity);
            });

            Console.WriteLine("Applied ALL_APPLICATION_PACKAGES permission to " + path);
        }

        private static async Task WaitForModules()
        {
            while (Minecraft.Process == null)
            {
                Thread.Sleep(4000);
            }

            await Task.Run(() =>
            {
                Console.WriteLine("Waiting for Minecraft to load");
                while (true)
                {
                    
                    Minecraft.Process.Refresh();
                    if (Minecraft.Process.Modules.Count > 150) break;
                    else
                        Thread.Sleep(4000);

                }
            });
            Console.WriteLine("Minecraft finished loading");
        }
    }
}
