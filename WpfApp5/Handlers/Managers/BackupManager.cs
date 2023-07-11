using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Flarial.Launcher.Managers
{
    // Configuration
    public class BackupConfiguration
    {
        public DateTime BackupTime { get; set; }
        public string MinecraftVersion { get; set; }
        public Guid BackupId { get; set; }
    }

    // Actual Manager
    public static class BackupManager
    {
        public static string backupDirectory = Path.Combine(VersionManagement.launcherPath, "Backups");

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
                    .Select(dir => Path.GetFileName(dir))
                    .ToList();
            });
        }

        public static async Task LoadBackup(string backupName)
        {
            try
            {
                var mcPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Packages",
                    "Microsoft.MinecraftUWP_8wekyb3d8bbwe",
                    "LocalState",
                    "games",
                    "com.mojang"
                );
                var backupMojangPath = Path.Combine(backupDirectory, backupName, "com.mojang");
                var backupRoamingPath = Path.Combine(backupDirectory, backupName, "RoamingState");

                if (!Directory.Exists(backupMojangPath))
                {
                    MessageBox.Show("No Minecraft backups available with the given ID.", "Failed to Load Backup");
                    return;
                }

                await DirectoryCopyAsync(backupMojangPath, mcPath, true);

                if (Directory.Exists(backupRoamingPath))
                {
                    var flarialPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Packages",
                        "Microsoft.MinecraftUWP_8wekyb3d8bbwe",
                        "RoamingState"
                    );
                    await DirectoryCopyAsync(backupRoamingPath, flarialPath, true);
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
                Directory.CreateDirectory(Path.Combine(backupDirectoryPath));
                Directory.CreateDirectory(Path.Combine(backupDirectoryPath));

                if (Directory.Exists(mcPath))
                {
                    await Task.WhenAll(Directory.GetDirectories(mcPath)
                        .Select(async dir =>
                        {
                            var attributes = File.GetAttributes(dir);
                            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                attributes &= ~FileAttributes.ReadOnly;
                                File.SetAttributes(dir, attributes);
                            }
                            await DirectoryCopyAsync(dir, Path.Combine(backupDirectoryPath, Path.GetFileName(dir)), true);
                        }));
                }
                else
                {
                    MessageBox.Show("Minecraft Data Path is invalid!", "Failed To Backup");
                }

                if (Directory.Exists(flarialPath))
                {
                    await Task.WhenAll(Directory.GetDirectories(flarialPath)
                        .Select(async dir =>
                        {
                            var attributes = File.GetAttributes(dir);
                            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                attributes &= ~FileAttributes.ReadOnly;
                                File.SetAttributes(dir, attributes);
                            }
                            await DirectoryCopyAsync(dir, Path.Combine(backupDirectoryPath, "RoamingState", Path.GetFileName(dir)), true);
                        }));
                }
                else
                {
                    MessageBox.Show("Roaming State Data Path is invalid!", "Failed To Backup");
                }

                var text = await CreateConfig();
                await File.WriteAllTextAsync(backupConfigPath, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static async Task DeleteBackup(string backupName)
        {
            await DeleteDirectoryAsync(Path.Combine(backupDirectory, backupName));
        }

        public static async Task<string> CreateConfig()
        {
            var version = Minecraft.GetVersion();
            await Task.CompletedTask;

            var backupConfig = new BackupConfiguration
            {
                BackupTime = DateTime.Now,
                MinecraftVersion = version.ToString(),
                BackupId = Guid.NewGuid(),
            };

            return JsonSerializer.Serialize(backupConfig);
        }

        public static async Task<BackupConfiguration> GetConfig(string backupName)
        {
            var path = Path.Combine(backupDirectory, backupName, "BackupConfig.json");

            if (!File.Exists(path))
            {
                return null;
            }

            using (FileStream openStream = File.OpenRead(path))
            {
                return await JsonSerializer.DeserializeAsync<BackupConfiguration>(openStream).ConfigureAwait(false);
            }
        }

        // Utilities
        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        } // Utilities
        private static async Task DirectoryCopyAsync(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            Directory.CreateDirectory(destDirName);

            var files = dir.GetFiles();
            await Task.WhenAll(files.Select(async file =>
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
                Console.WriteLine("Copying " + file + " to " + tempPath);
            }));

            if (copySubDirs)
            {
                var dirs = dir.GetDirectories();
                await Task.WhenAll(dirs.Select(subdir =>
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    return DirectoryCopyAsync(subdir.FullName, tempPath, copySubDirs);
                }));
            }
        }

        public static async Task DeleteDirectoryAsync(string targetDir)
        {
            var files = Directory.GetFiles(targetDir);
            var dirs = Directory.GetDirectories(targetDir);

            await Task.WhenAll(files.Select(async file =>
            {
                File.SetAttributes(file, FileAttributes.Normal);
                await Task.Run(() => File.Delete(file));
            }));

            await Task.WhenAll(dirs.Select(DeleteDirectoryAsync));

            Directory.Delete(targetDir, false);
        }
    }
}