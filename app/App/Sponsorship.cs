using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Media.Imaging.BitmapCreateOptions;
using static System.Windows.Media.Imaging.BitmapCacheOption;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace Flarial.Launcher.App;

static class Sponsorship
{
    const string CampaignUri = "https://litebyte.co/minecraft?utm_source=flarial-client&utm_medium=app&utm_campaign=bedrock-launch";

    const string BannerUri = "https://litebyte.co/images/flarial.png";

    static readonly HttpClient s_client = new();

    static async Task<ImageSource?> GetSourceAsync()
    {
        try
        {
            using MemoryStream stream = new(await s_client.GetByteArrayAsync(BannerUri));
            return BitmapFrame.Create(stream, PreservePixelFormat, OnLoad);
        }
        catch { return null; }
    }

    internal static async Task<Image?> GetImageAsync()
    {
        var source = await GetSourceAsync();
        if (source is null) return null;

        Image image = new()
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center,
            Height = 50,
            Width = 320,
            Margin = new(0, 0, 0, 12),
            Cursor = Cursors.Hand,
            Source = source
        };

        image.MouseLeftButtonDown += (_, _) =>
        {
            try
            {
                using (Process.Start(CampaignUri)) { }
            }
            catch { }
        };

        return image;
    }
}