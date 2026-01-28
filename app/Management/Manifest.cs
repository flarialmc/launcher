using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Flarial.Launcher.Management;

static class Manifest
{
    static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

    internal static readonly ImageSource Icon;

    internal static readonly string Version;

    static Manifest()
    {
        Version = $"{Assembly.GetName().Version}";
        using var stream = Assembly.GetManifestResourceStream("Application.ico");
        Icon = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
    }
}