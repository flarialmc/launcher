using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Flarial.Launcher.Functions.Windows;

namespace Flarial.Launcher.Functions
{
    public static class Injector // yes i stole from my own launcher
    {


        public static async Task Inject(string path, Label Status)
        {
            if (!await Task.Run(() => File.Exists(path)))
            {
                MessageBox.Show("The file does not exist in the provided path.");
                return;
            }

            Utils.OpenGame();
            while (!Utils.IsGameOpen())
            {
                await Task.Delay(1);
            }

            Minecraft.Init();

            await ApplyAppPackages(path);

            Status.Content = "Waiting for Minecraft";

            // Wait For MC to load
            await WaitForModules();

            Status.Content = "Injection begun";
            Trace.WriteLine($"Injection begun {path}");

            try
            {
                var targetProcess = Minecraft.Process;
                IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);

                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                UIntPtr bytesWritten;
                WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(path), (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), out bytesWritten);
                CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);

                Status.Content = "Finished installing client.";
            }
            catch (Exception e)
            {
                Status.Content = "Client failed.";
            }
        }

        private static async Task ApplyAppPackages(string path)
        {
            FileInfo InfoFile = new FileInfo(path);
            FileSecurity fSecurity = InfoFile.GetAccessControl();
            fSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            InfoFile.SetAccessControl(fSecurity);

            Console.WriteLine("Applied ALL_APPLICATION_PACKAGES permission to " + path);
        }

        private static async Task WaitForModules()
        {
            while (Minecraft.Process == null)
            {
                await Task.Delay(4000);
            }

            Console.WriteLine("Waiting for Minecraft to load");
            while (true)
            {
                Minecraft.Process.Refresh();
                if (Minecraft.Process.Modules.Count > 155)
                    break;
                else
                    await Task.Delay(4000);
            }
            Console.WriteLine("Minecraft finished loading");
        }

    }
}
