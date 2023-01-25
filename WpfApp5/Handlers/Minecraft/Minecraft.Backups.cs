using System;

namespace Flarial.Launcher;

public static partial class Minecraft
{
    public static class Backups
    {
        public record BackupDescriptor(string Name, Guid Id, Version Version, DateTime Date);

        public static BackupDescriptor CreateBackupDescriptor(string name, Guid id) => new BackupDescriptor
        (
            Name: name,
            Id: id,
            Version: GetVersion(),
            Date: DateTime.Now
        );

        public static string FormatBackupDescriptor(BackupDescriptor bd) =>
            $"{FormatBackupDescriptorVersion(bd)} | " +
            $"{FormatBackupDescriptorDate(bd)} | " +
            $"{FormatBackupDescriptorId(bd)}";

        public static string FormatBackupDescriptorId(BackupDescriptor bd) => bd.Id.ToString();
        public static string FormatBackupDescriptorVersion(BackupDescriptor bd) => bd.Version.ToString();

        public static string FormatBackupDescriptorDate(BackupDescriptor bd)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            return today == DateOnly.FromDateTime(bd.Date)
                ? $"Today {bd.Date.Hour:D2}:{bd.Date.Minute:D2}"
                : $"{bd.Date.Day:D2}.{bd.Date.Month:D2}.{bd.Date.Year:D4} ";
        }
    }
}