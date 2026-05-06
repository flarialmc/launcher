using System;
using System.IO;
using System.Linq;
using Flarial.Runtime.Game;

namespace Flarial.Launcher.Utilities;

static class LogExporter
{
    static readonly string[] s_logPatterns = ["*.txt", "*.log"];
    static readonly string[] s_crashPatterns = ["*.dmp", "*.crash"];

    static readonly string[] s_knownLogDirs = [
        "logs",
        "LocalState/logs",
        "LocalState/games/com.mojang/logs"
    ];

    static readonly string[] s_knownCrashDirs = [
        "crashes",
        "LocalState/crashes",
        "LocalState/games/com.mojang/crashes"
    ];

    internal static string Export(string destinationBase)
    {
        if (!Minecraft.IsInstalled)
            throw new InvalidOperationException("Minecraft is not installed.");

        var packagePath = Minecraft.Package.InstalledPath;
        var exportPath = Path.Combine(destinationBase, $"FlarialLogs_{DateTime.Now:yyyyMMdd_HHmmss}");

        Directory.CreateDirectory(exportPath);
        var logsPath = Path.Combine(exportPath, "logs");
        var crashesPath = Path.Combine(exportPath, "crashes");

        var collected = 0;

        foreach (var dir in s_knownLogDirs)
        {
            var fullPath = Path.Combine(packagePath, dir);
            if (!Directory.Exists(fullPath)) continue;
            collected += CopyFiles(fullPath, logsPath, s_logPatterns);
        }

        foreach (var dir in s_knownCrashDirs)
        {
            var fullPath = Path.Combine(packagePath, dir);
            if (!Directory.Exists(fullPath)) continue;
            collected += CopyFiles(fullPath, crashesPath, s_crashPatterns);
        }

        if (collected == 0)
        {
            Directory.Delete(exportPath, true);
            throw new InvalidOperationException("No log or crash files found.");
        }

        return exportPath;
    }

    static int CopyFiles(string sourceDir, string destDir, string[] patterns)
    {
        int count = 0;

        try
        {
            var files = patterns
                .SelectMany(pattern => Directory.GetFiles(sourceDir, pattern, SearchOption.TopDirectoryOnly))
                .Distinct();

            Directory.CreateDirectory(destDir);

            foreach (var file in files)
            {
                var dest = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
                count++;
            }
        }
        catch { }

        return count;
    }
}
