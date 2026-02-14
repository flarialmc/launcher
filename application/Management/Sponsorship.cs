using System;
using System.IO;
using System.Threading.Tasks;
using Flarial.Launcher.Runtime.Networking;

namespace Flarial.Launcher.Management;

abstract class Sponsorship
{
    internal Sponsorship() { }
    protected abstract string BannerUrl { get; }
    protected abstract string CampaignUrl { get; }

    protected async virtual Task<Tuple<Stream, string>?> GetAsync()
    {
        try { return new(new MemoryStream(await HttpService.GetBytesAsync(BannerUrl)), CampaignUrl); }
        catch { return null; }
    }

    internal static async Task<Tuple<Stream, string>?> GetAsync<T>() where T : Sponsorship, new() => await new T().GetAsync();

    /*
        - Register & store sponsorships in the launcher.
        - We can easily swap these out as required.
    */

    internal sealed class LiteByteHosting : Sponsorship
    {
        protected override string BannerUrl => "https://litebyte.co/images/flarial.png";
        protected override string CampaignUrl => "https://litebyte.co/minecraft?utm_source=flarial-client&utm_medium=app&utm_campaign=bedrock-launch";
    }

    internal sealed class CollapseNetwork : Sponsorship
    {
        protected override string CampaignUrl => "minecraft://?addExternalServer=Collapse|clps.gg:19132";
        protected override string BannerUrl => "https://collapsemc.com/assets/other/ad-banner.png";
    }

    internal class InfinityNetwork : Sponsorship
    {
        protected override string BannerUrl => "https://assets.infinitymcpe.com/banner.png";
        protected override string CampaignUrl => "https://discord.gg/infinitymcpe";
    }

    internal sealed class Empty : Sponsorship
    {
        /*
            - Used when a sponsorship spot is available.
        */

        protected override string BannerUrl => string.Empty;
        protected override string CampaignUrl => string.Empty;
        protected override Task<Tuple<Stream, string>?> GetAsync() => null!;
    }
}