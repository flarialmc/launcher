using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

static class EmbeddedResources
{
    static readonly Assembly s_assembly = Assembly.GetExecutingAssembly();

    internal static ImageSource GetImage(string name)
    {
        using var stream = s_assembly.GetManifestResourceStream(name);
        return BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
    }
}