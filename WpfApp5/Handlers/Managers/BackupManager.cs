using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            return await Task.Run(() => Directory.GetDirectories(backupDirectory).Select(Path.GetFileName).ToList());
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
                    "games"
                );

                var backupMojangPath = Path.Combine(backupDirectory, backupName, "com.mojang");
                var backupRoamingPath = Path.Combine(backupDirectory, backupName, "RoamingState");

                if (!Directory.Exists(backupMojangPath))
                {
                    MessageBox.Show("No Minecraft backups available with the given ID.", "Failed to Load Backup");
                    return;
                }

                await DirectoryCopyAsync(backupMojangPath, mcPath, true);

                var flarialPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Packages",
                    "Microsoft.MinecraftUWP_8wekyb3d8bbwe",
                    "RoamingState"
                );
                if (Directory.Exists(backupRoamingPath))
                {
                    await DirectoryCopyAsync(backupRoamingPath, flarialPath, true);
                }
                else
                {
                    MessageBox.Show("Roaming State backup data not found.", "Failed to Load Backup");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.CreateMessageBox("Backup loaded.");
                });
            }
        }

        public static async Task<bool> CreateBackup(string backupName)
        {
            try
            {
                var backupDirectoryPath = Path.Combine(backupDirectory, backupName);
                if (Directory.Exists(backupDirectoryPath))
                {
                    MessageBox.Show("Backup with the given name already exists.", "Failed to Create Backup");
                    return false;
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

                if (!Directory.Exists(mcPath))
                {
                    MessageBox.Show("Minecraft Data Path is invalid!", "Failed To Backup");
                    return false;
                }

                Directory.CreateDirectory(backupDirectoryPath);

                if(!await BackupDirectoryAsync(mcPath, Path.Combine(backupDirectoryPath, "com.mojang"))) return false;
                
                if (Directory.Exists(flarialPath))
                {
                    if (!await BackupDirectoryAsync(flarialPath, Path.Combine(backupDirectoryPath, "RoamingState")))
                        return false;
                }
                else
                {
                    MessageBox.Show("Roaming State Data Path is invalid!", "Failed To Backup");
                    return false;
                }

                var text = await CreateConfig();
                File.WriteAllText(Path.Combine(backupDirectoryPath, "BackupConfig.json"), text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return false;
            }
            
            return true;
        }

        private static async Task<bool> BackupDirectoryAsync(string source, string destination)
        {
            var sourceDirectory = new DirectoryInfo(source);
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + source);
            }

            var destinationDirectory = Directory.CreateDirectory(destination);

            await Task.WhenAll(sourceDirectory.GetFiles().Select(async file =>
            {
                
                try
                {
                    FileAttributes attributes = File.GetAttributes(file.FullName);
    
                    if ((attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindow.CreateMessageBox("Failed to install. Join our discord for help: https://flarial.xyz/discord");
                            MainWindow.CreateMessageBox("Files are encrypted!");
                        });
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error checking file attributes: {ex.Message}");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow.CreateMessageBox("Failed to install. Join our discord for help: https://flarial.xyz/discord");
                        MainWindow.CreateMessageBox("Error checking for files.");
                    });
                    return false;
                }
                
                var tempPath = Path.Combine(destinationDirectory.FullName, file.Name);
                await Task.Run(() => file.CopyTo(tempPath, true));
                Trace.WriteLine($"Copying {file} to {tempPath}");

                return true;
            }));

            await Task.WhenAll(sourceDirectory.GetDirectories().Select(subdir =>
            {
                var tempPath = Path.Combine(destinationDirectory.FullName, subdir.Name);
                return DirectoryCopyAsync(subdir.FullName, tempPath, true);
            }));
            return true;
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

            using (var openStream = File.OpenRead(path))
            {
                return await JsonSerializer.DeserializeAsync<BackupConfiguration>(openStream).ConfigureAwait(false);
            }
        }

        private static async Task DeleteDirectoryAsync(string targetDir)
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
                await Task.Run(() =>
                {
                    try
                    {
                        file.CopyTo(tempPath, true);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e);
                    }
                });
                Trace.WriteLine("Copying " + file + " to " + tempPath);
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

    }

}