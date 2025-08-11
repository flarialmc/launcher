using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Flarial.Launcher.Functions;

static class Sponsors
{
    readonly static HttpClient _httpClient = new();

    internal const string LiteByteCampaignUri = "https://litebyte.co/minecraft?utm_source=flarial-client&utm_medium=app&utm_campaign=bedrock-launch";

    const string LiteByteCampaignBannerUri = "https://litebyte.co/images/flarial.png";

    internal static async Task<ImageSource> GetLiteByteCampaignBanner()
    {
        try
        {
            MemoryStream stream = new(await _httpClient.GetByteArrayAsync(LiteByteCampaignBannerUri));
            var source = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            source.Freeze(); return source;
        }
        catch { return null; }
    }
}