using System;

namespace Flarial.Launcher;

public static partial class MinecraftGame
{
    public static class Backups
    {
        public class BackupDescriptor
        {
            public BackupDescriptor(string name, Guid id, Version version, DateTime date)
            {
                this.name = name;
                this.id = id;
                this.version = version;
                this.date = date;
            }

            public string name;
            public Guid id;
            public Version version;
            public DateTime date;
        };

        public static BackupDescriptor CreateBackupDescriptor(string name, Guid id) => new BackupDescriptor(name, id, GetVersion(), DateTime.Now);


        public static string FormatBackupDescriptor(BackupDescriptor bd) =>
            $"{FormatBackupDescriptorVersion(bd)} | " +
            $"{FormatBackupDescriptorDate(bd)} | " +
            $"{FormatBackupDescriptorId(bd)}";

        public static string FormatBackupDescriptorId(BackupDescriptor bd) => bd.id.ToString();
        public static string FormatBackupDescriptorVersion(BackupDescriptor bd) => bd.version.ToString();

        public static string FormatBackupDescriptorDate(BackupDescriptor bd) => $"{bd.date.Day:D2}.{bd.date.Month:D2}.{bd.date.Year:D4}";
    }
}