using Flarial.Launcher.Functions;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation;
using Windows.Management.Deployment;
using Application = System.Windows.Application;

namespace Flarial.Launcher.Managers
{
    public class VersionManagement
    {
        public static string launcherPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flarial", "Launcher");

      

        static void EnableDeveloperMode()
        {
            const string developerModeKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock";
            const string developerModeValueName = "AllowDevelopmentWithoutDevLicense";

            // Set the value to 1 to enable Developer Mode
            Registry.SetValue(developerModeKey, developerModeValueName, 1, RegistryValueKind.DWord);
        }
        public static string ExtractUrl(string jsonString)
        {
            // Parse the JSON string into a JObject
            JObject jsonObject = JObject.Parse(jsonString);

            // Get the value of the "url" property
            string url = (string)jsonObject["url"];

            return url;
        }

        public static async Task<string> GetVersionLinkAsync(string version)
        {
            string result = "";
            WebClient webClient = new WebClient();
            WebClient versionsWc = new WebClient();
            versionsWc.DownloadFile("https://cdn-c6f.pages.dev/launcher/VersionDl.txt", "VersionDl.txt");


            string[] rawVersions = File.ReadAllLines("VersionDl.txt");
            
            foreach (string combined in rawVersions)
            {
                string[] split = combined.Split(' ');
                if (split[0].Contains(version))
                {
                    result = ExtractUrl(webClient.DownloadStringTaskAsync(new Uri(split[1])).Result);
                }
            }
            
            Trace.WriteLine(version);

            return result;
        }

        public static async Task<bool> DownloadApplication(string version)
        {
            string path = Path.Combine(launcherPath, "Versions", $"Minecraft{version}.Appx");
            string url = await GetVersionLinkAsync(version);

            if (File.Exists(path))
            {
                Trace.WriteLine("File already exists, download skipped.");
                return true;
            }

            if (!string.IsNullOrEmpty(url))
            {
                Trace.WriteLine($"Got downloadable URL for Minecraft version {version}: {url}");
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += DownloadProgressCallback;
                    await webClient.DownloadFileTaskAsync(new Uri(url), path);
                    Trace.WriteLine("Download succeeded!");
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CustomDialogBox MessageBox = new CustomDialogBox("Download failed",
                        $"{url} issue with the URL. Please report this in our discord.", "MessageBox");
                    MessageBox.ShowDialog();
                });

                return false;
            }

