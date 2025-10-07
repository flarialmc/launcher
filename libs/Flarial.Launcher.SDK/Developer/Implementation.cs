namespace Flarial.Launcher.SDK;

public static partial class Developer
{
    public static partial bool Enabled => Native.CheckDeveloperLicense(out _) == default;

    public static partial void Request() => Native.RemoveDeveloperLicense(default);
}