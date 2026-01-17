using System.IO;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Networking;

namespace Flarial.Launcher.Management;

static class Sponsorship
{
    internal const string CampaignUri = "https://litebyte.co/minecraft?utm_source=flarial-client&utm_medium=app&utm_campaign=bedrock-launch";

    const string BannerUri = "https://litebyte.co/images/flarial.png";

    internal static async Task<MemoryStream?> BannerAsync()
    {
        try { return new(await HttpService.BytesAsync(BannerUri)); }
        catch { return null; }
    }
}