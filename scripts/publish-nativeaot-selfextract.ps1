param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$launcherProject = Join-Path $root "src\Flarial.Launcher\Flarial.Launcher.csproj"
$selfExtractProject = Join-Path $root "src\Flarial.SelfExtract\Flarial.SelfExtract.csproj"
$launcherPublish = Join-Path $root "src\Flarial.Launcher\bin\$Configuration\$RuntimeIdentifier\publish"
$payload = Join-Path $root "src\Flarial.SelfExtract\payload.zip"
$artifactDir = Join-Path $root "artifacts"
$artifact = Join-Path $artifactDir "Flarial.Launcher.SelfExtract.exe"

$sources = @(
    "https://api.nuget.org/v3/index.json",
    "C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\"
)

dotnet publish $launcherProject `
    -c $Configuration `
    -r $RuntimeIdentifier `
    -p:PublishAot=true `
    -p:PublishSingleFile=true `
    --source $sources[0] `
    --source $sources[1]

if (Test-Path $payload) {
    Remove-Item -LiteralPath $payload -Force
}

Compress-Archive `
    -Path (Join-Path $launcherPublish "*") `
    -DestinationPath $payload `
    -CompressionLevel Optimal

dotnet publish $selfExtractProject `
    -c $Configuration `
    -r $RuntimeIdentifier `
    --source $sources[0] `
    --source $sources[1]

New-Item -ItemType Directory -Force $artifactDir | Out-Null
$selfExtractExe = Join-Path $root "src\Flarial.SelfExtract\bin\$Configuration\net10.0-windows10.0.19041.0\$RuntimeIdentifier\publish\Flarial.SelfExtract.exe"
Copy-Item -LiteralPath $selfExtractExe -Destination $artifact -Force

Get-Item $artifact | Select-Object FullName, @{Name = "MB"; Expression = { [math]::Round($_.Length / 1MB, 2) } }
