using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Flarial.Launcher.Functions;

public class AutoUpdater
{
    private readonly string _latestJsonUrl;
    private readonly string _updaterPath;
    private readonly string currentVersion = "2.1";

    public AutoUpdater(string latestJsonUrl, string updaterPath)
    {
        _latestJsonUrl = latestJsonUrl;
        _updaterPath = updaterPath;
    }

    public async Task CheckForUpdates()
    {
        try
        {
            
            using var client = new HttpClient();
            string json = await client.GetStringAsync(_latestJsonUrl);
            var latestInfo = JsonSerializer.Deserialize<LatestInfo>(json);
            Trace.WriteLine($"Latest version: {json}");
            Version latestVersion = new Version(latestInfo.version);
            Version currVersion = new Version(currentVersion);

            if (latestVersion > currVersion)
            {
                InitiateUpdate(latestInfo.downloadUrl);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Update check failed: {ex.Message}");
        }
    }

    private void InitiateUpdate(string downloadUrl)
    {
        
        Trace.WriteLine($"Downloading latest version: {downloadUrl}");
        int appPid = Process.GetCurrentProcess().Id;
        string batPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater.bat");

        // Create the BAT file if it doesn't exist
        if (!File.Exists(batPath))
        {
            File.WriteAllText(batPath, @"
@echo off
title Flarial Updater
setlocal EnableDelayedExpansion

set ""downloadUrl=%~1""
set ""appPid=%~2""
set ""appPath=%~3""
set ""tempPath=%TEMP%\updated_app.exe""

:: Use PowerShell to print colored messages
powershell -NoProfile -Command ""Write-Host 'Downloading update...' -ForegroundColor Cyan""
curl.exe -L -s ""%downloadUrl%"" -o ""%tempPath%""
powershell -NoProfile -Command ""Write-Host 'Download complete (100%)!' -ForegroundColor Cyan""

if not exist ""%tempPath%"" (
    powershell -NoProfile -Command ""Write-Host 'Download failed!' -ForegroundColor Red""
    set /p dummy=""Press enter to exit: ""
    exit /b 1
)

powershell -NoProfile -Command ""Write-Host 'Killing the current application...' -ForegroundColor Yellow""
taskkill /f /PID %appPid%

timeout /t 2 /nobreak >nul

powershell -NoProfile -Command ""Write-Host 'Replacing the application...' -ForegroundColor Blue""
move /y ""%tempPath%"" ""%appPath%""

powershell -NoProfile -Command ""Write-Host 'Starting the updated application...' -ForegroundColor Green""
start """" ""%appPath%""

set /p dummy=""Press enter to exit: ""
endlocal



");
        }

        // Launch the BAT file with arguments
        Process.Start(new ProcessStartInfo
        {
            FileName = batPath,
            Arguments = $"\"{downloadUrl}\" {appPid} \"{Process.GetCurrentProcess().MainModule.FileName}\"",
            UseShellExecute = true,
            CreateNoWindow = false
        }).Dispose();
    }
}

public class LatestInfo
{
    public string version { get; set; }
    public string downloadUrl { get; set; }
}