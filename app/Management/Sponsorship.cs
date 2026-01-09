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
using Flarial.Launcher.Services.Networking;
using System.Windows.Interop;
using static Flarial.Launcher.PInvoke;

namespace Flarial.Launcher.Management;

static class Sponsorship
{
    internal const string CampaignUri = "https://litebyte.co/minecraft?utm_source=flarial-client&utm_medium=app&utm_campaign=bedrock-launch";

    const string BannerUri = "https://litebyte.co/images/flarial.png";

    internal static async Task<MemoryStream?> StreamAsync()
    {
        try { return new(await HttpService.BytesAsync(BannerUri)); }
        catch { return null; }
    }
}