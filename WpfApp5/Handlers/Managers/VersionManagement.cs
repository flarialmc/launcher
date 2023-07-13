using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Windows.Management.Deployment;
using Newtonsoft.Json.Linq;
using Application = System.Windows.Application;

namespace Flarial.Launcher.Managers
{
    public class VersionManagement
    {
        public static string launcherPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flarial", "Launcher");

        public class VersionStruct
        {
            public string Version { get; set; }
            public string DownloadURL { get; set; }
            public string Description { get; set; }
        }

        public static async Task<List<string>> GetVersionsAsync()
        {
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("SomeName"));
                var assets = await client.Repository.Release.GetAllAssets("PlasmaWasTaken", "LocalStorage", 56308497);

                List<string> releaseAssets = new List<string>();
                foreach (var asset in assets)
                {
                    var withoutMinecraft = asset.Name.Substring(9);
                    releaseAssets.Add(withoutMinecraft.Substring(0, withoutMinecraft.Length - 5));
                }

                return releaseAssets;
            }
            catch (Octokit.RateLimitExceededException)
            {
                MessageBox.Show("Octokit API rate limit was reached.");
                return null;
            }
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
            if (version == "1.20.0") result = ExtractUrl(webClient.DownloadStringTaskAsync(new Uri("https://api.jiayi.software/api/v1/minecraft/download_url?version=1.20.0.1&arch=x64")).Result);
            if (version == "1.20.10") result = null;

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
                        "There was an issue with the URL. Please report this in our discord.", "MessageBox");
                    MessageBox.ShowDialog();
                });

                return false;
            }

            return true;
        }
        public static async Task<bool> InstallAppBundle(string appBundleUri)
        {
            try
            {
                var packageUri = new Uri(appBundleUri);
                var packageManager = new PackageManager();

                var progress = new Progress<DeploymentProgress>(ReportProgress);
                var addPackageOperation = packageManager.AddPackageAsync(packageUri, null, DeploymentOptions.ForceUpdateFromAnyVersion);
                addPackageOperation.Progress += (sender, args) => ReportProgress(args);

                var addPackageTask = addPackageOperation.AsTask();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(300)); // Adjust timeout duration as needed

                var completedTask = await Task.WhenAny(addPackageTask, timeoutTask);

                if (completedTask == addPackageTask && addPackageTask.Status == TaskStatus.RanToCompletion)
                {
                    // Check if the package is registered after deployment
                    Minecraft.Init();
                    var packageName = Minecraft.Package;

                    if (packageName != null)
                    {
                        Trace.WriteLine("Installation succeeded!");
                        return true;
                    }
                    else
                    {
                        Trace.WriteLine("Package registration check failed.");
                        return false;
                    }
                }
                else
                {
                    Trace.WriteLine("Package deployment timed out.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"AddPackageSample failed, error message: {ex.Message}\nFull Stacktrace: {ex.ToString()}");
                return false;
            }
        }

        private static void ReportProgress(DeploymentProgress progress)
        {
            // Report the progress of the deployment
            MainWindow.progressPercentage = (int)progress.percentage;
        }

        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage != 100)
            MainWindow.progressPercentage = e.ProgressPercentage;
            MainWindow.progressBytesReceived = e.BytesReceived;
            MainWindow.progressBytesTotal = e.TotalBytesToReceive;
            Trace.WriteLine($"Downloaded {e.BytesReceived} of {e.TotalBytesToReceive} bytes. {e.ProgressPercentage}% complete...");
        }

        public static async Task<string> CacheVersionsList()
        {
            var jsonPath = Path.Combine(launcherPath, "List.json");

            if (File.Exists(jsonPath))
            {
                return File.ReadAllText(jsonPath);
            }

            var versionStructs = new List<VersionStruct>();
            var allVersions = await GetVersionsAsync();

            using (var client = new WebClient())
            {
                foreach (var version in allVersions)
                {
                    var url = await GetVersionLinkAsync(version);
                    var descUri = new Uri($"https://cdn.flarial.net/VersionsStructure/{version}.txt");

                    try
                    {
                        var desc = await client.DownloadStringTaskAsync(descUri);
                        versionStructs.Add(new VersionStruct
                        {
                            DownloadURL = url,
                            Version = version.Remove(version.LastIndexOf(".")),
                            Description = desc
                        });
                    }
                    catch (WebException ex)
                    {
                        Trace.WriteLine($"Failed to download description for version {version}: {ex.Message}");
                    }
                }
            }

            var json = JsonSerializer.Serialize(versionStructs);
            File.WriteAllText(jsonPath, json);

            return json;
        }

        public static async Task<bool> RemoveMinecraftPackage()
        {
            try
            {
                var packageManager = new PackageManager();

                var progress = new Progress<DeploymentProgress>(ReportProgress);
                var removePackageOperation = packageManager.RemovePackageAsync(Minecraft.Package.Id.FullName, RemovalOptions.None);
                removePackageOperation.Progress += (sender, args) => ReportProgress(args);

                var removePackageTask = removePackageOperation.AsTask();

                await removePackageTask;

                if (removePackageTask.Status == TaskStatus.RanToCompletion)
                {
                    Trace.WriteLine("Package removal succeeded!");
                    return true;
                }
                else
                {
                    Trace.WriteLine("Package removal failed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"RemovePackageAsync failed, error message: {ex.Message}\nFull Stacktrace: {ex.ToString()}");
                return false;
            }
        }


        public static async Task<bool> InstallMinecraft(string version)
        {
            MainWindow.progressPercentage = 0;
            string path = Path.Combine(launcherPath, "Versions", $"Minecraft{version}.Appx");

            bool ello = await DownloadApplication(version);

            if (ello)
            {
                Trace.WriteLine("Finished downloading the specified version's application bundle.");

                Minecraft.Init();

                if (Minecraft.Package != null)
                {
                    await BackupManager.CreateBackup("temp");
                    Trace.WriteLine("Uninstalling current Minecraft version.");

                    var packageManager = new PackageManager();

                    RemoveMinecraftPackage();

                }

                Trace.WriteLine("Deploying Minecraft's Application Bundle.");
                if (await InstallAppBundle(path) == false)
                {
                    Trace.WriteLine("Failed to deploy.");
                    return false;
                }

                await Task.Delay(1);

                Trace.WriteLine("Temporary backup found, now loading.");
                await BackupManager.LoadBackup("temp");

                Trace.WriteLine("Temporary backup loaded, now deleting.");
                await BackupManager.DeleteBackup("temp");


                Trace.WriteLine("Installation complete.");
            }

            return ello;
        }
    }
}
