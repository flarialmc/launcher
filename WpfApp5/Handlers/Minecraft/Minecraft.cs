using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using Windows.Management.Deployment;
using Flarial.Launcher.Managers;

namespace Flarial.Launcher
{

    public static partial class Minecraft
    {

        public static Process Process;
        public const string FamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

        public static Windows.Management.Deployment.PackageManager PackageManager { get; private set; }
        public static Windows.ApplicationModel.Package Package { get; private set; }
        public static Windows.ApplicationModel.PackageId PackageId => Package.Id ?? throw new NullReferenceException();
        public static Windows.System.ProcessorArchitecture PackageArchitecture => PackageId.Architecture;
        public static Windows.ApplicationModel.PackageVersion PackageVersion => PackageId.Version;
        public static Windows.Storage.ApplicationData ApplicationData { get; private set; }

        public static void Init()
        {

            InitManagers();
            FindPackage();



            var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
            if (mcIndex.Length > 0)
            {
                Process = mcIndex[0];

            }

        }

        public static bool isInstalled()
        {
            PackageManager = new Windows.Management.Deployment.PackageManager();
            var Packages = PackageManager.FindPackages(FamilyName);
            return Packages.Count() != 0;
        }
        
        public static bool IsAppxFileCorrupted(string filePath)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(filePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.Name.Equals("AppxManifest.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                // Read the XML content from the AppxManifest.xml file
                                string xmlContent = reader.ReadToEnd();

                                // Attempt to parse the XML content to check for any parsing errors
                                XmlDocument xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(xmlContent);

                                // If the XML parsing was successful, the APPX file is not corrupted
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // An exception occurred while attempting to open or process the APPX file,
                // indicating that the file is corrupted.
                return true;
            }

            // The APPX file is considered corrupted if the AppxManifest.xml entry is not found.
            return true;
        }

        public static string CheckAppxFiles(string folderPath)
        {
            string[] appxFiles = Directory.GetFiles(folderPath, "*.appx");

            if (appxFiles.Length > 0)
            {
                string lastAppxFile = appxFiles.OrderByDescending(f => File.GetLastWriteTime(f)).First();

                if (!IsAppxFileCorrupted(lastAppxFile))
                {
                        return lastAppxFile;
                }
            }

            return null;
        }

        public static void InitManagers()
        {
            PackageManager = new Windows.Management.Deployment.PackageManager();
            var Packages = PackageManager.FindPackages(FamilyName);
            
            if (Packages.Count() == 0)
            {
                    
                CustomDialogBox MessageBox = new CustomDialogBox("Error", "Minecraft needs to be installed to verify that you have bought the game.", "MessageBox");
                MessageBox.ShowDialog();
                Environment.Exit(0);
            }
            else
            {
                ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);
            }

            //  ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);


        }

        public static void FindPackage()
        {

            if (PackageManager is null) throw new NullReferenceException();
            var Packages = PackageManager.FindPackages(FamilyName);

            if (Packages.Count() != 0)
            {
                Package = Packages.First();
            }
        }

        public static Version GetVersion()
        {
            var v = PackageVersion;
            return new Version(v.Major, v.Minor, v.Build, v.Revision);
        }
    }
}