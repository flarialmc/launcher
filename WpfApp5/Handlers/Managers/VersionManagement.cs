using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Management.Deployment;

namespace Flarial.Launcher.Managers
{
    public class VersionManagement
    {
        public static string launcherPath = Environment.GetFolderPath((Environment.SpecialFolder.LocalApplicationData))
                + "\\Flarial\\Launcher\\";




        public class VersionStruct
        {
            public string Version { get; set; }
            public string DownloadURL { get; set; }
            public string Description { get; set; }

        }
        /*
        public static async Task<List<string>> GetAll86xBuildsAsync()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("SomeName"));
            var Releases = await client.Repository.Release.GetAll("ambiennt", "MCBEx86");

            List<string> ReleaseNames = new List<string>();
            foreach (var Release in Releases)
            {
                ReleaseNames.Add(Release.TagName);
                //    Trace.WriteLine(Release.TagName);
            }

            return ReleaseNames;
        }

        public static async Task<string> Get86BuildDownloadLinkAsync(string version)
        {
            string url = "DownloadUrl";

            GitHubClient client = new GitHubClient(new ProductHeaderValue("SomeName"));
            var Releases = await client.Repository.Release.GetAll("ambiennt", "MCBEx86");

            foreach (var Release in Releases)
            {
                if (Release.TagName == version)
                {
                    foreach (var asset in Release.Assets)
                    {
                        if (asset.Name.EndsWith(".Appx"))
                        {
                            url = asset.BrowserDownloadUrl;
                        }
                    }
                }
            }

            //   else { Trace.WriteLine("That version does not exist in the x86 archive!"); return null; }

            return url;
        }
        */






        public static async Task<List<string>> GetVersionsAsync()
        {
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("SomeName"));

                var Assets = await client.Repository.Release.GetAllAssets(
                    "PlasmaWasTaken",
                    "LocalStorage",
                    56308497
                );

                List<string> releaseAssets = new List<string>();
                foreach (var asset in Assets)
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

        public static async Task<string> GetVersionLinkAsync(string version)
        {
            try
            {
                var allVersions = await GetVersionsAsync();



                string AssetLink =
                    "https://github.com/PlasmaWasTaken/LocalStorage/releases/download/44/Minecraft";

                if (allVersions.Contains(version))
                {
                    AssetLink =
                        "https://github.com/PlasmaWasTaken/LocalStorage/releases/download/44/Minecraft"
                        + version
                        + ".Appx";
                }
                else
                {
                    MessageBox.Show("This version is not available for download!");
                }

                return AssetLink;
            }
            catch (RateLimitExceededException)
            {
                MessageBox.Show("Octokit Rate Limit was reached.");
                return null;
            }
        }







        public static async Task DownloadApplication(string version)
        {
            // Define path and url.
            string path = Path.Combine(launcherPath, "Versions", $"Minecraft{version}.Appx");
            string url = await GetVersionLinkAsync(version);

            // Check if file already exists.
            if (File.Exists(path))
            {
                Trace.WriteLine("File already exists, download skipped.");
                return;
            }

            // Download the file.
            if (!string.IsNullOrEmpty(url))
            {
                Trace.WriteLine("Got downloadable url for Minecraft version " + version + ": " + url);
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += DownloadProgressCallback;
                    await webClient.DownloadFileTaskAsync(new Uri(url), path);
                    Trace.WriteLine("Download succeeded!");
                }
            }
            else
            {
                Trace.WriteLine("Url was null, download failed.");
            }
        }



        //Actual version changer

        public static async Task InstallMinecraft(string version)
        {
            string path =

               launcherPath + "Versions\\"
                + $"Minecraft{version}.Appx";



            await DownloadApplication(version);
            Trace.WriteLine("Finished downloading the version specified's application bundle..");









            if (Minecraft.Package != null)
            {
                //Backup Data
                await BackupManager.createBackup("MinecraftBeforeInstallation");

                Trace.WriteLine("Uninstalling Current Minecraft Version.");
                //Now uninstall MC (If it exists)



                var packageManager = new PackageManager();
                Trace.WriteLine(Minecraft.Package.Id.FullName);
                var result = packageManager.RemovePackageAsync(Minecraft.Package.Id.FullName, RemovalOptions.RemoveForAllUsers);
                var completed = new AutoResetEvent(false);
                result.Completed = (waitResult, status) => completed.Set();
                completed.WaitOne();

            }

            //Install Minecraft.

            //  packageOptions.InstallAllResources = true;

            Trace.WriteLine("Deploying Minecraft's Application Bundle.");
            await InstallAppBundle(path);



            await Task.Delay(1);

            Trace.WriteLine("Temporary backup found, now loading.");
            //Load Backed up Data
            await BackupManager.loadBackup("MinecraftBeforeInstallation");

            Trace.WriteLine("Temporary backup loaded, now deleting.");
            //Delete temporary backup
            await BackupManager.DeleteBackup("MinecraftBeforeInstallation");

            Minecraft.Init();

            Trace.WriteLine("Installation Complete, now closing Task.");



            return;
        }

        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)

        {

            // Displays the operation identifier, and the transfer progress.

            Trace.WriteLine($" downloaded {e.BytesReceived} of {e.TotalBytesToReceive} bytes. {e.ProgressPercentage} % complete...");
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
        public static async Task<bool> InstallAppBundle(string appBundleUri)
        {
            try
            {
                var packageUri = new Uri(appBundleUri);
                var packageManager = new Windows.Management.Deployment.PackageManager();
                var deploymentResult = await packageManager.AddPackageAsync(packageUri, null, Windows.Management.Deployment.DeploymentOptions.None);
                if (deploymentResult.IsRegistered)
                {
                    Trace.WriteLine("Installation succeeded!");
                    return true;
                }
                else
                {
                    Trace.WriteLine($"Installation Error: {deploymentResult.ErrorText}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"AddPackageSample failed, error message: {ex.Message}\nFull Stacktrace: {ex.ToString()}");
                return false;
            }
        }

    }
}
