using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Windows.Foundation;
using Flarial.Launcher.Services.SDK;
using Windows.Management.Deployment;
using Windows.Storage;
using Flarial.Launcher.Styles;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System.Xml;
using Flarial.Launcher.Services.Core;

namespace Flarial.Launcher.Managers;

public static class VersionManagement
{
    public static string launcherPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flarial", "Launcher");

    public static string ExtractUrl(string jsonString)
    {
        // Parse the JSON string into a JObject
        JObject jsonObject = JObject.Parse(jsonString);

        // Get the value of the "url" property
        string url = (string)jsonObject["url"];

        return url;
    }

    public static async Task<bool> DownloadApplication(string url, string version)
    {
        string path = Path.Combine(launcherPath, "Versions", $"Minecraft{version}.Appx");

        WebClient webClient = new();

        if (File.Exists(path))
        {
            Trace.WriteLine("File already exists, download skipped.");
            return true;
        }

        if (!string.IsNullOrEmpty(url))
        {
            Trace.WriteLine($"Got downloadable URL for Minecraft version {version}: {url}");
            webClient.DownloadProgressChanged += DownloadProgressCallback;
            await webClient.DownloadFileTaskAsync(url, path);
            Trace.WriteLine("Download succeeded!");
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox($"Download failed: {url} issue with the URL");
            });