            return true;
        }

        static async Task<bool> RunPowerShellAsAdminAsync(string command)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            bool success = false;

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                Verb = "runas", // This is what triggers running as admin
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process
            {
                StartInfo = psi
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Trace.WriteLine(e.Data); // Log to Trace
                    Trace.WriteLine(e.Data); // Print to console
                    LogProgress(e.Data); // Check and log progress if found
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Trace.WriteLine(e.Data); // Log error to Trace
                    Trace.WriteLine(e.Data); // Print error to console
                }
            };

            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) =>
            {
                success = process.ExitCode == 0; // Check if the process succeeded
                taskCompletionSource.SetResult(success); // Complete the task with success status
                process.Dispose(); // Clean up the process resources
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await taskCompletionSource.Task; // Await the task completion
            return success; // Return the success status
        }

        static void LogProgress(string output)
        {
            // Example: Assuming progress indicators start with "Progress:"
            if (output.StartsWith("Progress:", StringComparison.OrdinalIgnoreCase))
            {
                Trace.WriteLine($"Progress: {output.Substring("Progress:".Length)}");
                // Log or process the progress information here
            }
        }
        public static async Task<bool> InstallAppBundle(string dir)
        {
            Trace.WriteLine("called installappbundle");
            Trace.WriteLine(dir);

            try
            {
                var packageUri = new Uri(Path.Combine(dir, "AppxManifest.xml"));
                Trace.WriteLine(packageUri.ToString());

                File.Delete(Path.Combine(dir, "AppxSignature.p7x"));

                MainWindow.progressPercentage = 100;
                MainWindow.progressType = "Installing";

                
                //    var progress = new Progress<DeploymentProgress>(ReportProgress);

           return  await   RunPowerShellAsAdminAsync($"Add-AppxPackage -ForceUpdateFromAnyVersion -Path {dir}\\AppxManifest.xml -Register");


                //var registerPackageOperation = packageManager.RegisterPackageByUriAsync(packageUri, new RegisterPackageOptions() { DeveloperMode = true });
                //registerPackageOperation.Progress += (sender, args) => ReportProgress(args);

                //    var registerPackageTask = registerPackageOperation.AsTask();

                //    await registerPackageTask;

                //    if (registerPackageTask.Status == TaskStatus.RanToCompletion)
                //    {

                //        Trace.WriteLine("Package installation succeeded!");
                //        return true;
                //    }
                //    else
                //    {
                //        Application.Current.Dispatcher.Invoke(() =>
                //        {
                //            CustomDialogBox MessageBox =
                //                new CustomDialogBox("Failed", "Failed to install package.", "MessageBox");
                //            MessageBox.ShowDialog();
                //        });
                //        return false;
                //    }
            }
            catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CustomDialogBox MessageBox = new CustomDialogBox("Failed",
                            $"RegisterPackageAsync failed, error message: {ex.Message}\nFull Stacktrace: {ex.ToString()}",
                            "MessageBox");
                        MessageBox.ShowDialog();


                    });
                    return false;
                }
            }
        private static void ReportProgress(DeploymentProgress progress)
        {
            // Report the progress of the deployment
            if (progress.percentage < 100)
            {
                MainWindow.progressPercentage = (int)progress.percentage;

                MainWindow.progressType = "Installing";

                Trace.WriteLine(progress.percentage + "% yas");
            }
        }

        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            MainWindow.progressType = "download";

            if (e.ProgressPercentage != 100)
                MainWindow.progressPercentage = e.ProgressPercentage;
            else Trace.WriteLine("nigger");

            MainWindow.progressBytesReceived = e.BytesReceived;
            MainWindow.progressBytesTotal = e.TotalBytesToReceive;
        }

        public static async Task<bool> RemoveMinecraftPackage()
        {
            
            try
            {
                var packageManager = new PackageManager();
                
                var removePackageOperation = packageManager.RemovePackageAsync(Minecraft.Package.Id.FullName, RemovalOptions.RemoveForAllUsers);

                await removePackageOperation;
                Trace.WriteLine("Yea it removed");
                if (removePackageOperation.Status == AsyncStatus.Completed)
                {

                    Trace.WriteLine("Package removal succeeded!");
                    return true;
                }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CustomDialogBox MessageBox =
                            new CustomDialogBox("Failed", "Failed to remove package.", "MessageBox");
                        MessageBox.ShowDialog();
                    });
                    return false;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CustomDialogBox MessageBox = new CustomDialogBox("Failed",
                        $"RemovePackageAsync failed, error message: {ex.Message}\nFull Stacktrace: {ex.ToString()}",
                        "MessageBox");
                    MessageBox.ShowDialog();
                });
                return false;
            }
        }

        public static async Task ExtractAppxAsync(string appxFilePath, string outputFolderPath, string version)
        {
            int totalEntries = 0;
            int currentEntry = 0;

            if (!Directory.Exists(outputFolderPath))
            {
                using (ZipArchive archive = ZipFile.OpenRead(appxFilePath))
                {
                    totalEntries = archive.Entries.Count;

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryOutputPath = Path.Combine(outputFolderPath, entry.FullName);
                        string entryDirectory = Path.GetDirectoryName(entryOutputPath);

                        if (!string.IsNullOrEmpty(entryDirectory))
                        {
                            Directory.CreateDirectory(entryDirectory);
                        }

                        if (!entry.FullName.EndsWith("/"))
                        {
                            await Task.Run(() => entry.ExtractToFile(entryOutputPath, true));
                        }

                        currentEntry++;
                        MainWindow.progressType = "Extracting";
                        MainWindow.progressBytesTotal = totalEntries;
                        MainWindow.progressBytesReceived = currentEntry;
                        MainWindow.progressPercentage = (int)((float)currentEntry / totalEntries * 100);
                    }
                }

                WebClient webClient = new WebClient();

                if (File.Exists(Path.Combine(launcherPath, "Versions", version, "UAP.Assets", "minecraft", "icons", "MCSplashScreen.scale-200.png")))
                    File.Delete(Path.Combine(launcherPath, "Versions", version, "UAP.Assets", "minecraft", "icons", "MCSplashScreen.scale-200.png"));

                if (File.Exists(Path.Combine(launcherPath, "Versions", version, "data", "resource_packs", "vanilla", "textures",
                       "ui", "title.png")))
                    File.Delete(Path.Combine(launcherPath, "Versions", version, "data", "resource_packs", "vanilla", "textures",
                        "ui", "title.png"));

                using (StreamWriter writer = new StreamWriter(Path.Combine(launcherPath, "Versions", version, "data", "resource_packs", "vanilla", "splashes.json"), false))
                {
                    string jsonString = "{ \"splashes\": [ \"§4Flarial §fMan!\" ] }";
                    writer.WriteLine(jsonString);
                }

                await webClient.DownloadFileTaskAsync(new Uri("https://cdn-c6f.pages.dev/assets/flarial-title.png"),
                    Path.Combine(launcherPath, "Versions", version, "data", "resource_packs", "vanilla", "textures",
                        "ui", "title.png"));
                await webClient.DownloadFileTaskAsync(new Uri("https://cdn-c6f.pages.dev/assets/flarial_mogang.png"),
                    Path.Combine(launcherPath, "Versions", version, "UAP.Assets", "minecraft", "icons", "MCSplashScreen.scale-200.png"));
            }
        }


        public static async Task<bool> InstallMinecraft(string version)
        {
            if (!Utils.IsDeveloperModeEnabled())
            {
                // Enable Developer Mode
                EnableDeveloperMode();
                Trace.WriteLine("Developer Mode has been enabled.");
            }
            else
            {
                Trace.WriteLine("Developer Mode is already enabled on your system.");
                // Continue with the rest of your application logic here.
            }


            MainWindow.progressPercentage = 0;
            string path = Path.Combine(launcherPath, "Versions", $"Minecraft{version}.Appx");

            bool ello = await DownloadApplication(version);

            if (ello)
            {
                Trace.WriteLine("Finished downloading the specified version's application bundle.");

                Minecraft.Init();

                if (Minecraft.Package != null)
                {
                    if (await BackupManager.GetConfig("temp") == null) 
                    await BackupManager.CreateBackup("temp");
                    Trace.WriteLine("Uninstalling current Minecraft version.");
                    await RemoveMinecraftPackage();
                }
                
                Trace.WriteLine("Uninstalled.");

                if (!Directory.Exists(Path.Combine(launcherPath, "Versions", version)))
                {
                    Trace.WriteLine("Starting extract.");
                    await ExtractAppxAsync(path, Path.Combine(launcherPath, "Versions", version), version);
                }
                else
                {
                    Trace.WriteLine("The folder exists");
                }


                Trace.WriteLine("Deploying Minecraft's Application Bundle.");

                if (await InstallAppBundle(Path.Combine(launcherPath, "Versions", version)) == false)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CustomDialogBox MessageBox = new CustomDialogBox("Failed", "Failed to install.", "MessageBox");
                        MessageBox.ShowDialog();
                    });
                    
                    return false;
                }

                await Task.Delay(1);

                if (await BackupManager.GetConfig("temp") != null)
                {
                    Trace.WriteLine("Temporary backup found, now loading.");
                    await BackupManager.LoadBackup("temp");

                    Trace.WriteLine("Temporary backup loaded, now deleting.");
                    await BackupManager.DeleteBackup("temp");
                }

                Trace.WriteLine("Installation complete.");


                Application.Current.Dispatcher.Invoke(() =>
                {
                    new CustomDialogBox("Restart the Launcher", "Please restart the launcher for it to be able to install new patches.", "MessageBox").ShowDialog();
                    
                });

                await Task.Delay(2000);
                Environment.Exit(5);
            }
            return ello;
        }
    }
}