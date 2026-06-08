using Flarial.Runtime.Services;

namespace Flarial.Runtime.Game;

public static class GamingServices
{
    const string PackageFamilyName = "Microsoft.GamingServices_8wekyb3d8bbwe";
    
    public static bool IsInstalled => PackageService.Get(PackageFamilyName) is { };
}