            return false;
        }

        return true;
    }


    public static List<string> GetDependenciesFromManifest(string manifestPath)
    {
        var dependencies = new List<string>();
        XmlDocument doc = new XmlDocument();
        doc.Load(manifestPath);

        XmlNodeList dependencyNodes = doc.GetElementsByTagName("PackageDependency");

        foreach (XmlNode node in dependencyNodes)
        {
            var packageName = node.Attributes["Name"]?.Value;
            if (!string.IsNullOrEmpty(packageName))
            {
                dependencies.Add(packageName);
            }
        }

        return dependencies;
    }


    public static bool IsDependencyInstalled(string packageName)
    {
        PackageManager packageManager = new();
        var installedPackages = packageManager.FindPackagesForUser(string.Empty);

        foreach (var package in installedPackages)
        {
            if (package.Id.FullName.Contains(packageName))
            {
                return true;
            }
        }

        return false;
    }

    public static bool AreDependenciesInstalled(string manifestPath)
    {
        var dependencies = GetDependenciesFromManifest(manifestPath);

        foreach (var dependency in dependencies)
        {
            if (!IsDependencyInstalled(dependency))
            {
                Trace.WriteLine($"Dependency {dependency} is not installed.");
                return false;
            }
        }

        Trace.WriteLine("All dependencies are installed.");
        return true;
    }

    public static async Task<bool> InstallAppBundle(string dir)
    {
        Trace.WriteLine("called installappbundle");
        Trace.WriteLine(dir);



        Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.IsLaunchEnabled = false;
        });


        CloseInstances();
        await DeleteAppDataFiles();



        var packageUri = new Uri(Path.Combine(dir, "AppxManifest.xml"));
        Trace.WriteLine(packageUri.ToString());

        File.Delete(Path.Combine(dir, "AppxSignature.p7x"));

        MainWindow.progressPercentage = 100;
        MainWindow.progressType = "Installing";

        var manifestPath = Path.Combine(dir, "AppxManifest.xml");
        var escapedPath = manifestPath.Replace("\"", "\\\"");
        Trace.WriteLine(escapedPath);

        try
        {

            var registerPackageOperation = MinecraftGame.PackageManager.RegisterPackageByUriAsync(new Uri(escapedPath), new RegisterPackageOptions { DeveloperMode = true, ForceAppShutdown = true, ForceUpdateFromAnyVersion = true, ForceTargetAppShutdown = true });
            var registerPackageTask = registerPackageOperation.AsTask();

            await registerPackageTask;

            if (registerPackageTask.Status == TaskStatus.RanToCompletion)
            {

                Trace.WriteLine("Package installation succeeded!");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.IsLaunchEnabled = true;
                });
                return true;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox("Failed to install. Join our discord for help: https://flarial.xyz/discord");
                MainWindow.CreateMessageBox("Your data and worlds are saved at %localappdata%/Flarial/Launcher.");
                Trace.WriteLine($"RegisterPackageAsync failed, {registerPackageOperation.GetResults().ExtendedErrorCode.Message}");
            });
            return false;
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Trace.WriteLine($"RegisterPackageAsync failed, {ex.Message}");
                MessageBox.Show($"RegisterPackageAsync failed, {ex.Message}", "ERROR!");
            });
            return false;
        }
    }

    private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
    {
        MainWindow.progressType = "download";
        MainWindow.isDownloadingVersion = true;

        if (e.ProgressPercentage != 100)
            MainWindow.progressPercentage = e.ProgressPercentage;
        else
        {
            MainWindow.isDownloadingVersion = false;
            Trace.WriteLine("done downloading");
        }

        MainWindow.progressBytesReceived = e.ProgressPercentage;
    }

    public static async Task<bool> RemoveMinecraftPackage()
    {

        CloseInstances();

        try
        {

            var removePackageOperation = MinecraftGame.PackageManager.RemovePackageAsync(
                MinecraftGame.Package.Id.FullName,
                RemovalOptions.RemoveForAllUsers
            );
            await removePackageOperation;
            Trace.WriteLine("Yea it removed");
            if (removePackageOperation.Status == AsyncStatus.Completed)
            {

                Trace.WriteLine("Package removal succeeded!");
                await DeleteAppDataFiles();
                return true;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox("Failed to remove package.");
            });
            return false;
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CreateMessageBox($"RemovePackageAsync failed, {ex.Message}");
            });
            MessageBox.Show($"Failed to remove package, {ex.Message}", "ERROR!");
            return false;
        }
    }

    public static async Task<bool> ExtractAppxAsync(string appxFilePath, string outputFolderPath, string version)
    {
        int totalEntries = 0;
        int currentEntry = 0;

        if (!Directory.Exists(outputFolderPath))
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(appxFilePath))
                {
                    totalEntries = archive.Entries.Count;

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        try
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
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to extract {entry.FullName}: {ex.Message}");
                        }
                    }
                }
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine($"The archive is invalid or corrupted: {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"You do not have the required permissions: {ex.Message}");
                return false;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O error occurred: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return false;
            }
        }

        return true;
    }


    public static async Task<bool> InstallMinecraft(string version, UIElement element)
    {
        if (!Minecraft.IsInstalled)
        {
            Application.Current.MainWindow.Dispatcher.Invoke(() => MainWindow.CreateMessageBox("⚠️ Minecraft isn't installed, please install it."));
            return false;
        }

        var gdk = Minecraft.UsingGameDevelopmentKit;

        if (gdk || Minecraft.IsPackaged)
        {
            if (!gdk)
            {
                string backupname = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(":", "-");
                if (!await BackupManager.CreateBackup(backupname)) return false;
            }

            MainWindow.isDownloadingVersion = true;

            using var request = await MainWindow.VersionCatalog.InstallAsync(version, i => Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.progressType = "Installing";
                MainWindow.progressPercentage = i;
                if (MainWindow.progressPercentage == 100) MainWindow.isDownloadingVersion = false;

            })); await request;
        }
        else
        {
            if (!Developer.Enabled)
            {
                // Enable Developer Mode
                Developer.Request();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.CreateMessageBox("Please enable developer mode.");
                });
                Trace.WriteLine("Developer Mode has been requested.");
                return false;
            }


            Trace.WriteLine("Developer Mode is enabled on your system!");
            // Continue with the rest of your application logic here.

            string url = (await MainWindow.VersionCatalog.UriAsync(version)).OriginalString;

            MainWindow.progressPercentage = 0;
            string path = Path.Combine(launcherPath, "Versions", $"Minecraft{version}.Appx");

            element.Dispatcher.Invoke(() => VersionItemProperties.SetState(element, 4));
            Trace.WriteLine(url);
            bool ello = await DownloadApplication(url, version);

            if (ello)
            {
                Trace.WriteLine("Finished downloading the specified version's application bundle.");

                if (!Developer.Enabled)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow.CreateMessageBox(
                            "FAILED TO TURN ON DEVELOPER MODE! Turn it on yourself, we cannot continue with Version Changer.");
                    }); //
                    return false;
                }

                string backupname = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(":", "-");

                if (!Directory.Exists(Path.Combine(launcherPath, "Versions", version)))
                {
                    Trace.WriteLine("Starting extract.");
                    bool result = await ExtractAppxAsync(path, Path.Combine(launcherPath, "Versions", version),
                        version);

                    if (!result)
                    {

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindow.CreateMessageBox("The Downloaded APPX is corrupted.");
                            MainWindow.CreateMessageBox(
                                "Restart the Launcher and click your desired version again.");
                        });

                        File.Delete(path);

                        return false;
                    }
                }
                else
                {
                    Trace.WriteLine("The folder exists");
                }

                string pathway = Path.Combine(launcherPath, "Versions", version);
                var packageUri = new Uri(Path.Combine(pathway, "AppxManifest.xml"));
                Trace.WriteLine(packageUri.ToString());

                File.Delete(Path.Combine(pathway, "AppxSignature.p7x"));

                MainWindow.progressPercentage = 100;
                MainWindow.progressType = "Installing";

                var manifestPath = Path.Combine(pathway, "AppxManifest.xml");
                var escapedPath = manifestPath.Replace("\"", "\\\"");

                if (!await Task.Run(() => AreDependenciesInstalled(escapedPath)))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow.CreateMessageBox(
                            "Failed to install. Join our discord for help: https://flarial.xyz/discord");
                        MainWindow.CreateMessageBox("Proper dependencies not installed.");
                    });
                    return false;
                }

                if (MinecraftGame.Package != null)
                {
                    if (await BackupManager.GetConfig(backupname) == null)
                    {
                        MainWindow.progressType = "backup";
                        MainWindow.progressPercentage = 100;
                        element.Dispatcher.Invoke(() => VersionItemProperties.SetState(element, 5));
                        if (!await BackupManager.CreateBackup(backupname)) return false;
                    }

                    Trace.WriteLine("Uninstalling current Minecraft version.");
                    await RemoveMinecraftPackage();
                }

                Trace.WriteLine("Uninstalled.");

                element.Dispatcher.Invoke(() => VersionItemProperties.SetState(element, 1));


                Trace.WriteLine("Deploying Minecraft's Application Bundle.");

                if (await InstallAppBundle(Path.Combine(launcherPath, "Versions", version)) == false)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow.CreateMessageBox(
                            "Failed to install. Join our discord for help: https://flarial.xyz/discord");
                        MainWindow.CreateMessageBox(
                            "Your data and worlds are saved at %localappdata%/Flarial/Launcher.");
                    });

                    return false;
                }

                await Task.Delay(1);

                if (await BackupManager.GetConfig(backupname) != null)
                {
                    Trace.WriteLine("Temporary backup found, now loading.");
                    await BackupManager.LoadBackup(backupname);
                }

                Trace.WriteLine("Installation complete.");
                //
                Application.Current.Dispatcher.Invoke(() =>
                {
                    //new CustomDialogBox("Restart the Launcher", "Please restart the launcher for it to be able to install new patches.", "MessageBox").ShowDialog();
                    MainWindow.CreateMessageBox("Installed!");
                });

            }

            return ello;
        }

        return false;
    }

    static void CloseInstances()
    {

        string[] processesToClose = { "Minecraft.Windows.exe", "Minecraft.Windows" };

        foreach (var processName in processesToClose)
        {
            // Get all processes with the specified name
            Process[] processes = Process.GetProcessesByName(processName);

            foreach (Process process in processes)
            {
                try
                {
                    process.CloseMainWindow();
                    process.WaitForExit(5000);

                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error closing process {processName}: {ex.Message}");
                }
            }
        }
    }

    static async Task DeleteAppDataFiles()
    {
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "Microsoft.MinecraftUWP_8wekyb3d8bbwe");

        try
        {
            if (Directory.Exists(appDataPath))
            {
                Directory.Delete(appDataPath, true);
                Trace.WriteLine("Application data deleted successfully.");
            }
            else
            {
                Trace.WriteLine("Application data directory does not exist.");
            }

            if (MinecraftGame.ApplicationData != null)
            {
                await MinecraftGame.ApplicationData.ClearAsync(ApplicationDataLocality.Local | ApplicationDataLocality.Roaming | ApplicationDataLocality.Temporary | ApplicationDataLocality.LocalCache);
            }
            else
            {
                Trace.WriteLine("Minecraft ApplicationData is null.");
            }////
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error deleting application data: {ex.Message}");
        }
    }//

}