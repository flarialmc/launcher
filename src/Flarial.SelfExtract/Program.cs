using System.Diagnostics;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Flarial.SelfExtract;

static class Program
{
    const string ResourceName = "payload.zip";
    const string EntryPoint = "Flarial.Launcher.exe";

    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            using var payload = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
                ?? throw new FileNotFoundException("Embedded launcher payload was not found.", ResourceName);

            var extractRoot = GetExtractRoot(payload);
            var marker = Path.Combine(extractRoot, ".complete");
            var launcher = Path.Combine(extractRoot, EntryPoint);

            if (!File.Exists(marker) || !File.Exists(launcher))
            {
                Directory.CreateDirectory(extractRoot);
                ClearDirectory(extractRoot);

                payload.Position = 0;
                using var archive = new ZipArchive(payload, ZipArchiveMode.Read, leaveOpen: false);
                archive.ExtractToDirectory(extractRoot, overwriteFiles: true);
                File.WriteAllText(marker, DateTimeOffset.UtcNow.ToString("O"));
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = launcher,
                WorkingDirectory = extractRoot,
                UseShellExecute = false,
                Arguments = string.Join(" ", args.Select(QuoteArgument))
            });

            return 0;
        }
        catch (Exception exception)
        {
            File.WriteAllText(
                Path.Combine(Path.GetTempPath(), "Flarial.SelfExtract.log"),
                exception.ToString());
            return 1;
        }
    }

    static string GetExtractRoot(Stream payload)
    {
        var hash = SHA256.HashData(payload);
        var version = Convert.ToHexString(hash, 0, 8);
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Flarial",
            "Launcher",
            "SelfExtract",
            version);
    }

    static void ClearDirectory(string path)
    {
        foreach (var file in Directory.EnumerateFiles(path))
            File.Delete(file);

        foreach (var directory in Directory.EnumerateDirectories(path))
            Directory.Delete(directory, recursive: true);
    }

    static string QuoteArgument(string value)
    {
        if (value.Length == 0)
            return "\"\"";

        if (!value.Any(char.IsWhiteSpace) && !value.Contains('"'))
            return value;

        return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
