using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
namespace Flarial.Launcher.Managers
{
    //Configuration
    public class BackupConfiguration
    {
        public DateTime BackupTime
        {
            get;
            set;
        }
        public string MinecraftVersion
        {
            get;
            set;
        }
        public Guid BackupId
        {
            get;
            set;
        }
    }


    //Actual Manager
    public static class BackupManager
    {
        public static string backupDirectory = VersionManagement.launcherPath + "\\Backups\\";




        public static async Task<List<string>> FilterByName(string filterName)
        {
            var unfilteredBackups = await GetAllBackupsAsync();
            return unfilteredBackups.Where(backup => backup.StartsWith(filterName)).ToList();
        }
        public static async Task<List<string>> GetAllBackupsAsync()
        {
            return await Task.Run(() =>
            {
                return Directory.GetDirectories(backupDirectory)
                                .Select(dir => new DirectoryInfo(dir).Name)
                                .ToList();
            });
        }


        public static async Task LoadBackup(string backupName)
        {
            try
            {
                var mcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages\\Microsoft.MinecraftUWP_8wekyb3d8bbwe\\LocalState\\games\\com.mojang");
                var backupMojangPath = Path.Combine(backupDirectory, backupName, "com.mojang");
                var backupRoamingPath = Path.Combine(backupDirectory, backupName, "RoamingState");

                if (!Directory.Exists(backupMojangPath))
                {
                    MessageBox.Show("No Minecraft backups available with the given ID.", "Failed to Load Backup");
                    return;
                }

                await DirectoryCopy(backupMojangPath, mcPath, true);

                if (Directory.Exists(backupRoamingPath))
                {
                    var flarialPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages\\Microsoft.MinecraftUWP_8wekyb3d8bbwe\\RoamingState");
                    await DirectoryCopy(backupRoamingPath, flarialPath, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public static async Task CreateBackup(string backupName)
        {
            try
            {
                var backupDirectoryPath = Path.Combine(backupDirectory, backupName);
                if (Directory.Exists(backupDirectoryPath))
                {
                    return;
                }

                var mcPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Packages",
                    "Microsoft.MinecraftUWP_8wekyb3d8bbwe",
                    "LocalState",
                    "games"
                );
                var flarialPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Packages",
                    "Microsoft.MinecraftUWP_8wekyb3d8bbwe",
                    "RoamingState"
                );

                var backupConfigPath = Path.Combine(backupDirectoryPath, "BackupConfig.json");

                Directory.CreateDirectory(backupDirectoryPath);
                Directory.CreateDirectory(Path.Combine(backupDirectoryPath, "com.mojang"));
                Directory.CreateDirectory(Path.Combine(backupDirectoryPath, "RoamingState"));

                if (Directory.Exists(mcPath))
                {
                    await Task.WhenAll(Directory.EnumerateDirectories(mcPath).Select(async dir =>
                    {
                        var attributes = File.GetAttributes(dir);
                        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            attributes &= ~FileAttributes.ReadOnly;
                            File.SetAttributes(dir, attributes);
                        }
                        await DirectoryCopy(dir, Path.Combine(backupDirectoryPath, "com.mojang", new DirectoryInfo(dir).Name), true);
                    }));
                }
                else
                {
                    MessageBox.Show("Minecraft Data Path is invalid!", "Failed To Backup");
                }

                if (Directory.Exists(flarialPath))
                {
                    await Task.WhenAll(Directory.EnumerateDirectories(flarialPath).Select(async dir =>
                    {
                        var attributes = File.GetAttributes(dir);
                        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            attributes &= ~FileAttributes.ReadOnly;
                            File.SetAttributes(dir, attributes);
                        }
                        await DirectoryCopy(dir, Path.Combine(backupDirectoryPath, "RoamingState", new DirectoryInfo(dir).Name), true);
                    }));
                }
                else
                {
                    MessageBox.Show("Roaming State Data Path is invalid!", "Failed To Backup");
                }

                var text = await CreateConfig();
                File.WriteAllText(backupConfigPath, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public static async Task DeleteBackup(string backupName)
        {
            await DeleteDirectory(backupDirectory + backupName);
        }

        public static async Task<string> CreateConfig()
        {
            var version = Minecraft.GetVersion();
            await Task.Delay(1);

            var backupConfig = new BackupConfiguration
            {
                BackupTime = DateTime.Now,
                MinecraftVersion = version.ToString(),
                BackupId = Guid.NewGuid(),
            };

            return JsonSerializer.Serialize(backupConfig);
        }
        public static async Task<BackupConfiguration> getConfig(string backupName)
        {
            var path = Path.Combine(backupDirectory, backupName, "BackupConfig.json");

            if (!File.Exists(path))
            {
                return null;
            }

            using (FileStream openStream = File.OpenRead(path))
            {
                return await JsonSerializer.DeserializeAsync<BackupConfiguration>(openStream);
            }
        }



        //Utilities 
        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
        private static async Task DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }
            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            Directory.CreateDirectory(destDirName);
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
                Console.WriteLine("Copying " + file + " to " + tempPath);
            }
            // If copying subdirectories, copy them and their contents to new
            // location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    await DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        public static async Task DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            foreach (string dir in dirs)
            {
                await DeleteDirectory(dir);
            }
            Directory.Delete(target_dir, false);
        }
    }
}