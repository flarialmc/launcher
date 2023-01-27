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







        public static async Task DownloadApplication(string Version)
        {
            bool archived = false;

            string path = "None yet.";

            //Select path between archive or normal.

            path =
               launcherPath + "Versions\\"
                + $"Minecraft{Version}.Appx";


            Trace.WriteLine(
                "The path to which the bundle will be installed is as follows: " + path
            );

            var url = "Not sure";


            url = await GetVersionLinkAsync(Version);


            if (url != "Not sure" && url != null)
            {
                Trace.WriteLine("Got downloadable url for the specified Minecraft version: " + url);
                if (File.Exists(path))
                {
                    return;
                }
                WebClient webClient = new WebClient();

                await webClient.DownloadFileTaskAsync(new Uri(url), path);
                return;
            }
            Trace.WriteLine("Url was null, download failed.");
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
                var result = packageManager.RemovePackageAsync(Minecraft.Package.InstalledLocation.Name, RemovalOptions.RemoveForAllUsers);
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


        public static async Task<string> CacheVersionsList()
        {
            var p =
               launcherPath + "\\List.json";


            if (File.Exists(p))
            {
                return File.ReadAllText(p);
            }
            else
            {
                List<VersionStruct> versionStructs = new List<VersionStruct>();
                var AllVersions = await GetVersionsAsync();

                foreach (var v in AllVersions)
                {
                    var Url = await GetVersionLinkAsync(v);

                    //May fail sometimes.
                    var Desc = new WebClient().DownloadString(new Uri($"https://cdn.flarial.net/VersionsStructure/{v}.txt"));


                    versionStructs.Add(new VersionStruct()
                    {
                        DownloadURL = Url,
                        Version = v,
                        Description = Desc

                    });


                }


                var json = JsonSerializer.Serialize(versionStructs);

                File.WriteAllText(p, json);
                return json;


            }


        }
        public static async Task<bool> InstallAppBundle(string appBundleUri)
        {





            bool returnValue = true;
            try
            {
                Uri packageUri = new Uri(appBundleUri);
                Windows.Management.Deployment.PackageManager packageManager = new Windows.Management.Deployment.PackageManager();
                Windows.Foundation.IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> deploymentOperation = packageManager.AddPackageAsync(packageUri, null, Windows.Management.Deployment.DeploymentOptions.None);
                ManualResetEvent opCompletedEvent = new ManualResetEvent(false); // this event will be signaled when the deployment operation has completed.
                deploymentOperation.Completed = (depProgress, status) => { opCompletedEvent.Set(); };
                Trace.WriteLine(string.Format("Installing package {0}", appBundleUri));
                Trace.WriteLine("Waiting for installation to complete...");
                opCompletedEvent.WaitOne();
                if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Error)
                {
                    Windows.Management.Deployment.DeploymentResult deploymentResult = deploymentOperation.GetResults();
                    Trace.WriteLine(string.Format("Installation Error: {0}", deploymentOperation.ErrorCode));
                    Trace.WriteLine(string.Format("Detailed Error Text: {0}", deploymentResult.ErrorText));
                    returnValue = false;
                }
                else if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Canceled)
                {
                    Trace.WriteLine("Installation Canceled");
                    returnValue = false;
                }
                else if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Completed)
                {
                    Trace.WriteLine("Installation succeeded!");
                }
                else
                {
                    returnValue = false;
                    Trace.WriteLine("Installation status unknown");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("AddPackageSample failed, error message: {0}", ex.Message));
                Trace.WriteLine(string.Format("Full Stacktrace: {0}", ex.ToString()));
                return false;
            }
            return returnValue;
        }

    }
}
