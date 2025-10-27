namespace Flarial.Launcher.SDK;

public static  class Developer
{
    public static  bool Enabled => Native.CheckDeveloperLicense(out _) == default;

    public static  void Request() => Native.RemoveDeveloperLicense(default);
}