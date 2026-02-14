using System.Drawing;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Flarial.Launcher;

static class EmbeddedResources
{
    static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    internal static ImageSource GetImageSource(string name)
    {
        using var stream = _assembly.GetManifestResourceStream(name);
        return BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
    }

    internal static Icon GetIcon(string name)
    {
        using var stream = _assembly.GetManifestResourceStream(name);
        return new(stream);
    }
}